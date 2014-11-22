using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTools.Test.Testing.EntityFramework.SampleContext;

namespace SharpTools.Test.Testing.EntityFramework
{
    [TestClass]
    public class DbContextMockingTest
    {
        private ISampleContext _context;

        [TestInitialize]
        public void Setup()
        {
            _context = SampleContextMockFactory.Create();

            var role = new Role()
            {
                Name = "Admins"
            };

            _context.Roles.Add(role);

            var user = new User()
            {
                FirstName = "Paul",
                LastName = "Schoenfelder",
                Email = "test@example.com",
                HashedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("password")),
                UserRole = role
            };

            _context.Users.Add(user);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
            _context = null;
        }

        [TestMethod]
        public void CanQuery()
        {
            var admins = _context.Roles.FirstOrDefault(r => r.Name == "Admins");
            Assert.IsNotNull(admins);
            Assert.AreEqual("Admins", admins.Name);

            var byId = _context.Users.Where(u => u.UserRole.Id == admins.Id).ToArray();
            Assert.IsNotNull(byId);
            Assert.IsTrue(byId.Any());
            Assert.AreEqual(byId.Count(), 1);
            Assert.AreEqual("Paul", byId.First().FirstName);
        }

        [TestMethod]
        public void CanGenerateIdentifiers()
        {
            var john = new User()
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "jsmith@example.com",
                UserName = "jsmith",
                HashedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("password"))
            };

            _context.Users.Add(john);

            var retrieved = _context.Users.FirstOrDefault(u => u.Email == john.Email);
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(john.Id, retrieved.Id);
            Assert.IsTrue(john.Id > 1);
        }

        [TestMethod]
        public void CanQueryRelations()
        {
            var admins = _context.Roles.FirstOrDefault(r => r.Name == "Admins");
            var bill = new User()
            {
                FirstName = "Bill",
                LastName = "Bo",
                Email = "bilbo@example.com",
                UserName = "bilbo",
                HashedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("password")),
                UserRole = admins
            };

            _context.Users.Add(bill);

            var adminUsers = from r in _context.Roles
                             join u in _context.Users on r.Id equals u.UserRole.Id
                             where r.Name == "Admins"
                             select u.Id;

            Assert.IsTrue(adminUsers.ToArray().Contains(bill.Id));

            // TODO: Make sure relations are linked on Add
            var usingLinq = _context.Roles.Where(r => r.Name == "Admins")
                    .SelectMany(r => r.Users)
                    .Select(u => u.Id)
                    .ToArray();

            Assert.IsTrue(usingLinq.Contains(bill.Id));
        }
    }
}
