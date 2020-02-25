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

        private readonly (IType type, string name)[] parameters;
        private readonly AsmILGenerator ilGenerator;
        public MethodImplAttributes MethodImplAttributes { get; private set; } = 0;

        public IType DeclaringType { get; }

        public AsmMethodBuilder(IType declaringType, IType returnType, IType[] parameters, MethodAttributes methodAttributes, string name)
        {
            DeclaringType = declaringType;
            ReturnType = returnType;
            Name = name;
            MethodAttributes = methodAttributes;
            this.parameters = parameters.Select(x => (x, "")).ToArray();
            ilGenerator = new AsmILGenerator();
        }

        public void DefineParameter(int idx, ParameterAttributes parameterAttributes, string name)
        {
            if (!IsStatic)
            {
                idx--;
            }
            parameters[idx - 1].name = name;
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
