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

        public string Name { get; }

        public EmitLocalBuilder(LocalBuilder localBuilder, IType localType, string name)
        {
            LocalBuilder = localBuilder;
            LocalType = localType;
            Name = name;
        }
    }
}
