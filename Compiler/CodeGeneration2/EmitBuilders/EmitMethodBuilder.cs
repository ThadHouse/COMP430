using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.EmitBuilders
{
    public class EmitMethodBuilder : EmitMethodInfo, IMethodBuilder
    {
        public MethodBuilder MethodBuilder { get; }

        public EmitMethodBuilder(MethodBuilder methodInfo, IType returnType, IType[] parameters) : base(methodInfo, returnType, parameters)
        {
            MethodBuilder = methodInfo;
        }

        public void DefineParameter(int idx, ParameterAttributes parameterAttributes, string name)
        {
            MethodBuilder.DefineParameter(idx, parameterAttributes, name);
        }

        public IILGenerator GetILGenerator()
        {
            return new EmitILGenerator(MethodBuilder.GetILGenerator());
        }

        public void SetImplementationFlags(MethodImplAttributes attributes)
        {
            MethodBuilder.SetImplementationFlags(attributes);
        }
    }
}
