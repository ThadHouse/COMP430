using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Compiler.CodeGeneration2.Builders
{
    public interface IBaseMethodBuilder
    {
        IILGenerator GetILGenerator();

        void DefineParameter(int idx, ParameterAttributes parameterAttributes, string name);

        void SetImplementationFlags(MethodImplAttributes attributes);
    }
}
