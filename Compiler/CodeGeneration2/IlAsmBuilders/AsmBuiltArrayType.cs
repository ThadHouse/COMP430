using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.IlAsmBuilders
{
    public class AsmBuiltArrayType : IType
    {
        private readonly IType baseType;

        public AsmBuiltArrayType(IType type)
        {
            this.baseType = type ?? throw new ArgumentNullException(nameof(type));
            FullName = baseType.FullName + "[]";
        }

        public string ModuleName => baseType.ModuleName;

        public bool IsValueType => false;

        public string FullName { get; }

        public bool IsArray => true;

        public IConstructorInfo[] GetConstructors(BindingFlags flags)
        {
            throw new NotImplementedException();
        }

        public IFieldInfo[] GetFields(BindingFlags flags)
        {
            throw new NotImplementedException();
        }

        public IMethodInfo GetMethod(string methodName)
        {
            throw new NotImplementedException();
        }

        public IMethodInfo[] GetMethods(BindingFlags flags)
        {
            throw new NotImplementedException();
        }

        public bool IsAssignableFrom(IType other)
        {
            throw new NotImplementedException();
        }

        public IType MakeArrayType()
        {
            return new AsmBuiltArrayType(this);
        }
    }
}
