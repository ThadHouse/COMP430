using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.EmitBuilders
{
    public class EmitLabel : ILabel
    {
        public Label Label { get; }

        public EmitLabel(Label label)
        {
            Label = label;
        }
    }
}
