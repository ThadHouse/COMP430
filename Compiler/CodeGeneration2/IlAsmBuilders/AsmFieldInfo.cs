using System;
using System.Collections.Generic;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.IlAsmBuilders
{
    public class AsmFieldInfo : IFieldInfo
    {

        public string Name { get; }

        public IType FieldType { get; }

        public AsmFieldInfo(string name, IType fieldType)
        {
            FieldType = fieldType;
            Name = name;
        }
    }
}
