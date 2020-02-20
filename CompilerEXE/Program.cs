﻿using System;
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

            var code = @"
delegate void myFunc(int a, ref string b); delegate void otherfunc(); 

class A::B::MyClass { 
    constructor() {
    }

    method static int StaticMethod() {
        return 42;
    }

    method entrypoint void Main() {
        System::Console.WriteLine(""Hello World!"");
        System::Console.WriteLine(A::B::MyClass.StaticMethod());
    }

    method int myMethod() { 
        auto a = 42; 
        a = a * 36;
        return a + 5;
    }

    field int x = 5 + 3 + 6;
    field string val = ""hello""; 
    field string c = x.ToString(); 
} 

class OtherClass { 
    field string g = new string(42); 
}
";

            var tokens = tokenizer.EnumerateTokens(code.AsSpan());

            var ast = parser.ParseTokens(tokens);

            string assemblyName = "HelloWorld";
            var createdAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndSave);
            var createdModule = createdAssembly.DefineDynamicModule(assemblyName, assemblyName + ".exe");

            var types = typeChecker.GenerateTypes(ast, createdModule);

            var entryPoint = codeGenerator.GenerateAssembly(types);

            if (entryPoint == null)
            {
                throw new InvalidOperationException("Entry point must be null");
            }

            createdAssembly.SetEntryPoint(entryPoint);

            createdAssembly.Save("HelloWorld.exe");

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
