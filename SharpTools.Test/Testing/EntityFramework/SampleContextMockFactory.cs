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
            contextMock.DefaultValue = DefaultValue.Empty;

            contextMock.Setup(ctx => ctx.Users).Returns(new InMemoryDbSet<User>());
            contextMock.Setup(ctx => ctx.Roles).Returns(new InMemoryDbSet<Role>());

            return contextMock.Object;
        }
    }
}
