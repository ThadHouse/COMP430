using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Builders
{
    public interface IBuiltInTypeProvider
    {
        (IType[] delegateConstructorTypes, IType voidType, IConstructorInfo objectConstructorInfo) GenerateAssemblyTypes(CodeGenerationStore store);
    }
}
