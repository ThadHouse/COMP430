using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Compiler.CodeGeneration2.Builders
{
    public interface IType
    {
        string ModuleName { get; }

        IType MakeArrayType();

        IMethodInfo GetMethod(string methodName);

        bool IsValueType { get; }

        bool IsAssignableFrom(IType other);

        string FullName { get; }

        bool IsArray { get; }

        IFieldInfo[] GetFields(BindingFlags flags);

        IMethodInfo[] GetMethods(BindingFlags flags);

        IConstructorInfo[] GetConstructors(BindingFlags flags);
    }
}
