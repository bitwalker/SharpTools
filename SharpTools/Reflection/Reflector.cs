using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace SharpTools.Reflection
{
    /// <summary>
    /// Provides facilities for common reflection operations.
    /// </summary>
    public static class Reflector
    {
        /// <summary>
        /// Returns the fully qualified type name of the provided type,
        /// but without version and public key properties.
        /// </summary>
        public static string ToSimpleTypeString(Type type)
        {
            var assemblyName = type.Assembly.FullName.Split(new[] { ',' }).First();
            var typeName     = type.FullName;
            return string.Format("{0}, {1}", typeName, assemblyName);
        }

        /// <summary>
        /// Resolves the provided type name and returns a new instance of the type.
        /// The type string must be fully qualified (type & assembly)
        /// </summary>
        /// <param name="typeString">The fully qualified name of the type to resolve</param>
        /// <returns>An instance of the requested type, as an Object</returns>
        public static object ResolveTypeString(string typeString)
        {
            var parts        = typeString.Split(new[] { ',' }).Select(s => s.Trim()).ToArray();
            var typeName     = parts.First();
            var assemblyName = parts.Last();
            return AppDomain.CurrentDomain.CreateInstanceAndUnwrap(assemblyName, typeName);
        }

        /// <summary>
        /// Returns information about the calling function's caller. In other words,
        /// calling this function from another function will return details about the caller
        /// of that function.
        /// </summary>
        /// <example>
        /// public Test
        /// {
        ///     public static void Main(params string[] args)
        ///     {
        ///         Two();
        ///     }
        ///     public static void Two()
        ///     {
        ///         var callerInfo = Reflector.GetCallerInfo();
        ///         Assert.AreEqual(callerInfo.CallerType, typeof(Test));
        ///         Assert.AreEqual(callerInfo.Class, "Test");
        ///         Assert.AreEqual(callerInfo.Method, "Main");
        ///         Assert.AreEqual(callerInfo.Signature, "public static void Main(String[] args)");
        ///     }
        /// }
        /// </example>
        /// <returns></returns>
        public static CallerInfo GetCallerInfo()
        {
            var s      = new StackTrace();
            var method = s.GetFrame(2).GetMethod();
            var info = MethodInfo.GetMethodFromHandle(method.MethodHandle, method.DeclaringType.TypeHandle) as MethodInfo;
            return CallerInfo.FromMethod(info);
        }

        /// <summary>
        /// Get all concrete types in the provided assembly
        /// </summary>
        /// <param name="assembly">The assembly to search</param>
        /// <returns>A collection of Type</returns>
        public static IEnumerable<Type> GetConcreteTypes(Assembly assembly)
        {
            return assembly
                .GetTypes()
                .Where(t => !t.IsInterface)
                .Where(t => !t.IsAbstract)
                .Where(t => !t.IsGenericTypeDefinition);
        }

        /// <summary>
        /// Get all concrete implementation types of the provided interface in the current app domain.
        /// </summary>
        /// <param name="interfaceType">The interface that is implemented by the collected types</param>
        /// <returns>A collection of GenericTypeInfo</returns>
        public static IEnumerable<GenericTypeInfo> GetImplementationTypes(Type interfaceType)
        {
            // If this is a generic interface, but the specific type provided had
            // it's type parameters set, get the generic definition and search for that instead
            var isGeneric           = interfaceType.IsGenericType;
            var isGenericDefinition = interfaceType.IsGenericTypeDefinition;
            Type actualInterface = interfaceType;
            if (isGeneric && !isGenericDefinition)
                actualInterface = interfaceType.GetGenericTypeDefinition();

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetConcreteTypes)
                .SelectMany(t =>
                {
                    if (isGeneric)
                    {
                        return t.GetInterfaces()
                                .Where(i => actualInterface.IsAssignableFrom(i.GetGenericTypeDefinition()))
                                .Select(i => GenericTypeInfo.FromType(t, i));
                    }
                    else
                    {
                        return t.GetInterfaces()
                            .Where(i => actualInterface.IsAssignableFrom(i))
                            .Select(i => GenericTypeInfo.FromType(t, i));
                    }
                });
        }

        private const string METHOD_SIGNATURE_FORMAT = "{0}({1})";
        private const string PARAMETER_FORMAT = "{0}{1} {2}";
        private const string REF_PARAM_FORMAT = "ref {0} {1}";
        private const string OUT_PARAM_FORMAT = "out {0} {1}";
        private const string OPTIONAL_FORMAT  = " = {0}";

        /// <summary>
        /// Convert a MethodBase definition to a stringified representation of
        /// the method's signature.
        /// </summary>
        /// <param name="method">The method to stringify</param>
        /// <returns>A stringified representation of a method signature</returns>
        public static string Stringify(MethodInfo method)
        {
            string signature = string.Empty;

            var name         = method.Name;
            var parameters   = method.GetParameters();
            var visibility   = method.IsPublic   ? "public" : "private";
            var locality     = method.IsStatic   ? "static" : "";
            var virtuality   = method.IsVirtual  ? "virtual" : "";
            var abstractness = method.IsAbstract ? "abstract" : "";
            var returns      = method.ReturnType.Equals(typeof (void))
                                ? "void"
                                : GetGenericTypeSignature(method.ReturnType);

            foreach (var component in new [] { visibility, locality, virtuality, abstractness, returns })
            {
                if (string.IsNullOrWhiteSpace(component))
                    continue;

                signature += (component + " ");
            }

            var stringified = parameters
                .OrderBy(p => p.Position)
                .Select(p =>
                {
                    string result;
                    bool isParams = Attribute.IsDefined(p, typeof (ParamArrayAttribute));
                    var paramName = p.Name;
                    var paramType = GetGenericTypeSignature(p.ParameterType);
                    if (p.IsOut)
                        result = string.Format(OUT_PARAM_FORMAT, paramType, paramName);
                    else if (p.ParameterType.IsByRef)
                        result = string.Format(REF_PARAM_FORMAT, paramType, paramName);
                    else
                        result = string.Format(PARAMETER_FORMAT, isParams ? "params " : "", paramType, paramName);

                    if (p.IsOptional)
                        return result + string.Format(OPTIONAL_FORMAT, p.DefaultValue);
                    else
                        return result;
                });

            var paramsString = string.Join(", ", stringified);
            signature += string.Format(METHOD_SIGNATURE_FORMAT, name, paramsString);
            return signature;
        }

        /// <summary>
        /// Fetch a property from a type given a property access expression.
        /// </summary>
        /// <example>
        /// var nameProperty = Reflector.GetProperty<PrimaryKeyInfo>(pk => pk.Name);
        /// </example>
        /// <typeparam name="T">The type to reflect on</typeparam>
        /// <param name="expression">The property access expression which defines what property to get.</param>
        /// <returns>PropertyInfo</returns>
        public static PropertyInfo GetProperty<T>(Expression<Func<T, object>> expression)
        {
            return GetMember(expression.Body) as PropertyInfo;
        }

        /// <summary>
        /// Fetch a property from a type given a property access expression.
        /// </summary>
        /// <example>
        /// var nameProperty = Reflector.GetProperty<PrimaryKeyInfo>(pk => pk.Name);
        /// </example>
        /// <typeparam name="T">The type to reflect on</typeparam>
        /// <typeparam name="U">The type of the property to retrieve</typeparam>
        /// <param name="expression">The property access expression which defines what property to get.</param>
        /// <returns>PropertyInfo</returns>
        public static PropertyInfo GetProperty<T, U>(Expression<Func<T, U>> expression)
        {
            return GetMember(expression.Body) as PropertyInfo;
        }

        /// <summary>
        /// Fetch a member from a type given a member access expression.
        /// </summary>
        /// <example>
        /// var nameField = Reflector.GetProperty<PrimaryKeyInfo>(pk => pk.Name);
        /// </example>
        /// <typeparam name="T">The type to reflect on</typeparam>
        /// <param name="expression">The member access expression which defines what member to get</param>
        /// <returns>MemberInfo</returns>
        public static MemberInfo GetMember<T>(Expression<Func<T, object>> expression)
        {
            return GetMember(expression.Body);
        }

        /// <summary>
        /// Fetch a member from a type given a member access expression.
        /// </summary>
        /// <example>
        /// var nameField = Reflector.GetProperty<PrimaryKeyInfo>(pk => pk.Name);
        /// </example>
        /// <typeparam name="T">The type to reflect on</typeparam>
        /// <typeparam name="U">The type of the member to get</typeparam>
        /// <param name="expression">The member access expression which defines what member to get</param>
        /// <returns>MemberInfo</returns>
        public static MemberInfo GetMember<T, U>(Expression<Func<T, U>> expression)
        {
            return GetMember(expression.Body);
        }

        /// <summary>
        /// Given a getter method, attempt to get the associated property name
        /// </summary>
        /// <param name="methodInfo">The MethodInfo of the getter for the property</param>
        /// <param name="name">The string variable to store the name in, if successful</param>
        /// <returns>Boolean</returns>
        public static bool TryGetPropertyNameFromGetter(MethodInfo methodInfo, out string name)
        {
            if (methodInfo.Name.StartsWith("get_"))
            {
                name = methodInfo.Name.Substring(4);
                return true;
            }

            name = null;
            return false;
        }

        /// <summary>
        /// Given a setter method, attempt to get the associated property name
        /// </summary>
        /// <param name="methodInfo">The MethodInfo of the setter for the property</param>
        /// <param name="name">The string variable to store the name in, if successful</param>
        /// <returns>Boolean</returns>
        public static bool TryGetPropertyNameFromSetter(MethodInfo methodInfo, out string name)
        {
            if (methodInfo.Name.StartsWith("set_"))
            {
                name = methodInfo.Name.Substring(4);
                return true;
            }

            name = null;
            return false;
        }

        /// <summary>
        /// Given an expression, attempt to get the member that was accessed
        /// </summary>
        /// <param name="expression">A member call/access expression</param>
        /// <returns>MemberInfo</returns>
        private static MemberInfo GetMember(Expression expression)
        {
            if (IsIndexedPropertyAccess(expression) || IsMethodExpression(expression))
            {
                return ((MethodCallExpression) expression).Method;
            }
            return GetMemberExpression(expression).Member;
        }

        /// <summary>
        /// Given an expression, attempt to get a nested member expression
        /// </summary>
        /// <param name="expression">An expression representing a member expression</param>
        /// <returns>MemberExpression</returns>
        private static MemberExpression GetMemberExpression(Expression expression)
        {
            return GetMemberExpression(expression, true);
        }

        private static MemberExpression GetMemberExpression(Expression expression, bool enforceCheck)
        {
            MemberExpression memberExpression = null;
            if (expression.NodeType == ExpressionType.Convert)
            {
                var body = (UnaryExpression) expression;
                memberExpression = body.Operand as MemberExpression;
            }
            else if (expression.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = expression as MemberExpression;
            }
            if (enforceCheck && (memberExpression == null))
            {
                throw new ArgumentException("Not a member access", "expression");
            }
            return memberExpression;
        }

        /// <summary>
        /// Determines if the given expression represents an indexed property access
        /// </summary>
        /// <param name="expression">The expression to check</param>
        /// <returns>Boolean</returns>
        private static bool IsIndexedPropertyAccess(Expression expression)
        {
            return (IsMethodExpression(expression) && expression.ToString().Contains("get_Item"));
        }

        /// <summary>
        /// Determines if the given expression represents a method call
        /// </summary>
        /// <param name="expression">The expression to check</param>
        /// <returns>Boolean</returns>
        private static bool IsMethodExpression(Expression expression)
        {
            return ((expression is MethodCallExpression) || ((expression is UnaryExpression) && IsMethodExpression((expression as UnaryExpression).Operand)));
        }

        private static string GetGenericTypeSignature(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var name = type.Name;
            var args = string.Join(", ", type.GetGenericArguments().Select(GetGenericTypeSignature));
            var genericIndex = name.LastIndexOf('`');
            name = name.Substring(0, genericIndex);
            name += ("<" + args + ">");
            return name;
        }
    }
}