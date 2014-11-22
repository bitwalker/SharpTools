using System;
using Moq;

using SharpTools.Testing.EntityFramework;
using SharpTools.Test.Testing.EntityFramework.SampleContext;

namespace SharpTools.Test.Testing.EntityFramework
{
    public class SampleContextMockFactory
    {
        public static ISampleContext Create()
        {
            var contextMock = new Mock<ISampleContext>();
            contextMock.SetupAllProperties();
            contextMock.DefaultValue = DefaultValue.Mock;

            var userSet = new InMemoryDbSet<User>(contextMock.Object);
            var roleSet = new InMemoryDbSet<Role>(contextMock.Object);
            contextMock.Setup(ctx => ctx.Users).Returns(userSet);
            contextMock.Setup(ctx => ctx.Roles).Returns(roleSet);
            contextMock.Setup(ctx => ctx.Set(It.Is<Type>(t => typeof (User).Equals(t)))).Returns(userSet);
            contextMock.Setup(ctx => ctx.Set(It.Is<Type>(t => typeof (Role).Equals(t)))).Returns(roleSet);

            return contextMock.Object;
        }
    }
}
