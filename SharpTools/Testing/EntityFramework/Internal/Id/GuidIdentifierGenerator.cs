using System;
using System.Diagnostics;

namespace SharpTools.Testing.EntityFramework.Internal.Id
{
    [DebuggerDisplay("GuidIdentifierGenerator")]
    internal class GuidIdentifierGenerator : IIdentifierGenerator
    {
        public object Generate()
        {
            return Guid.NewGuid();
        }
    }
}
