using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpTools.Testing.EntityFramework.Internal;

namespace SharpTools.Testing.EntityFramework.Internal.Id
{
    /// <summary>
    /// The basic implementation of any <see cref="IIdentityType"/>.
    /// Contains predefined identity types for all commonly used identifier types.
    /// </summary>
    [DebuggerDisplay("{UnderlyingClrType}")]
    internal class IdentityType : IIdentityType
    {
        public static readonly IIdentityType Int32  = new IdentityType(typeof(int),    id => int.Parse(id), 0);
        public static readonly IIdentityType Int64  = new IdentityType(typeof(long),   id => long.Parse(id), 0L);
        public static readonly IIdentityType String = new IdentityType(typeof(string), id => id, null);
        public static readonly IIdentityType Guid   = new IdentityType(typeof(Guid),   id => new Guid(id), System.Guid.Empty);

        private static readonly Dictionary<Type, IIdentityType> Lookup = new Dictionary<Type, IIdentityType>
                                                                             {
                                                                                 {typeof (int),    Int32},
                                                                                 {typeof (long),   Int64},
                                                                                 {typeof (string), String},
                                                                                 {typeof (Guid),   Guid},
                                                                             };

        private readonly Type _underlyingClrType;
        private readonly Func<object, string> _toString;
        private readonly Func<string, object> _fromString;
        private readonly object _uninitializedValue;

        public Type UnderlyingClrType
        {
            get { return _underlyingClrType; }
        }

        public IdentityType(Type underlyingClrType, Func<string, object> fromString, object uninitializedValue)
            : this(underlyingClrType, o => o.ToString(), fromString, uninitializedValue)
        {
        }

        public IdentityType(Type underlyingClrType, Func<object, string> toString, Func<string, object> fromString, object uninitializedValue)
        {
            _underlyingClrType = underlyingClrType;
            _toString = toString;
            _fromString = fromString;
            _uninitializedValue = uninitializedValue;
        }

        public static IIdentityType TryGet(Type type)
        {
            IIdentityType result;
            Lookup.TryGetValue(type, out result);
            return result;
        }

        private const string MISMATCHED_TYPES = "Mismatched types! Expected {0}, got {1}";
        public string ToString(object o)
        {
            if (o.GetType() != _underlyingClrType)
            {
                var expected = _underlyingClrType.Name;
                var got      = o.GetType().Name;
                throw new ArgumentException(string.Format(MISMATCHED_TYPES, expected, got));
            }

            return _toString(o);
        }

        public object FromString(string id)
        {
            return _fromString(id);
        }

        public bool IsUninitializedValue(object idToCheck)
        {
            if (idToCheck == null)
                return _uninitializedValue == null;

            return idToCheck.Equals(_uninitializedValue);
        }
    }
}