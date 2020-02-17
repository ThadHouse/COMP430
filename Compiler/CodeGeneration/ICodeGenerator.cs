using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Compiler.Parser.Nodes;

namespace Compiler.CodeGeneration
{
    public interface ICodeGenerator
    {
        void GenerateAssembly(IReadOnlyDictionary<string, TypeDefinitionNode> types, ModuleBuilder moduleBuilder);
    }
}
