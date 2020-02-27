using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.EmitBuilders
{
    public class EmitModuleBuilder : IModuleBuilder
    {
        private readonly ModuleBuilder moduleBuilder;

        public EmitModuleBuilder(ModuleBuilder moduleBuilder, Assembly[] dependentAssemblies)
        {
            this.moduleBuilder = moduleBuilder;
            BuiltInTypeProvider = new EmitBuiltInTypeProvider(dependentAssemblies);
        }

        public IBuiltInTypeProvider BuiltInTypeProvider { get; }

        public ITypeBuilder DefineType(string type, TypeAttributes typeAttributes, Type? baseType = null)
        {
            return new EmitTypeBuilder(moduleBuilder.DefineType(type, typeAttributes, baseType));
        }


    }
}
