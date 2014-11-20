using System;
using System.Diagnostics;

namespace SharpTools.Testing.EntityFramework.Internal.Id
{
    [DebuggerDisplay("DefaultIdentifierGenerator")]
    internal class DefaultIdentifierGenerator : IIdentifierGenerator
    {
        private readonly Type _keyType;

        public DefaultIdentifierGenerator(Type keyType)
        {
            _keyType = keyType;
        }

        public object Generate()
        {
            try
            {
                return Activator.CreateInstance(_keyType);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
