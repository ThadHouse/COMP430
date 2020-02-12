using Compiler.FSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Reflection;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {



            var createdAsm = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("MyApplication", new Version(1, 0)), "MyApplication", ModuleKind.Console);
            var module = createdAsm.MainModule;

            var programType = new TypeDefinition("MyApplication", "Program", Mono.Cecil.TypeAttributes.Class | Mono.Cecil.TypeAttributes.Public, module.TypeSystem.Object);

            module.Types.Add(programType);

            var ctor = new MethodDefinition(".ctor", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.SpecialName | Mono.Cecil.MethodAttributes.RTSpecialName, module.TypeSystem.Void);
            var il = ctor.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ldarg_0));

            il.Append(il.Create(OpCodes.Call, module.ImportReference(typeof(object).GetConstructor(Array.Empty<Type>()))));
            il.Append(il.Create(OpCodes.Nop));
            il.Append(il.Create(OpCodes.Ret));

            programType.Methods.Add(ctor);

        }
    }
}
