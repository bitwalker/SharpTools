using System;
using System.Data.Entity.Core.Metadata.Edm;

namespace SharpTools.Testing.EntityFramework.Internal
{
    internal sealed class ForeignKeyException : Exception
    {
        private const string UNHANDLED_MULTIPLICITY = "{0}.{1} defines an unhandled relationship multiplicity type: {2}";

        public RelationshipMultiplicity ToMultiplicity { get; private set; }
        public RelationshipMultiplicity FromMultiplicity { get; private set; }

        public ForeignKeyException(string entityType, string keyName, RelationshipMultiplicity from, RelationshipMultiplicity to)
            : base(string.Format(UNHANDLED_MULTIPLICITY, entityType, keyName, string.Format("{0} -> {1}", from, to)))
        {
        }
    }
}
