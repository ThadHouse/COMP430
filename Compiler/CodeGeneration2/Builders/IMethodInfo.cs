using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Builders
{
    public interface IMethodInfo
    {
        IType ReturnType { get; }

        bool IsStatic { get; }

        string Name { get; }

        IType DeclaringType { get; }

        IType[] GetParameters();
    }
}
