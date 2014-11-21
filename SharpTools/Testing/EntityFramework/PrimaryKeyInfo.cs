using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace SharpTools.Testing.EntityFramework
{
    internal sealed class PrimaryKeyInfo
    {
        public string Name { get; private set; }
        public Type KeyType { get; private set; }
        public StoreGeneratedPattern StoreGeneratedPattern { get; private set; }
        public bool IsStoreGenerated { get { return StoreGeneratedPattern != StoreGeneratedPattern.None; }}
        public bool IsComputed { get { return StoreGeneratedPattern == StoreGeneratedPattern.Computed; }}
        public bool IsIdentity { get { return StoreGeneratedPattern == StoreGeneratedPattern.Identity; }}
        
        public PrimaryKeyInfo(EntityType entityType, EdmProperty keyProperty)
        {
            Name = keyProperty.Name;
            KeyType = keyProperty.PrimitiveType.ClrEquivalentType;
            StoreGeneratedPattern = keyProperty.StoreGeneratedPattern;
        }
        
        public static IEnumerable<PrimaryKeyInfo> Map(EntityType entityType)
        {
            return entityType.KeyProperties.Select(kp => new PrimaryKeyInfo(entityType, kp));
        }
    }
}
