using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.EmitBuilders
{
    public class EmitLocalBuilder : ILocalBuilder
    {
        public LocalBuilder LocalBuilder { get; }

        public IType LocalType { get; }

        public int LocalIndex => LocalBuilder.LocalIndex;

        public EmitLocalBuilder(LocalBuilder localBuilder, IType localType)
        {
            LocalBuilder = localBuilder;
            LocalType = localType;
        }
    }
}
