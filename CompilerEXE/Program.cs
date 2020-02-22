using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Compiler;
using Compiler.CodeGeneration;
using Compiler.CodeGeneration2;
using Compiler.Parser;
using Compiler.Parser.Nodes;
using Compiler.Tokenizer;
using Compiler.TypeChecker;

namespace CompilerEXE
{
    public class Program
    {
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

            var rootNode = new RootSyntaxNode();

            var tokenizer = new SimpleTokenizer();
            var parser = new SimpleParser();
            var codeGenerator = new NewCodeGenerator();

            foreach (var file in args)
            {
                var code = File.ReadAllText(file);
                var fileTokens = tokenizer.EnumerateTokens(code.AsSpan());
                tracer.AddEpoch($"Tokenizing {file}");
                parser.ParseTokens(fileTokens, rootNode);
                tracer.AddEpoch($"Parsing {file}");
            }

            var createdAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(programName), AssemblyBuilderAccess.RunAndSave);
            var createdModule = createdAssembly.DefineDynamicModule(programName, programName + ".exe");

            var entryPoint = codeGenerator.GenerateAssembly(rootNode, createdModule, assemblies, tracer);

            tracer.AddEpoch("Code Generation");

            if (entryPoint == null)
            {
                throw new InvalidOperationException("Entry point must be null");
            }

            createdAssembly.SetEntryPoint(entryPoint);

            createdAssembly.Save(programName + ".exe");

            tracer.PrintEpochs();
        }
    }
}
