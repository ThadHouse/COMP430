using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.EmitBuilders
{
    public class EmitConstructorBuilder : EmitConstructorInfo, IConstructorBuilder
    {
        public ConstructorBuilder ConstructorBuilder { get; }

        public EmitConstructorBuilder(ConstructorBuilder constructorInfo, IType[] parameters) : base(constructorInfo, parameters)
        {
            ConstructorBuilder = constructorInfo;
        }

        public void DefineParameter(int idx, ParameterAttributes parameterAttributes, string name)
        {
            ConstructorBuilder.DefineParameter(idx, parameterAttributes, name);
        }

        public IILGenerator GetILGenerator()
        {
            return new EmitILGenerator(ConstructorBuilder.GetILGenerator());
        }

        public void SetImplementationFlags(MethodImplAttributes attributes)
        {
            ConstructorBuilder.SetImplementationFlags(attributes);
        }
    }
}
