﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.IlAsmBuilders
{
    public class AsmTypeBuilder : ITypeBuilder
    {
        private readonly List<IConstructorBuilder> constructors = new List<IConstructorBuilder>();
        private readonly List<IMethodBuilder> methods = new List<IMethodBuilder>();
        private readonly List<IFieldBuilder> fields = new List<IFieldBuilder>();


        public AsmTypeBuilder(string moduleName, string fullName, bool isArray, string baseType)
        {
            FullName = fullName;
            IsArray = isArray;
            BaseType = baseType;
            ModuleName = moduleName;
        }

        public string BaseType { get; }

        public bool IsValueType => false;

        public string FullName { get; }

        public bool IsArray { get; }

        public string ModuleName { get; }

        public void CreateTypeInfo()
        {
            //throw new NotImplementedException();
        }

        public IConstructorBuilder DefineConstructor(MethodAttributes attributes, IType[] parameters)
        {
            var builder = new AsmConstructorBuilder(this, attributes, parameters);
            constructors.Add(builder);
            return builder;
        }

        public IFieldBuilder DefineField(string name, IType fieldType, FieldAttributes attributes)
        {
            var builder = new AsmFieldBuilder(this, fieldType, attributes, name);
            fields.Add(builder);
            return builder;
        }

        public IMethodBuilder DefineMethod(string name, MethodAttributes attributes, IType returnType, IType[] parameters)
        {
            var builder = new AsmMethodBuilder(this, returnType, parameters, attributes, name);
            methods.Add(builder);
            return builder;
        }

        public IConstructorInfo[] GetConstructors(BindingFlags flags)
        {
            throw new NotSupportedException("Cannot get constructors of a builder");
        }

        public IFieldInfo[] GetFields(BindingFlags flags)
        {
            throw new NotSupportedException("Cannot get fields of a builder");
        }

        public IMethodInfo GetMethod(string methodName)
        {
            throw new NotSupportedException("Cannot get method of a builder");
        }

        public IMethodInfo[] GetMethods(BindingFlags flags)
        {
            throw new NotSupportedException("Cannot get methods of a builder");
        }

        public bool IsAssignableFrom(IType other)
        {
            if (other == null)
            {
                return false;
            }
            return FullName == other.FullName;
        }

        public IType MakeArrayType()
        {
            throw new NotSupportedException("Making arrays of a defined type is not supported");
        }
    }
}
