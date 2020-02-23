using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.EmitBuilders
{
    public class EmitMethodInfo : IMethodInfo
    {
        public IType ReturnType { get; }

        public bool IsStatic => MethodInfo.IsStatic;

        public string Name => MethodInfo.Name;

        public MethodInfo MethodInfo { get; }

        private readonly IType[] parameters;

        public EmitMethodInfo(MethodInfo methodInfo, IType returnType, IType[] parameters)
        {
            MethodInfo = methodInfo;
            ReturnType = returnType;
            this.parameters = parameters;
        }

        public IType[] GetParameters()
        {
            return parameters;
        }
    }
}
