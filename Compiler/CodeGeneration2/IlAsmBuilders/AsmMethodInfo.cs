using System;
using System.Collections.Generic;
using System.Text;
using Compiler.CodeGeneration2.Builders;

using SRMethodInfo = System.Reflection.MethodInfo;

namespace Compiler.CodeGeneration2.IlAsmBuilders
{
    public class AsmMethodInfo : IMethodInfo
    {
        public IType ReturnType { get; }

        public bool IsStatic => methodInfo.IsStatic;

        public string Name => methodInfo.Name;

        private SRMethodInfo methodInfo { get; }

        public IType DeclaringType { get; }

        private readonly IType[] parameters;

        public AsmMethodInfo(IType declaringType, SRMethodInfo methodInfo, IType returnType, IType[] parameters)
        {
            DeclaringType = declaringType;
            this.methodInfo = methodInfo;
            ReturnType = returnType;
            this.parameters = parameters;
        }

        public IType[] GetParameters()
        {
            return parameters;
        }
    }
}
