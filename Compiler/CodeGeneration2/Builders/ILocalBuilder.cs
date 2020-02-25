using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Builders
{
    public interface ILocalBuilder
    {
        IType LocalType { get; }

        int LocalIndex { get; }

        string Name { get; }
    }
}
