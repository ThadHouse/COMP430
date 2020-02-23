using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Compiler.CodeGeneration2.Builders
{
    public interface ITypeBuilder : IType
    {
        void CreateTypeInfo();

        IFieldBuilder DefineField(string name, IType fieldType, FieldAttributes attributes);

        IConstructorBuilder DefineConstructor(MethodAttributes attributes, IType[] parameters);

        IMethodBuilder DefineMethod(string name, MethodAttributes attributes, IType returnType, IType[] parameters);
    }
}
