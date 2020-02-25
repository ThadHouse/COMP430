using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.EmitBuilders
{
    public class EmitType : IType
    {
        public static Dictionary<Type, IType> TypeCache { get; } = new Dictionary<Type, IType>();

        private static IType GetTypeFromCache(Type type)
        {
            if (TypeCache.TryGetValue(type, out var value))
            {
                return value;
            }
            value = new EmitType(type);
            return value;
        }

        public Type Type { get; }

        public bool IsValueType => Type.IsValueType;

        public string FullName => Type.FullName;

        public bool IsArray => Type.IsArray;

        public string ModuleName => throw new NotSupportedException("Module Name is not supported for reflection emit");

        private readonly Dictionary<BindingFlags, IFieldInfo[]> fields = new Dictionary<BindingFlags, IFieldInfo[]>();
        private readonly Dictionary<BindingFlags, IMethodInfo[]> methods = new Dictionary<BindingFlags, IMethodInfo[]>();
        private readonly Dictionary<BindingFlags, IConstructorInfo[]> constructors = new Dictionary<BindingFlags, IConstructorInfo[]>();

        public EmitType(Type type)
        {
            Type = type;
            TypeCache.Add(type, this);
        }

        public IType MakeArrayType()
        {
            var arrayType = Type.MakeArrayType();
            if (TypeCache.TryGetValue(arrayType, out var arrType))
            {
                return arrType;
            }
            return new EmitType(arrayType);
        }

        public IMethodInfo GetMethod(string methodName)
        {
            var method = Type.GetMethod(methodName);
            return new EmitMethodInfo(method, GetTypeFromCache(method.ReturnType), method.GetParameters().Select(x => GetTypeFromCache(x.ParameterType)).ToArray());
        }

        public IFieldInfo[] GetFields(BindingFlags flags)
        {
            if (fields.TryGetValue(flags, out var value))
            {
                return value;
            }
            value = Type.GetFields(flags).Select(x => new EmitFieldInfo(x, GetTypeFromCache(x.FieldType))).ToArray();
            fields.Add(flags, value);
            return value;
        }

        public IMethodInfo[] GetMethods(BindingFlags flags)
        {
            if (methods.TryGetValue(flags, out var value))
            {
                return value;
            }
            value = Type.GetMethods(flags).Select(x =>
            {
                var retType = GetTypeFromCache(x.ReturnType);
                var parameters = x.GetParameters().Select(y => GetTypeFromCache(y.ParameterType)).ToArray();
                return new EmitMethodInfo(x, retType, parameters);
            }).ToArray();
            methods.Add(flags, value);
            return value;
        }

        public IConstructorInfo[] GetConstructors(BindingFlags flags)
        {
            if (constructors.TryGetValue(flags, out var value))
            {
                return value;
            }
            value = Type.GetConstructors(flags).Select(x => new EmitConstructorInfo(x, x.GetParameters().Select(y => GetTypeFromCache(y.ParameterType)).ToArray())).ToArray();
            constructors.Add(flags, value);
            return value;
        }

        public bool IsAssignableFrom(IType other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return Type.IsAssignableFrom(((EmitType)other).Type);
        }
    }
}
