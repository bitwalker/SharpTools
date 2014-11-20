using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Metadata.Edm;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity.Core.Mapping;
using System.Globalization;
using System.Data.Entity.Core.Objects;
using SharpTools.Testing.EntityFramework.Internal;

namespace SharpTools.Testing.EntityFramework
{
    public sealed class InMemoryDbSet<TEntity> : DbSet, IDbSet<TEntity>, IDbAsyncEnumerable<TEntity>
        where TEntity : class
    {
        private readonly Type _parentContextType;
        private readonly ObservableCollection<TEntity> _data;
        private readonly IQueryable _query;
        private readonly PrimaryKeyInfo[] _primaryKeys;
        private readonly PropertyInfo[] _primaryKeyProps;

        public InMemoryDbSet()
        {
            _parentContextType = FindContextTypeForEntity(typeof (TEntity));
            _data  = new ObservableCollection<TEntity>();
            // Handle inserts like SQL Server (by setting the id)
            _query = _data.AsQueryable();

            // Cache reflected metadata for this entity, to speed
            // up lookups for functions such as Find
            _primaryKeys     = GetPrimaryKeys();
            _primaryKeyProps = typeof (TEntity)
                .GetProperties()
                .Where(p => _primaryKeys.Any(pk => pk.Name == p.Name))
                .ToArray();
        }

        public override object Find(params object[] keyValues)
        {
            return FindInternal(keyValues);
        }

        TEntity IDbSet<TEntity>.Find(params object[] keyValues)
        {
            return FindInternal(keyValues);
        }

        private TEntity FindInternal(params object[] keyValues)
        {
            var querySource = this.AsQueryable();
            for (var i = 0; i < keyValues.Length && i < _primaryKeys.Length; i++)
            {
                var pk       = _primaryKeyProps[i];
                var keyValue = keyValues[i];
                querySource  = querySource.Where(e => pk.GetValue(e).Equals(keyValue));
            }
            return querySource.SingleOrDefault();
        }

        public override async Task<object> FindAsync(params object[] keyValues)
        {
            var querySource = this.AsQueryable();
            for (var i = 0; i < keyValues.Length && i < _primaryKeys.Length; i++)
            {
                var pk       = _primaryKeyProps[i];
                var keyValue = keyValues[i];
                querySource  = querySource.Where(e => pk.GetValue(e).Equals(keyValue));
            }
            return await querySource.SingleOrDefaultAsync();
        }

        public override Task<object> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return Task.Run(() => { return Find(keyValues); }, cancellationToken);
        }

        TEntity IDbSet<TEntity>.Add(TEntity item)
        {
            // Generate and set all of the primary key values for this item
            foreach (var primaryKey in _primaryKeys)
            {
                var keyGenerator = IdentifierGeneratorFactory.Create(primaryKey);
                var keyProperty  = _primaryKeyProps.First(p => p.Name == primaryKey.Name);
                var key          = keyGenerator.Generate();
                keyProperty.SetValue(item, key);
            }

            _data.Add(item);
            return item;
        }

        TEntity IDbSet<TEntity>.Remove(TEntity item)
        {
            _data.Remove(item);
            return item;
        }

        TEntity IDbSet<TEntity>.Attach(TEntity item)
        {
            _data.Add(item);
            return item;
        }

        TEntity IDbSet<TEntity>.Create()
        {
            return Activator.CreateInstance<TEntity>();
        }

        TDerivedEntity IDbSet<TEntity>.Create<TDerivedEntity>()
        {
            return Activator.CreateInstance<TDerivedEntity>();
        }

        ObservableCollection<TEntity> IDbSet<TEntity>.Local
        {
            get { return _data; }
        }

        Type IQueryable.ElementType
        {
            get { return _query.ElementType; }
        }

        Expression IQueryable.Expression
        {
            get { return _query.Expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return new InMemoryDbAsyncQueryProvider<TEntity>(_query.Provider); }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IDbAsyncEnumerator<TEntity> IDbAsyncEnumerable<TEntity>.GetAsyncEnumerator()
        {
            return new InMemoryDbAsyncEnumerator<TEntity>(_data.GetEnumerator());
        }

        private PrimaryKeyInfo[] GetPrimaryKeys()
        {
            var entityType = typeof (TEntity);
            var dbSetType = typeof (IDbSet<>);
            Func<PropertyInfo, bool> isDbSet = p => dbSetType.IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition());

            var workspace = new MetadataWorkspace(new[] { "res://*/" }, new[] { _parentContextType.Assembly });
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<TEntity>();
            var dbModel = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            return dbModel.StoreModel
                .EntityTypes
                .Where(et => et.Name == entityType.Name)
                .SelectMany(PrimaryKeyInfo.Map)
                .ToArray();
        }

        private static Type FindContextTypeForEntity(Type entityType)
        {
            var dbContextType = typeof(DbContext);
            var setType = typeof(IDbSet<>);

            Func<Type, bool> isEntityType          = t => entityType.Equals(t);
            Func<PropertyInfo, bool> isDbSet       = p => setType.IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition());
            Func<PropertyInfo, bool> isEntityDbSet = p => isDbSet(p) && isEntityType(p.PropertyType.GenericTypeArguments[0]);
            Func<Type, bool> isHostContext         = t => t.GetProperties().Any(isEntityDbSet);

            // Assumption: Models and context are defined in same assembly
            return entityType.Assembly
                .GetTypes()
                .Where(dbContextType.IsAssignableFrom)
                .Single(isHostContext);
        }
    }
}
