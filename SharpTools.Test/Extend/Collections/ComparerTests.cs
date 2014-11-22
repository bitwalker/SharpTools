using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTools.Extend.Collections;
using SharpTools.Reflection;

namespace SharpTools.Test.Extend.Collections
{
    [TestClass]
    public class ComparerTests
    {
        [TestMethod]
        public void GenericComparer_Compare()
        {
            var descendingLastNames = GenericComparer<Person>.Create(p => p.LastName, false);
            var ascendingFirstNames = GenericComparer<Person>.Create(p => p.FirstName, true);

            var people = new List<Person>();
            people.Add(new Person("Alex", "Peters"));
            people.Add(new Person("Peter", "Smith"));
            people.Add(new Person("Paul", "Schoenfelder"));
            people.Add(new Person("Alex", "Smith"));

            people.Sort(descendingLastNames);

            var expected = new[] {"Smith", "Smith", "Schoenfelder", "Peters"};
            var result = people.Select(p => p.LastName).ToArray();
            Assert.IsTrue(expected.SequenceEqual(result));

            people.Sort(ascendingFirstNames);
            expected = new[] {"Alex", "Alex", "Paul", "Peter"};
            result = people.Select(p => p.FirstName).ToArray();
            Assert.IsTrue(expected.SequenceEqual(result));
        }

        [TestMethod]
        public void MultiComparer_Compare()
        {
            // Sort first by ascending first names, then by descending last name
            var ascFirstNames = new GenericComparer<Person, string>(p => p.FirstName, true);
            var descLastNames = new GenericComparer<Person, string>(p => p.LastName, false);
            var comparer = new MultiComparer<Person>(ascFirstNames, descLastNames);

            var people = new List<Person>();
            people.Add(new Person("Alex", "Peters"));
            people.Add(new Person("Peter", "Smith"));
            people.Add(new Person("Paul", "Schoenfelder"));
            people.Add(new Person("Alex", "Smith"));

            people.Sort(comparer);

            var expected = new[] {"Alex Smith", "Alex Peters", "Paul Schoenfelder", "Peter Smith"};
            var result = people.Select(p => string.Format("{0} {1}", p.FirstName, p.LastName)).ToArray();
            Assert.IsTrue(expected.SequenceEqual(result));
        }

        [TestMethod]
        public void MultiComparer_Helpers()
        {
            // Sort (asc order) by first name, then by last name
            var comparer = MultiComparer<Person>.Create(p => p.FirstName, p => p.LastName);

            var people = new List<Person>();
            people.Add(new Person("Alex", "Peters"));
            people.Add(new Person("Peter", "Smith"));
            people.Add(new Person("Paul", "Schoenfelder"));
            people.Add(new Person("Alex", "Smith"));

            people.Sort(comparer);

            var expected = new[] {"Alex Peters", "Alex Smith", "Paul Schoenfelder", "Peter Smith"};
            var result = people.Select(p => string.Format("{0} {1}", p.FirstName, p.LastName)).ToArray();
            Assert.IsTrue(expected.SequenceEqual(result));
        }

        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public Person(string first, string last)
            {
                FirstName = first;
                LastName = last;
            }
        }
    }
}
