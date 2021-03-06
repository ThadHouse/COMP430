﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Compiler.CodeGeneration2.Builders
{
    public interface IModuleBuilder
    {
        ITypeBuilder DefineType(string type, TypeAttributes typeAttributes, Type? baseType = null);

        IBuiltInTypeProvider BuiltInTypeProvider { get; }
    }
}
