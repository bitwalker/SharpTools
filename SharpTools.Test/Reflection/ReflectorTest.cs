using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTools.Reflection;

namespace SharpTools.Test.Reflection
{
    [TestClass]
    public class ReflectorTest
    {
        [TestMethod]
        public void GetCallerInfo()
        {
            var test = new Test();
            test.Main("blah", "blah");
        }

        [TestMethod]
        public void ToSimpleTypeString()
        {
            var typeString = Reflector.ToSimpleTypeString(typeof (Test));
            Assert.AreEqual("SharpTools.Test.Reflection.ReflectorTest+Test, SharpTools.Test", typeString);
        }

        [TestMethod]
        public void ResolveTypeString()
        {
            var typeString = Reflector.ToSimpleTypeString(typeof (Test));
            var instance   = Reflector.ResolveTypeString(typeString) as Test;

            Assert.IsNotNull(instance);
            Assert.AreEqual("Testing", instance.Name);
        }

        [TestMethod]
        public void GetConcreteTypes()
        {
            var types = Reflector.GetConcreteTypes(Assembly.GetAssembly(typeof (Test)));
            Assert.IsTrue(types.Contains(typeof(Test)));
            Assert.IsFalse(types.Contains(typeof(ITest)));
        }

        [TestMethod]
        public void GetImplementationTypes()
        {
            var types = Reflector.GetImplementationTypes(typeof(ITest)).ToArray();
            Assert.IsTrue(types.Any(t => t.ConcreteType.Equals(typeof(Test))));
            Assert.IsFalse(types.Any(t => t.ConcreteType.Equals(typeof(ReflectorTest))));
            Assert.IsFalse(types.Any(t => t.ConcreteType.Equals(typeof(ITest))));

            var concreteType = types.FirstOrDefault(t => t.ConcreteType.Equals(typeof (Test)));
            Assert.AreEqual(typeof(ITest), concreteType.InterfaceType);
            Assert.IsFalse(concreteType.TypeParameters.Any());
        }

        [TestMethod]
        public void Stringify()
        {
            var main = Reflector.Stringify(typeof (Test).GetMethod("Main"));
            Assert.AreEqual("public virtual void Main(params String[] args)", main);

            var verifyCaller = Reflector.Stringify(typeof (Test).GetMethod("VerifyCaller"));
            Assert.AreEqual("public IEnumerable<String> VerifyCaller(String[] args)", verifyCaller);
        }

        [TestMethod]
        public void GetProperty()
        {
            var propInfo = Reflector.GetProperty<Test>(t => t.Name);
            Assert.IsNotNull(propInfo);
            Assert.AreEqual("Name", propInfo.Name);
            Assert.AreEqual(typeof(string), propInfo.PropertyType);
        }

        [TestMethod]
        public void GetMember()
        {
            var memberInfo = Reflector.GetMember<Test>(t => t.VerifyCaller(null));
            Assert.IsNotNull(memberInfo);
            Assert.AreEqual("VerifyCaller", memberInfo.Name);
            Assert.AreEqual(MemberTypes.Method, memberInfo.MemberType);
        }

        public interface ITest
        {
            string Name { get; set; }
        }

        public class Test : ITest
        {
            public string Name { get; set; }

            public Test()
            {
                Name = "Testing";
            }

            public virtual void Main(params string[] args)
            {
                VerifyCaller(args);
            }

            public IEnumerable<string> VerifyCaller(string[] args)
            {
                var callerInfo = Reflector.GetCallerInfo();
                Assert.AreEqual(typeof(Test), callerInfo.CallerType);
                Assert.AreEqual("Test", callerInfo.Class);
                Assert.AreEqual("Main", callerInfo.Method);
                Assert.AreEqual("public virtual void Main(params String[] args)", callerInfo.Signature);

                return args;
            }
        }
    }
}
