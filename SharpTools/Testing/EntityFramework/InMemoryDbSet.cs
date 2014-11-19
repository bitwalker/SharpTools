using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Metadata.Edm;
using System.Threading;
using System.Threading.Tasks;

namespace SharpTools.Testing.EntityFramework
{
    public sealed class InMemoryDbSet<TEntity, TKey> : DbSet, IDbSet<TEntity>, IDbAsyncEnumerable<TEntity>
        where TEntity : class
    {
        private readonly ObservableCollection<TEntity> _data;
        private readonly IQueryable _query;
        private readonly PropertyInfo[] _primaryKeys;
        private string _entityIdProperty = string.Format("{0}ID", typeof (TEntity).Name);

        public InMemoryDbSet()
        {
            _data  = new ObservableCollection<TEntity>();
            _query = _data.AsQueryable();

            // Cache reflected metadata for this entity, to speed
            // up lookups for functions such as Find
            var keyNames = GetKeyPropertyNames();
            var primaryKeys = typeof (TEntity)
                .GetProperties()
                .Where(p => keyNames.Contains(p.Name))
                .ToArray();
            _primaryKeys = primaryKeys;
        }

        public InMemoryDbSet(string primaryKeyPropertyName) : this()
        {
            _entityIdProperty = primaryKeyPropertyName;
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
                var pk       = _primaryKeys[i];
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
                var pk       = _primaryKeys[i];
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
            // Generate IDs for entities which have primary keys
            if (_primaryKeys.Length > 0 && GetItemId(item) == default(TKey))
            {
                TKey id = GetNextId<TKey>();
                item = SetItemId(item, id);
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

        private string[] GetKeyPropertyNames()
        {
            var assembly = Assembly.GetCallingAssembly();
            var workspace = new MetadataWorkspace(new[] {"res://*/"}, new [] {assembly});
            return GetKeyPropertyNames(typeof(TEntity), workspace);
        }

        private string[] GetKeyPropertyNames(Type type, MetadataWorkspace workspace)
        {
            EdmType edmType;

            if (workspace.TryGetType(type.Name, type.Namespace, DataSpace.OSpace, out edmType))
            {
                return edmType.MetadataProperties
                    .Where(mp => mp.Name == "KeyMembers")
                    .SelectMany(mp => mp.Value as ReadOnlyMetadataCollection<EdmMember>)
                    .OfType<EdmProperty>()
                    .Select(edmProperty => edmProperty.Name)
                    .ToArray();
            }

            // Test for an Id property
            if (type.GetProperty(_entityIdProperty) != null)
                return new[] {_entityIdProperty};
            // Failed, so let's try a sane default
            if (type.GetProperty("Id") != null)
                return new[] {"Id"};
            // That failed, so try {TEntity}Id
            var idProperty = string.Format("{0}ID", type.Name);
            if (type.GetProperty(idProperty) != null)
                return new[] {idProperty};
            // Still nothing? Well, nothing to do but return an empty list
            return new string[0];
        }

        private dynamic GetNextId<TKey>()
        {
            var keyType = typeof (TKey);
            if (keyType.Equals(typeof (int)) || keyType.Equals(typeof(long)))
            {
                if (_data.Any())
                    return _data.Select(GetItemId).Max() + 1;
                else return 1;
            }
            else if (keyType.Equals(typeof(Guid)))
            {
                return Guid.NewGuid();
            }
            else if (keyType.Equals(typeof (string)))
            {
                return Guid.NewGuid().ToString();
            }
            else return null;
        }

        private dynamic GetItemId(TEntity item)
        {
            var keyType = typeof (TKey);
            var primaryKey = _primaryKeys
                .Where(p =>
                    p.Name == _entityIdProperty ||
                    p.Name == "Id" ||
                    p.Name == string.Format("{0}Id", typeof (TEntity).Name)
                )
                .FirstOrDefault(p => p.PropertyType.Equals(keyType));

            // If the type of the primary key is a number type
            // Return a positive number, since the only value that matters is 0 (uncreated record)
            if (keyType.Equals(typeof(int)) || keyType.Equals(typeof(long)))
            {
                if (primaryKey == null)
                    return 1;
            }
            // Likewise, ensure guid/string types return an id of some kind
            else if (keyType.Equals(typeof(Guid)))
            {
                if (primaryKey == null)
                    return Guid.NewGuid();
            }
            else if (keyType.Equals(typeof(string)))
            {
                if (primaryKey == null)
                    return Guid.NewGuid().ToString();
            }

            return (TKey) primaryKey.GetValue(item);
        }

        private TEntity SetItemId(TEntity item, TKey id)
        {
            var primaryKey = _primaryKeys.First(p =>
                p.Name == _entityIdProperty ||
                p.Name == "Id" ||
                p.Name == string.Format("{0}Id", typeof(TEntity).Name)
            );
            primaryKey.SetValue(item, id);
            return item;
        }
    }
}
