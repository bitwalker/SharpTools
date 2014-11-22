using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;

namespace SharpTools.Database.EntityFramework
{
    public class BaseDbContext : DbContext, IDbContext
    {
        public BaseDbContext() : base() {}
        public BaseDbContext(string nameOrConnectionString) : base(nameOrConnectionString) {}
        public BaseDbContext(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model) {}
        public BaseDbContext(DbCompiledModel model) : base(model) {}
        public BaseDbContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection)
            : base(existingConnection, model, contextOwnsConnection) {}
        public BaseDbContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection) {}
        public BaseDbContext(ObjectContext objectContext, bool dbContextOwnsObjectContext)
            : base(objectContext, dbContextOwnsObjectContext) {}

        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return base.Database.ExecuteSqlCommand(sql, parameters);
        }

        public int ExecuteSqlCommand(TransactionalBehavior behavior, string sql, params object[] parameters)
        {
            return base.Database.ExecuteSqlCommand(behavior, sql, parameters);
        }

        public Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
        {
            return base.Database.ExecuteSqlCommandAsync(sql, parameters);
        }

        public Task<int> ExecuteSqlCommandAsync(TransactionalBehavior behavior, string sql, params object[] parameters)
        {
            return base.Database.ExecuteSqlCommandAsync(behavior, sql, parameters);
        }
    }
}
