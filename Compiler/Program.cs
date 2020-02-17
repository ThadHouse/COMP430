using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {

            

            var createdAsm = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("MyApplication"), (AssemblyBuilderAccess)3); // 3 is run and save, not exposed in NETStandard
            var module = createdAsm.DefineDynamicModule("MyModule");

            var programType = module.DefineType("Program", TypeAttributes.Public | TypeAttributes.Class);

            var ctor = programType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null);
            var il = ctor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Call, typeof(object).GetConstructor(Array.Empty<Type>())); 

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ret);

        }
    }
}
