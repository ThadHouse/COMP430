using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.EmitBuilders
{
    public class EmitFieldBuilder : EmitFieldInfo, IFieldBuilder
    {
        public FieldBuilder FieldBuilder { get; }

        public EmitFieldBuilder(FieldBuilder fieldInfo, IType fieldType) : base(fieldInfo, fieldType)
        {
            FieldBuilder = fieldInfo;
        }
    }
}
