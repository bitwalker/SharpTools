using System;
using System.Reflection;

namespace SharpTools.Reflection
{
    public struct CallerInfo
    {
        public Type CallerType { get; set; }
        public string Class { get; set; }
        public string Method { get; set; }
        public string Signature { get; set; }

        public static CallerInfo FromMethod(MethodInfo method)
        {
            return new CallerInfo()
            {
                CallerType = method.DeclaringType,
                Class = method.DeclaringType.Name,
                Method = method.Name,
                Signature = Reflector.Stringify(method)
            };
        }
    }
}
