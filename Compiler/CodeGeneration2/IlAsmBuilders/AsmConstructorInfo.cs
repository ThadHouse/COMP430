using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.IlAsmBuilders
{
    public class AsmConstructorInfo : IConstructorInfo
    {
        private readonly IType[] parameters;

        public AsmConstructorInfo(IType[] parameters)
        {
            this.parameters = parameters;
        }

        public IType[] GetParameters()
        {
            return parameters;
        }
    }
}
