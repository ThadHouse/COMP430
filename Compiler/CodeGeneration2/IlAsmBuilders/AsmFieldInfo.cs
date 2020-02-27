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

        public IType DeclaringType { get; }

        public bool IsStatic { get; }

        public AsmFieldInfo(IType declaringType, string name, IType fieldType, bool isStatic)
        {
            DeclaringType = declaringType;
            FieldType = fieldType;
            Name = name;
            IsStatic = isStatic;
        }
    }
}
