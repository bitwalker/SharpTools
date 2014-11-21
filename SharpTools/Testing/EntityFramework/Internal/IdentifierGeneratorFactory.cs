using System;
using SharpTools.Testing.EntityFramework.Internal.Id;

namespace SharpTools.Testing.EntityFramework.Internal
{
    using LazyShortGenerator = Lazy<AutoIncrementingShortIdentifierGenerator>;
    using LazyIntGenerator   = Lazy<AutoIncrementingIntegerIdentifierGenerator>;
    using LazyLongGenerator  = Lazy<AutoIncrementingLongIdentifierGenerator>;
    using LazyGuidGenerator  = Lazy<GuidIdentifierGenerator>;

    internal class IdentifierGeneratorFactory
    {

        private static LazyShortGenerator _shortIds =
            new LazyShortGenerator(() => new AutoIncrementingShortIdentifierGenerator()); 
        private static LazyIntGenerator _intIds =
            new LazyIntGenerator(() =>   new AutoIncrementingIntegerIdentifierGenerator()); 
        private static LazyLongGenerator _longIds =
            new LazyLongGenerator(() =>  new AutoIncrementingLongIdentifierGenerator()); 
        private static LazyGuidGenerator _guidIds =
            new LazyGuidGenerator(() =>  new GuidIdentifierGenerator()); 

        public static IIdentifierGenerator Create(PrimaryKeyInfo info)
        {
            if (info.KeyType.Equals(typeof (short)))
                return _shortIds.Value;
            if (info.KeyType.Equals(typeof (int)))
                return _intIds.Value;
            if (info.KeyType.Equals(typeof (long)))
                return _longIds.Value;
            if (info.KeyType.Equals(typeof (Guid)))
                return _guidIds.Value;

            return new DefaultIdentifierGenerator(info.KeyType);
        }
    }
}
