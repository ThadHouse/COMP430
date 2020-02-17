using System;
using System.Reflection;
using System.Reflection.Emit;
using Compiler.CodeGeneration;
using Compiler.Parser;
using Compiler.Tokenizer;
using Compiler.TypeChecker;

namespace CompilerEXE
{
    class Program
    {
        static void Main()
        {
            var tokenizer = new SimpleTokenizer();
            var parser = new SimpleParser();
            var typeChecker = new SimpleTypeChecker();
            var codeGenerator = new CodeGenerator();

            var code = "delegate void myFunc(int a, ref string b); class A::B::MyClass { field int x = 5 + 3 + 6; field string val = \"hello\"; } class OtherClass {}";

            var tokens = tokenizer.EnumerateTokens(code.AsSpan());

            var ast = parser.ParseTokens(tokens);

            var types = typeChecker.TypeCheck(ast);

            string assemblyName = "HelloWorld";
            var createdAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), (AssemblyBuilderAccess)3); // 3 is run and save, not exposed in NETStandard
            var createdModule = createdAssembly.DefineDynamicModule(assemblyName, assemblyName + ".dll");

            codeGenerator.GenerateAssembly(types, createdModule);

            createdAssembly.Save("HelloWorld.dll");

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
