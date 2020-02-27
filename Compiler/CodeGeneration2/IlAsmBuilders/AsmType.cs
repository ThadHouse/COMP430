using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Compiler.CodeGeneration2.Builders;

using SType = System.Type;

namespace Compiler.CodeGeneration2.IlAsmBuilders
{
    public class AsmType : IType
    {
        public static Dictionary<SType, IType> TypeCache { get; } = new Dictionary<SType, IType>();

        private static IType GetTypeFromCache(SType type)
        {
            if (TypeCache.TryGetValue(type, out var value))
            {
                return value;
            }
            value = new AsmType(type);
            return value;
        }

        private SType type { get; }

        public bool IsValueType => type.IsValueType;

        public string FullName => type.FullName;

        public bool IsArray => type.IsArray;

        public string ModuleName { get; }

        private readonly Dictionary<BindingFlags, IFieldInfo[]> fields = new Dictionary<BindingFlags, IFieldInfo[]>();
        private readonly Dictionary<BindingFlags, IMethodInfo[]> methods = new Dictionary<BindingFlags, IMethodInfo[]>();
        private readonly Dictionary<BindingFlags, IConstructorInfo[]> constructors = new Dictionary<BindingFlags, IConstructorInfo[]>();

        public AsmType(SType type)
        {
            this.type = type ?? throw new ArgumentNullException(nameof(type));
            ModuleName = type.Module.Name;
            if (ModuleName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase) || ModuleName.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
            {
                ModuleName = ModuleName.Substring(0, ModuleName.Length - 4);
            }
            TypeCache.Add(type, this);
        }

        public IType MakeArrayType()
        {
            var arrayType = type.MakeArrayType();
            if (TypeCache.TryGetValue(arrayType, out var arrType))
            {
                return arrType;
            }
            return new AsmType(arrayType);
        }

        public IMethodInfo GetMethod(string methodName)
        {
            var method = type.GetMethod(methodName);
            return new AsmMethodInfo(this, method, GetTypeFromCache(method.ReturnType), method.GetParameters().Select(x => GetTypeFromCache(x.ParameterType)).ToArray());
        }

        public IFieldInfo[] GetFields(BindingFlags flags)
        {
            if (fields.TryGetValue(flags, out var value))
            {
                return value;
            }
            bool isStatic = flags.HasFlag(BindingFlags.Static);
            value = type.GetFields(flags).Select(x => new AsmFieldInfo(this, x.Name, GetTypeFromCache(x.FieldType), isStatic)).ToArray();
            fields.Add(flags, value);
            return value;
        }

        public IMethodInfo[] GetMethods(BindingFlags flags)
        {
            if (methods.TryGetValue(flags, out var value))
            {
                return value;
            }
            value = type.GetMethods(flags).Select(x =>
            {
                var retType = GetTypeFromCache(x.ReturnType);
                var parameters = x.GetParameters().Select(y => GetTypeFromCache(y.ParameterType)).ToArray();
                return new AsmMethodInfo(this, x, retType, parameters);
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
            value = type.GetConstructors(flags).Select(x => new AsmConstructorInfo(this, x.GetParameters().Select(y => GetTypeFromCache(y.ParameterType)).ToArray())).ToArray();
            constructors.Add(flags, value);
            return value;
        }

        public bool IsAssignableFrom(IType other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return this.FullName == other.FullName;

            //return type.IsAssignableFrom(((EmitType)other).Type);
        }
    }
}
