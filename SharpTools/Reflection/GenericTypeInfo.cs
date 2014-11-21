using System;

namespace SharpTools.Reflection
{
    public struct GenericTypeInfo
    {
        public Type InterfaceType { get; set; }
        public Type ConcreteType { get; set; }
        public Type[] TypeParameters { get; set; }

        public static GenericTypeInfo FromType(Type type, Type interfaceType)
        {
            return new GenericTypeInfo() {
                InterfaceType = interfaceType,
                ConcreteType = type,
                TypeParameters = interfaceType.GetGenericArguments()
            };
        }
    }
}
