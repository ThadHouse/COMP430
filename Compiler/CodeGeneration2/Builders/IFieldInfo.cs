using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Builders
{
    public interface IFieldInfo
    {
        string Name { get; }

        IType FieldType { get; }
    }
}
