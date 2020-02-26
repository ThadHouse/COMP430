using System;
using System.Collections.Generic;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.IlAsmBuilders
{
    public class AsmLabel : ILabel
    {
        public int Idx { get; }
        public AsmLabel(int idx)
        {
            Idx = idx;
        }
    }
}
