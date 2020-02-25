using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.IlAsmBuilders
{
    public class AsmConstructorBuilder : IConstructorBuilder
    {
        private readonly (IType type, string name)[] parameters;
        private readonly AsmILGenerator ilGenerator;
        public MethodImplAttributes MethodImplAttributes { get; private set; } = 0;
        public MethodAttributes MethodAttributes { get; }

        public IType DeclaringType { get; }

        public AsmConstructorBuilder(IType declaringType, MethodAttributes methodAttributes, IType[] parameters)
        {
            DeclaringType = declaringType;
            this.parameters = parameters.Select(x => (x, "")).ToArray();
            ilGenerator = new AsmILGenerator();
            MethodAttributes = methodAttributes;
        }

        public void DefineParameter(int idx, ParameterAttributes parameterAttributes, string name)
        {
            parameters[idx].name = name;
        }

        public IILGenerator GetILGenerator()
        {
            return ilGenerator;
        }

        public IType[] GetParameters()
        {
            return parameters.Select(x => x.type).ToArray();
        }

        public void SetImplementationFlags(MethodImplAttributes attributes)
        {
            MethodImplAttributes = attributes;
        }
    }
}
