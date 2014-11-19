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
            var adminUsers = _context.Users.Where(u => u.UserRole.Id == 1).ToArray();
            Assert.IsNotNull(adminUsers);
            Assert.IsTrue(adminUsers.Any());
            Assert.AreEqual(adminUsers.Count(), 1);
            Assert.AreEqual("Paul", adminUsers.First().FirstName);
        }
    }
}
