using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace SharpTools.Testing.EntityFramework
{
    internal class PrimaryKeyInfo
    {
        public string Name { get; set; }
        public bool IsComputed { get; set; }
        public bool IsIdentity { get; set; }
        public Type KeyType { get; set; }
        
        public PrimaryKeyInfo(EntityType entityType, EdmProperty keyProperty)
        {
            Name = keyProperty.Name;
            IsComputed = keyProperty.IsStoreGeneratedComputed;
            IsIdentity = keyProperty.IsStoreGeneratedIdentity;
            KeyType = keyProperty.PrimitiveType.ClrEquivalentType;
        }
        
        public static IEnumerable<PrimaryKeyInfo> Map(EntityType entityType)
        {
            return entityType.KeyProperties.Select(kp => new PrimaryKeyInfo(entityType, kp));
        }
    }
}
