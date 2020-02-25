using System;
using System.Collections.Generic;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.IlAsmBuilders
{
    public class AsmLocalBuilder : ILocalBuilder
    {
        public IType LocalType { get; }

        public int LocalIndex { get; }

        public string Name { get; }

        public AsmLocalBuilder(IType localType, int localIndex, string name)
        {
            LocalType = localType;
            LocalIndex = localIndex;
            Name = name;
        }
    }
}
