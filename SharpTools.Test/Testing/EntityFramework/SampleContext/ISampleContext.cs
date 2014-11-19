using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;

namespace SharpTools.Test.Testing.EntityFramework.SampleContext
{
    public interface ISampleContext : IDisposable
    {
        IDbSet<User> Users { get; set; }
        IDbSet<Role> Roles { get; set; }
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        DbSet Set(Type entityType);
        int SaveChanges();
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        IEnumerable<DbEntityValidationResult> GetValidationErrors();
        DbEntityEntry Entry<TEntity>(TEntity entity) where TEntity : class;
        DbEntityEntry Entry(object entity);
        int ExecuteSqlCommand(string sql, params object[] parameters);
        int ExecuteSqlCommand(TransactionalBehavior behavior, string sql, params object[] parameters);
        Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters);
        Task<int> ExecuteSqlCommandAsync(TransactionalBehavior behavior, string sql, params object[] parameters);
    }
}
