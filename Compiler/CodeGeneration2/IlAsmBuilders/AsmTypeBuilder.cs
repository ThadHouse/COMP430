using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.IlAsmBuilders
{
    public class AsmTypeBuilder : ITypeBuilder
    {
        public AsmTypeBuilder(string fullName, bool isArray, string baseType)
        {
            FullName = fullName;
            IsArray = isArray;
            BaseType = baseType;
        }

        public string BaseType { get; }

        public bool IsValueType => false;

        public string FullName { get; }

        public bool IsArray { get; }

        public void CreateTypeInfo()
        {
            throw new NotImplementedException();
        }

        public IConstructorBuilder DefineConstructor(MethodAttributes attributes, IType[] parameters)
        {
            throw new NotImplementedException();
        }

        public IFieldBuilder DefineField(string name, IType fieldType, FieldAttributes attributes)
        {
            throw new NotImplementedException();
        }

        public IMethodBuilder DefineMethod(string name, MethodAttributes attributes, IType returnType, IType[] parameters)
        {
            throw new NotImplementedException();
        }

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
            throw new NotImplementedException();
        }
    }
}
