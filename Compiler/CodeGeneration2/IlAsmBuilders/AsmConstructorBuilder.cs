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

        public IReadOnlyList<(IType type, string name)> MethodParameters => methodParameters;


        private readonly AsmILGenerator ilGenerator;

        private readonly (IType type, string name)[] methodParameters;

        public MethodImplAttributes MethodImplAttributes { get; private set; } = 0;
        public MethodAttributes MethodAttributes { get; }

        public IType DeclaringType { get; }

        public AsmConstructorBuilder(IType declaringType, MethodAttributes methodAttributes, IType[] parameters)
        {
            DeclaringType = declaringType;
            methodParameters = parameters.Select(x => (x, "")).ToArray();
            ilGenerator = new AsmILGenerator();
            MethodAttributes = methodAttributes;
        }

        public void DefineParameter(int idx, ParameterAttributes parameterAttributes, string name)
        {
            methodParameters[idx].name = name;
        }

        public AsmILGenerator GetILGenerator()
        {
            return ilGenerator;
        }

        IILGenerator IBaseMethodBuilder.GetILGenerator()
        {
            return ilGenerator;
        }

        public IType[] GetParameters()
        {
            return methodParameters.Select(x => x.type).ToArray();
        }

        public void SetImplementationFlags(MethodImplAttributes attributes)
        {
            MethodImplAttributes = attributes;
        }
    }
}
