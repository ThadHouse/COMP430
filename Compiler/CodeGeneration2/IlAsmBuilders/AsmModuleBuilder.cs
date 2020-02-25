using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.IlAsmBuilders
{
    public class AsmModuleBuilder : IModuleBuilder
    {
        public string ModuleName { get; }

        public AsmModuleBuilder(string moduleName)
        {
            ModuleName = moduleName;
        }

        public ITypeBuilder DefineType(string type, TypeAttributes typeAttributes, Type? baseType = null)
        {
            return new AsmTypeBuilder(ModuleName, type, false, (baseType != null) ? baseType.FullName : typeof(object).FullName);
        }
    }
}
