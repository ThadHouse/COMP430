using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.EmitBuilders
{
    public class EmitConstructorInfo : IConstructorInfo
    {
        public ConstructorInfo ConstructorInfo { get; }

        private readonly IType[] parameters;

        public EmitConstructorInfo(ConstructorInfo constructorInfo, IType[] parameters)
        {
            ConstructorInfo = constructorInfo;
            this.parameters = parameters;
        }

        public IType[] GetParameters()
        {
            return parameters;
        }
    }
}
