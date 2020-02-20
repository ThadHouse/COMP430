using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.Parser.Nodes;

namespace Compiler.CodeGeneration
{
    public interface ICodeGenerator
    {
        MethodInfo? GenerateAssembly(IReadOnlyList<(TypeBuilder typeBuilder, TypeDefinitionNode syntax)> types);
    }
}
