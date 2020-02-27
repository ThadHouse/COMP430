using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.EmitBuilders
{
    public class EmitFieldInfo : IFieldInfo
    {
        public FieldInfo FieldInfo { get; }

        public string Name => FieldInfo.Name;

        public IType FieldType { get; }

        public IType DeclaringType => throw new NotSupportedException();

        public bool IsStatic => FieldInfo.IsStatic;

        public EmitFieldInfo(FieldInfo fieldInfo, IType fieldType)
        {
            FieldType = fieldType;
            FieldInfo = fieldInfo;
        }
    }
}
