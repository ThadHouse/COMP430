using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Compiler;
using Compiler.CodeGeneration2;
using Compiler.CodeGeneration2.Builders;
using Compiler.CodeGeneration2.EmitBuilders;
using Compiler.CodeGeneration2.IlAsmBuilders;
using Compiler.Parser;
using Compiler.Parser.Nodes;
using Compiler.Tokenizer;
using Compiler.TypeChecker;

namespace CompilerEXE
{
    public class Program
    {
        static void IlAsmMain(string programName, Assembly[] assemblies, Tracer tracer, IReadOnlyList<ImmutableRootSyntaxNode> rootNode)
        {
            var emitModuleBuilder = new AsmModuleBuilder(programName, assemblies);

            var codeGenerator = new NewCodeGenerator(emitModuleBuilder, tracer);

            var entryPoint = codeGenerator.GenerateAssembly(rootNode);


            if (entryPoint == null)
            {
                throw new InvalidOperationException("Entry point must be null");
            }

            var asm = emitModuleBuilder.Emitter.Finalize();
            File.WriteAllLines($"{programName}.il", asm);

            tracer.AddEpoch("Code Generation");

        }

        static void EmitMain(string programName, Assembly[] assemblies, Tracer tracer, IReadOnlyList<ImmutableRootSyntaxNode> rootNodes)
        {
            var createdAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(programName), AssemblyBuilderAccess.RunAndSave);
            var createdModule = createdAssembly.DefineDynamicModule(programName, programName + ".exe");

            var emitModuleBuilder = new EmitModuleBuilder(createdModule, assemblies);

            var codeGenerator = new NewCodeGenerator(emitModuleBuilder, tracer);

            var entryPoint = codeGenerator.GenerateAssembly(rootNodes);

            //            System.Console.Out.WriteLine("Hello World from Out!");


            if (entryPoint == null)
            {
                throw new InvalidOperationException("Entry point must be null");
            }

            createdAssembly.SetEntryPoint(((EmitMethodInfo)entryPoint).MethodInfo);

            createdAssembly.Save(programName + ".exe");

            tracer.AddEpoch("Code Generation");
        }

        static void Main(string? programName = null, string[]? args = null)
        {
            var tracer = new Tracer();
            tracer.Restart();

            if (args == null)
            {
                throw new InvalidOperationException("You must pass in a file");
            }

            var libraries = args.Where(x =>
            {
                var ext = Path.GetExtension(x);
                return ext == ".exe" || ext == ".dll";
            }).ToArray();

            args = args.Except(libraries).ToArray();

            if (args.Length == 0)
            {
                throw new InvalidOperationException("You must pass in a file to actually compile");
            }

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            Assembly[] assemblies = libraries.Select(x =>
            {
                try
                {
                    return Assembly.LoadFrom(x);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    return null;
                }
            }).Where(x => x != null).Append(typeof(object).Assembly).ToArray();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

            if (programName == null)
            {
                programName = Path.GetFileNameWithoutExtension(args[0]);
            }

            tracer.AddEpoch("Initialization");

            var tokenizer = new SimpleTokenizer();
            var parser = new SimpleParser();

            var rootNodes = new List<ImmutableRootSyntaxNode>();

            foreach (var file in args)
            {
                var code = File.ReadAllText(file);
                var fileTokens = tokenizer.EnumerateTokens(code.AsSpan());
                tracer.AddEpoch($"Tokenizing {file}");
                var immutableNode = parser.ParseTokens(fileTokens);
                rootNodes.Add(immutableNode);
                tracer.AddEpoch($"Parsing {file}");
            }

            EmitMain(programName + "Emit", assemblies, tracer, rootNodes);

            // Skip the ilasm backend for demonstation.
            //IlAsmMain(programName, assemblies, tracer, rootNodes);



            tracer.PrintEpochs();
            Console.WriteLine("Compilation Complete!");
        }
    }
}
