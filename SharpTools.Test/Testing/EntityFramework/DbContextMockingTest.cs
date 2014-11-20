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
        public void CanMockDbContextEffectively()
        {
            var admins = _context.Roles.First(r => r.Name == "Admins");
            var byName = _context.Users.Where(u => u.UserRole.Name == admins.Name).ToArray();
            var byId = _context.Users.Where(u => u.UserRole.Id == admins.Id).ToArray();
            Assert.IsNotNull(byName);
            Assert.IsNotNull(byId);
            Assert.IsTrue(byName.Any());
            Assert.IsTrue(byId.Any());
            Assert.AreEqual(byName.Count(), 1);
            Assert.AreEqual(byId.Count(), 1);
            Assert.AreEqual("Paul", byName.First().FirstName);
            Assert.AreEqual("Paul", byId.First().FirstName);
        }
    }
}
