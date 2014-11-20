using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpTools.Testing.EntityFramework.Internal.Id;

namespace SharpTools.Testing.EntityFramework.Internal
{
    internal class IdentifierGeneratorFactory
    {
        public static IIdentifierGenerator Create(PrimaryKeyInfo info)
        {
            if (info.KeyType.Equals(typeof(short)))
                return new AutoIncrementingShortIdentifierGenerator();
            if (info.KeyType.Equals(typeof(int)))
                return new AutoIncrementingIntegerIdentifierGenerator();
            if (info.KeyType.Equals(typeof(long)))
                return new AutoIncrementingLongIdentifierGenerator();
            if (info.KeyType.Equals(typeof(Guid)))
                return new GuidIdentifierGenerator();

            return new DefaultIdentifierGenerator(info.KeyType);
        }
    }
}
