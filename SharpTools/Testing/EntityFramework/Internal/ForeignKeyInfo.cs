using System.Reflection;
using System.Data.Entity.Core.Metadata.Edm;

namespace SharpTools.Testing.EntityFramework.Internal
{
    internal sealed class ForeignKeyInfo
    {
        public enum RelationshipType
        {
            OneToOne,
            OneToMany,
            ManyToOne,
            ManyToMany
        }

        public RelationshipType Relationship { get; private set; }
        public RelationshipEndMember From { get; private set; }
        public RelationshipEndMember To { get; private set; }
        public PropertyInfo FromProperty { get; private set; }
        public PropertyInfo ToProperty { get; private set; }

        public ForeignKeyInfo(NavigationProperty foreignKey, PropertyInfo toProperty, PropertyInfo fromProperty)
        {
            From = foreignKey.FromEndMember;
            To   = foreignKey.ToEndMember;
            FromProperty = fromProperty;
            ToProperty   = toProperty;
            Relationship = DetermineRelationshipType(this);
        }

        public static RelationshipType DetermineRelationshipType(ForeignKeyInfo key)
        {
            var fromType = key.From.RelationshipMultiplicity;
            var toType = key.To.RelationshipMultiplicity;
            if (fromType == RelationshipMultiplicity.One && toType == RelationshipMultiplicity.One)
                return RelationshipType.OneToOne;
            else if (fromType == RelationshipMultiplicity.ZeroOrOne && toType == RelationshipMultiplicity.Many)
                return RelationshipType.OneToMany;
            else if (fromType == RelationshipMultiplicity.One && toType == RelationshipMultiplicity.Many)
                return RelationshipType.OneToMany;
            else if (fromType == RelationshipMultiplicity.Many && toType == RelationshipMultiplicity.One)
                return RelationshipType.ManyToOne;
            else if (fromType == RelationshipMultiplicity.Many && toType == RelationshipMultiplicity.ZeroOrOne)
                return RelationshipType.ManyToOne;
            else if (fromType == RelationshipMultiplicity.Many && toType == RelationshipMultiplicity.Many)
                return RelationshipType.ManyToMany;
            else
                throw new ForeignKeyException(key.From.DeclaringType.Name, key.From.Name, fromType, toType);
        }
    }
}
