using System;
using System.Collections;
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

using SharpTools.Database.EntityFramework;
using SharpTools.Testing.EntityFramework.Internal;

namespace SharpTools.Testing.EntityFramework
{
    public sealed class InMemoryDbSet<TEntity> : DbSet, IDbSet<TEntity>, IDbAsyncEnumerable<TEntity>
        where TEntity : class
    {
        private const string CLR_TYPE_ANNOTATION = @"http://schemas.microsoft.com/ado/2013/11/edm/customannotation:ClrType";

        // Cache the type of the set since it's used often when loading metadata
        private static readonly Type _entityClrType = typeof (TEntity);
        private static readonly EntityType _entityType;
        // Cache the map of entity types to CLR types for fast lookups
        private static readonly IDictionary<EntityType, Type> _entityTypeMap;
        // Cache primary and foreign key information since it's used so often
        private static readonly PrimaryKeyInfo[] _primaryKeys;
        private static readonly ForeignKeyInfo[] _foreignKeys;
        private static readonly PropertyInfo[] _primaryKeyProps;

        // Used for lookups of other entities when handling foreign key relations
        private readonly IDbContext _parentContext;
        // The collection where the actual entities are stored
        private readonly ObservableCollection<TEntity> _data;
        // Cache the set's queryable handle
        private readonly IQueryable _query;

        // Metadata about this entity set can be loaded just once, hence the static ctor
        static InMemoryDbSet()
        {
            // We need the context type this entity is stored in to load it's metadata
            var contextType  = FindContextTypeForEntity(typeof (TEntity));
            // We need the database model that EF generates in order to get key information
            var metadata     = GetMetadataWorkspace(contextType);
            var dbModel      = GetDatabaseModel(metadata);
            // The entity type map allows us to translate an EntityType to it's actual CLR type
            _entityTypeMap   = GetEntityTypeMap(dbModel);
            // Since primary and foreign keys reference EntityTypes instead of CLR types, we
            // cache the EntityType for this set since it's used so often
            _entityType      = _entityTypeMap
                .Where(map => map.Value.Equals(_entityClrType))
                .Select(map => map.Key)
                .FirstOrDefault();
            // Same with the keys
            _primaryKeys     = GetPrimaryKeys(dbModel);
            _foreignKeys     = GetForeignKeys(dbModel);
            _primaryKeyProps = _entityClrType
                .GetProperties()
                .Where(p => _primaryKeys.Any(pk => pk.Name == p.Name))
                .ToArray();
        }

        public InMemoryDbSet(IDbContext context)
        {
            _parentContext = context;
            _data  = new ObservableCollection<TEntity>();
            _query = _data.AsQueryable();
        }

        #region Generic Find Implementation

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

        #endregion

        #region IDbSet<TEntity> Implementation

        TEntity IDbSet<TEntity>.Add(TEntity item)
        {
            // Generate and set all of the primary key values for this item
            foreach (var primaryKey in _primaryKeys.Where(pk => pk.IsStoreGenerated && !pk.IsComputed))
            {
                var keyProperty  = _primaryKeyProps.First(p => p.Name == primaryKey.Name);
                if (keyProperty.CanWrite)
                {
                    var keyGenerator = IdentifierGeneratorFactory.Create(primaryKey);
                    var key          = keyGenerator.Generate();
                    keyProperty.SetValue(item, key);
                }
            }

            // Detect foreign key references and update the remote entity
            foreach (var foreignKey in _foreignKeys)
            {
                switch (foreignKey.Relationship)
                {
                    case ForeignKeyInfo.RelationshipType.OneToMany:
                    case ForeignKeyInfo.RelationshipType.ManyToMany:
                        var keys = foreignKey.FromProperty.GetValue(item) as ICollection;
                        if (keys == null || keys.Count == 0)
                            continue;
                        AddRelationship(item, foreignKey, keys);
                        break;
                    case ForeignKeyInfo.RelationshipType.ManyToOne:
                    case ForeignKeyInfo.RelationshipType.OneToOne:
                        var key = foreignKey.FromProperty.GetValue(item);
                        if (key == null)
                            continue;
                        AddRelationship(item, foreignKey, key);
                        break;
                    default:
                        break;
                }
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

        #endregion

        #region IQueryable Implementation

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

        #endregion

        #region IEnumerable Implementation

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

        #endregion

        #region Metadata Functions

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

        private static PrimaryKeyInfo[] GetPrimaryKeys(DbModel dbModel)
        {
            return dbModel.StoreModel
                .EntityTypes
                .Where(e => _entityType.Equals(e))
                .SelectMany(PrimaryKeyInfo.Map)
                .ToArray();
        }

        private static ForeignKeyInfo[] GetForeignKeys(DbModel dbModel)
        {
            // Get foreign keys
            var foreignKeys = dbModel.ConceptualModel
                .EntityTypes
                .SelectMany(et => et.DeclaredNavigationProperties)
                .Where(n => n.DeclaringType.Name.Equals(_entityType.Name))
                .ToArray();

            // Get relationship PropertyInfos
            var propInfos = dbModel.ConceptualModel
                .AssociationTypes
                .SelectMany(at => at.AssociationEndMembers)
                .Select(ExtractAssociationMappings)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var keyInfos = new List<ForeignKeyInfo>();
            foreach (var foreignKey in foreignKeys)
            {
                var toProperty   = propInfos[foreignKey.ToEndMember.Name];
                var fromProperty = propInfos[foreignKey.FromEndMember.Name];
                keyInfos.Add(new ForeignKeyInfo(foreignKey, toProperty, fromProperty));
            }
            return keyInfos.ToArray();
        }

        private static KeyValuePair<string, PropertyInfo> ExtractAssociationMappings(AssociationEndMember member)
        {
            var name = member.Name;
            var propInfo = member.MetadataProperties
                .Where(mp => mp.Name.Equals("ClrPropertyInfo"))
                .Select(mp => mp.Value as PropertyInfo)
                .Single();

            return new KeyValuePair<string, PropertyInfo>(name, propInfo);
        }

        private static MetadataWorkspace GetMetadataWorkspace(Type contextType)
        {
            return new MetadataWorkspace(new[] { "res://*/" }, new[] { contextType.Assembly });
        }

        private static DbModel GetDatabaseModel(MetadataWorkspace metadata)
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<TEntity>();
            return modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
        }

        private static IDictionary<EntityType, Type> GetEntityTypeMap(DbModel databaseModel)
        {
            var entityTypes = databaseModel.StoreModel.EntityTypes.ToDictionary(e => e.FullName, e => e);

            Func<EntityType, EntityType> getEntityType = (e) =>
            {
                var entityRefNamespace  = "CodeFirstNamespace";
                var realEntityNamespace = "CodeFirstDatabaseSchema";
                var realEntityName = e.FullName.Replace(entityRefNamespace, realEntityNamespace);
                return entityTypes[realEntityName];
            };

            var map = databaseModel
                .ConceptualToStoreMapping
                .EntitySetMappings
                .SelectMany(m => m.EntityTypeMappings)
                .Select(m => m.EntityType)
                .ToDictionary(getEntityType, ExtractClrTypeMetadata);
            return map;
        }

        private static Type ExtractClrTypeMetadata(EntityType entityType)
        {
            return entityType.MetadataProperties
                .Where(p => p.Name.Equals(CLR_TYPE_ANNOTATION))
                .Select(p => p.Value as Type)
                .FirstOrDefault();
        }

        #endregion

        private object ResolveForeignKey(Type foreignType, object key)
        {
            return _parentContext.Set(foreignType).Find(key);
        }

        private void AddRelationship(TEntity entity, ForeignKeyInfo foreignKey, object key)
        {
            var localKeyProperty   = foreignKey.FromProperty;
            var foreignKeyProperty = foreignKey.ToProperty;

            // Build safe setters for the local/foreign relations
            Action<object> setForeignRelation = (val) =>
            {
                if (foreignKeyProperty != null)
                    foreignKeyProperty.SetValue(key, val);
            };
            Action<object> setLocalRelation = (val) =>
            {
                if (localKeyProperty != null)
                    localKeyProperty.SetValue(entity, val);
            };

            // Handle each type of relationship as needed
            switch (foreignKey.Relationship)
            {
                case ForeignKeyInfo.RelationshipType.OneToOne:
                    setLocalRelation(key);
                    setForeignRelation(entity);
                    return;
                case ForeignKeyInfo.RelationshipType.OneToMany:
                    // Add the foreign entity to the local entity's collection for that relationship
                    var oneToManyLocal = localKeyProperty.GetValue(entity);
                    localKeyProperty.PropertyType.GetMethod("Add").Invoke(oneToManyLocal, new[] {key});
                    // Add the local entity to the foregin entity as it's foreign reference
                    setForeignRelation(entity);
                    return;
                case ForeignKeyInfo.RelationshipType.ManyToOne:
                    // Add the local entity to the foreign entity's collection for that relationship
                    var foreignCollection  = foreignKeyProperty.GetValue(key);
                    foreignKeyProperty.PropertyType.GetMethod("Add").Invoke(foreignCollection, new[] {entity});
                    // Add the local entity to the foregin entity as it's foreign reference
                    setLocalRelation(key);
                    return;
                case ForeignKeyInfo.RelationshipType.ManyToMany:
                    // Add the foreign entity to the local entity's collection for that relationship
                    var manyToManyLocal = localKeyProperty.GetValue(entity);
                    localKeyProperty.PropertyType.GetMethod("Add").Invoke(manyToManyLocal, new[] {key});
                    // Add the local entity to the foreign entity's collection for that relationship
                    var manyToManyForeign = foreignKeyProperty.GetValue(key);
                    foreignKeyProperty.PropertyType.GetMethod("Add").Invoke(manyToManyForeign, new[] {entity});
                    return;
                default:
                    break;
            }
        }
    }
}
