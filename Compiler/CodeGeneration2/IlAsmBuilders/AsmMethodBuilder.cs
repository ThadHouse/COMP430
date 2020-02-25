using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.IlAsmBuilders
{
    public class AsmMethodBuilder : IMethodBuilder
    {
        public IType ReturnType { get; }
        public bool IsStatic => MethodAttributes.HasFlag(MethodAttributes.Static);

        public MethodAttributes MethodAttributes { get; }

        public string Name { get; }

        public IReadOnlyList<(IType type, string name)> MethodParameters => methodParameters;


        private readonly AsmILGenerator ilGenerator;

        private readonly (IType type, string name)[] methodParameters;

        public MethodImplAttributes MethodImplAttributes { get; private set; } = 0;

        public IType DeclaringType { get; }

        public AsmMethodBuilder(IType declaringType, IType returnType, IType[] parameters, MethodAttributes methodAttributes, string name, bool entryPoint)
        {
            DeclaringType = declaringType;
            ReturnType = returnType;
            Name = name;
            MethodAttributes = methodAttributes;
            methodParameters = parameters.Select(x => (x, "")).ToArray();
            ilGenerator = new AsmILGenerator(entryPoint);
        }

        public void DefineParameter(int idx, ParameterAttributes parameterAttributes, string name)
        {
            if (!IsStatic)
            {
                idx--;
            }
            methodParameters[idx - 1].name = name;
        }

        public AsmILGenerator GetILGenerator()
        {
            return ilGenerator;
        }


        public IType[] GetParameters()
        {
            return MethodParameters.Select(x => x.type).ToArray();
        }

        public void SetImplementationFlags(MethodImplAttributes attributes)
        {
            MethodImplAttributes = attributes;
        }

        IILGenerator IBaseMethodBuilder.GetILGenerator()
        {
            return ilGenerator;
        }

    }
}
