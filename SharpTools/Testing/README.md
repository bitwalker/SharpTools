## SharpTools.Testing

This module contains helpful utilities for testing your projects.

### Current Submodules

#### EntityFramework

This module contains everything you need to make sure your data access layer 
using Entity Framework is fully unit testable. If you wish to make use of this
capability, use the following class hierarchy (nested in order of inheritance):

- SharpTools.Database.IDbContext (defines the interface for the DbContext API)
  - SharpTools.Database.BaseDbContext (implements IDbContext for you)
  - MyApp.Database.IMyAppContext (extends IDbContext with `IDbSet<T>` properties for all your models)
	- MyApp.Database.MyAppContext (implements `IMyAppContext` and `BaseDbContext`)

You will want to write your services/providers/anything which uses the context against the `IMyAppContext` interface.
When it comes time to write unit tests, you'll mock that interface, and set it up like so (using Moq here):

```csharp
public class MyAppContextMockFactory
{
	public static IMyAppContext Create()
	{
		var contextMock = new Mock<IMyAppContext>();
		contextMock.SetupAllProperties();
		contextMock.DefaultValue = DefaultValue.Mock;

		// Use an InMemoryDbSet in place of the standard DbSet for
		// each model in the IMyAppContext interface
		var userSet = new InMemoryDbSet<User>(contextMock.Object);
		var roleSet = new InMemoryDbSet<Role>(contextMock.Object);

		// Configure the mock to return the in-memory sets
		contextMock.Setup(ctx => ctx.Users).Returns(userSet);
		contextMock.Setup(ctx => ctx.Roles).Returns(roleSet);
		contextMock.Setup(ctx => ctx.Set(It.Is<Type>(t => typeof (User).Equals(t)))).Returns(userSet);
		contextMock.Setup(ctx => ctx.Set(It.Is<Type>(t => typeof (Role).Equals(t)))).Returns(roleSet);

		return contextMock.Object;
	}
}
```

Once this has been set up, you can inject the mocked implementation into your services under test, and
the mocked context will behave as close as possible to the way the real context behaves against a SQL database. Some
things like invalid LINQ expressions are not caught by this because it's backed by LINQ-to-Objects instead of LINQ-To-SQL.
What we do emulate though is database-generated primary keys, automatic wiring of foreign key relations, and the full DbSet
API.

Unfortunately it's not possible to make this behave identically to the way it would against SQL Server (at least not 
without writing something as complex as EF itself), so you'll always want to run a set of integration tests against
a real database. However, there is a great deal of code we test which does not need to validate the database
interaction, but nevertheless needs to be able to interact with the context during execution. This Testing module
will give you this ability, with virtually no extra effort on your end whatsoever.