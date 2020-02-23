using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.EmitBuilders
{
    public class EmitTypeBuilder : EmitType, ITypeBuilder
    {
        public TypeBuilder TypeBuilder { get; }

        public EmitTypeBuilder(TypeBuilder type) : base(type)
        {
            TypeBuilder = type;
        }

        public void CreateTypeInfo()
        {

            TypeBuilder.CreateTypeInfo();
        }

        public IConstructorBuilder DefineConstructor(MethodAttributes attributes, IType[] parameters)
        {
            var p = parameters.Select(x => ((EmitType)x).Type).ToArray();
            return new EmitConstructorBuilder(TypeBuilder.DefineConstructor(attributes, CallingConventions.Standard, p), parameters);
        }

        public IFieldBuilder DefineField(string name, IType fieldType, FieldAttributes attributes)
        {
            if (fieldType == null)
            {
                throw new ArgumentNullException(nameof(fieldType));
            }
            return new EmitFieldBuilder(TypeBuilder.DefineField(name, ((EmitType)fieldType).Type, attributes), fieldType);
        }

        public IMethodBuilder DefineMethod(string name, MethodAttributes attributes, IType returnType, IType[] parameters)
        {
            if (returnType == null)
            {
                throw new ArgumentNullException(nameof(returnType));
            }
            var p = parameters.Select(x => ((EmitType)x).Type).ToArray();
            var r = ((EmitType)returnType).Type;
            return new EmitMethodBuilder(TypeBuilder.DefineMethod(name, attributes, r, p), returnType, parameters);
        }
    }
}
