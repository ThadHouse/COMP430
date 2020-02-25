﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Builders
{
    public interface IConstructorInfo
    {
        IType[] GetParameters();

        IType DeclaringType { get; }
    }
}
