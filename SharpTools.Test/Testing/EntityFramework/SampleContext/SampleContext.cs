using System.Data.Entity;
using System.Threading.Tasks;

namespace SharpTools.Test.Testing.EntityFramework.SampleContext
{
    public class SampleContext : DbContext, ISampleContext
    {
        public IDbSet<User> Users { get; set; }

        public IDbSet<Role> Roles { get; set; }

        public SampleContext()
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public new System.Data.Entity.Infrastructure.DbEntityEntry Entry<TEntity>(TEntity entity) where TEntity : class
        {
            return base.Entry(entity);
        }

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
