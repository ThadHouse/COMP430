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

            var code = @"
delegate void myFunc(int a, ref string b); delegate void otherfunc(); 

class A::B::MyClass { 
    constructor() {
    }

    method int myMethod() { 
        return 42; 
        return ""hello""; 
        auto a = 42; 
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
            var createdModule = createdAssembly.DefineDynamicModule(assemblyName, assemblyName + ".dll");

            var types = typeChecker.GenerateTypes(ast, createdModule);

            codeGenerator.GenerateAssembly(types);

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
