﻿using System;
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

        public EmitModuleBuilder(ModuleBuilder moduleBuilder)
        {
            this.moduleBuilder = moduleBuilder;
        }

        public ITypeBuilder DefineType(string type, TypeAttributes typeAttributes, Type? baseType = null)
        {
            return new EmitTypeBuilder(moduleBuilder.DefineType(type, typeAttributes, baseType));
        }


    }
}
