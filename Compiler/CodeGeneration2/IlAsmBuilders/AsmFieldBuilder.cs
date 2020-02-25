using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.IlAsmBuilders
{
    public class AsmFieldBuilder : IFieldBuilder
    {
        public string Name { get; }

        public IType FieldType { get; }

        public FieldAttributes FieldAttributes { get; }

        public IType DeclaringType { get; }

        public AsmFieldBuilder(IType declaringType, IType fieldType, FieldAttributes fieldAttributes, string name)
        {
            DeclaringType = declaringType;
            Name = name;
            FieldType = fieldType;
            FieldAttributes = fieldAttributes;
        }
    }
}
