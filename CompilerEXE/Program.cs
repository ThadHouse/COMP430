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
using Compiler.Parser;
using Compiler.Parser.Nodes;
using Compiler.Tokenizer;
using Compiler.TypeChecker;

namespace CompilerEXE
{
    public class Program
    {
        static Assembly[] Assemblies = Array.Empty<Assembly>();

        static (IType[] delegateConstructorTypes, IType voidType, IConstructorInfo baseInfo) GenerateAssemblyTypes(CodeGenerationStore store)
        {
            foreach (var assembly in Assemblies)
            {
                var types = assembly.GetTypes().Where(x => x.IsPublic).Select(x => new EmitType(x));

                foreach (var type in types)
                {

                    store.Types.Add(type.FullName, type);
                }
            }

            foreach (var type in store.Types.Values)
            {

                var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

                store.Fields.Add(type, fieldInfos);

                var methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                store.Methods.Add(type, methodInfos);

                foreach (var method in methodInfos)
                {
                    store.MethodParameters.Add(method, method.GetParameters());
                }

                var constructorInfos = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

                store.Constructors.Add(type, constructorInfos);

                foreach (var constructor in constructorInfos)
                {
                    store.ConstructorParameters.Add(constructor, constructor.GetParameters().ToArray());
                }

            }

            store.Types.Clear();
            foreach (var type in EmitType.TypeCache)
            {
                if (type.Key.FullName != null)
                {
                    store.Types.Add(type.Key.FullName, type.Value);
                }
            }

            var delegateConstructorTypes = new IType[] { store.Types["System.Object"], store.Types["System.IntPtr"] };
            var voidType = store.Types["System.Void"];
            var objConstructorArr = store.Types["System.Object"];
            var objConstructor = store.Types["System.Object"].GetConstructors(BindingFlags.Public | BindingFlags.Instance)[0];
            return (delegateConstructorTypes, voidType, objConstructor);
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

            var rootNode = new RootSyntaxNode();

            var tokenizer = new SimpleTokenizer();
            var parser = new SimpleParser();
            var codeGenerator = new NewCodeGenerator(GenerateAssemblyTypes);

            foreach (var file in args)
            {
                var code = File.ReadAllText(file);
                var fileTokens = tokenizer.EnumerateTokens(code.AsSpan());
                tracer.AddEpoch($"Tokenizing {file}");
                parser.ParseTokens(fileTokens, rootNode);
                tracer.AddEpoch($"Parsing {file}");
            }

            Assemblies = assemblies;

            var createdAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(programName), AssemblyBuilderAccess.RunAndSave);
            var createdModule = createdAssembly.DefineDynamicModule(programName, programName + ".exe");

            var emitModuleBuilder = new EmitModuleBuilder(createdModule);

            var entryPoint = codeGenerator.GenerateAssembly(rootNode, emitModuleBuilder, tracer);

            tracer.AddEpoch("Code Generation");

            if (entryPoint == null)
            {
                throw new InvalidOperationException("Entry point must be null");
            }

            createdAssembly.SetEntryPoint(((EmitMethodInfo)entryPoint).MethodInfo);

            createdAssembly.Save(programName + ".exe");

            tracer.PrintEpochs();
        }
    }
}
