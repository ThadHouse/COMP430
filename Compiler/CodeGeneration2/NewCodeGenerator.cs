using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.CodeGeneration2.Builders;
using Compiler.CodeGeneration2.Exceptions;
using Compiler.Parser.Nodes;
using Compiler.Parser.Nodes.Statements;
using Compiler.TypeChecker;

namespace Compiler.CodeGeneration2
{

    public class NewCodeGenerator
    {
        private IType[]? delegateConstructorTypes;
        private SimpleTypeChecker? typeChecker;
        private IConstructorInfo? baseConstructorInfo;
        private readonly IModuleBuilder moduleBuilder;
        private readonly Tracer tracer;

        public NewCodeGenerator(IModuleBuilder moduleBuilder,
            Tracer tracer)
        {
            this.moduleBuilder = moduleBuilder;
            this.tracer = tracer;
        }

        private GeneratedData CreateTypesToGenerate(IReadOnlyList<ImmutableRootSyntaxNode> rootNodes, CodeGenerationStore store)
        {
            var toGenerate = new GeneratedData();

            foreach (var rootNode in rootNodes)
            {

                foreach (var node in rootNode.Delegates)
                {
                    var type = moduleBuilder.DefineType(node.Name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.AutoLayout, typeof(MulticastDelegate));

                    toGenerate.Delegates.Add(type, node);

                    store.Types.Add(node.Name, type);
                }

                foreach (var node in rootNode.Classes)
                {
                    var type = moduleBuilder.DefineType(node.Name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.AutoLayout);

                    store.Types.Add(node.Name, type);

                    toGenerate.Classes.Add(type, node);
                }
            }

            return toGenerate;
        }

        private void GenerateDelegates(GeneratedData toGenerate, CodeGenerationStore store)
        {
            foreach (var delegateToGenerate in toGenerate.Delegates)
            {
                var type = delegateToGenerate.Key;
                var syntaxNode = delegateToGenerate.Value;

                store.Fields.Add(type, Array.Empty<IFieldInfo>());

                var constructor = type.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, delegateConstructorTypes!);
                constructor.SetImplementationFlags(MethodImplAttributes.Runtime);

                store.Constructors.Add(type, new IConstructorInfo[] { constructor });

                store.ConstructorParameters.Add(constructor, delegateConstructorTypes!);

                var parameterTypes = new IType[syntaxNode.Parameters.Count];

                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    var paramType = store.TypeDefLookup(syntaxNode.Parameters[i].Type);
                    if (syntaxNode.Parameters[i].IsRef)
                    {
                        throw new RefTypeException("Ref types are not supported");
                    }
                    typeChecker!.AssertTypeIsNotVoid(paramType);
                    parameterTypes[i] = paramType;
                }

                var returnType = store.TypeDefLookup(syntaxNode.ReturnType);

                var method = type.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot, returnType, parameterTypes, false);
                method.SetImplementationFlags(MethodImplAttributes.Runtime);

                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    method.DefineParameter(i + 1, ParameterAttributes.None, syntaxNode.Parameters[i].Name);
                }

                store.Methods.Add(type, new IMethodInfo[] { method });

                store.MethodParameters.Add(method, parameterTypes);

                store.Delegates.Add((method.ReturnType, parameterTypes, type, constructor));
            }
        }

        private IReadOnlyList<StatementSyntaxNode> GenerateClassFields(ITypeBuilder type, IReadOnlyList<FieldSyntaxNode> fields, CodeGenerationStore store,
            ISyntaxNode syntaxNode)
        {
            var definedFields = new List<IFieldInfo>();
            var initExpressions = new List<StatementSyntaxNode>();
            store.Fields.Add(type, definedFields);
            foreach (var field in fields)
            {
                var fieldType = store.TypeDefLookup(field.Type);

                typeChecker!.AssertTypeIsNotVoid(fieldType);

                var definedField = type.DefineField(field.Name, fieldType, FieldAttributes.Public);
                definedFields.Add(definedField);
                if (field.Expression != null)
                {
                    initExpressions.Add(new ExpressionEqualsExpressionSyntaxNode(field, new VariableSyntaxNode(field, field.Name), field.Expression));
                }
            }

            initExpressions.Add(new BaseClassConstructorSyntax(syntaxNode));

            return initExpressions;
        }

        private void GenerateClassMethods(ITypeBuilder type, IReadOnlyList<MethodSyntaxNode> methods, CodeGenerationStore store,
            Dictionary<IMethodBuilder, MethodSyntaxNode> methodsDictionary, ref IMethodInfo? entryPoint)
        {
            var definedMethods = new List<IMethodInfo>();
            store.Methods.Add(type, definedMethods);

            foreach (var method in methods)
            {
                var methodAttributes = MethodAttributes.Public;
                if (method.IsStatic)
                {
                    methodAttributes |= MethodAttributes.Static;
                }

                var parameters = method.Parameters.Select(x =>
                {
                    var tpe = store.TypeDefLookup(x.Type);
                    if (x.IsRef)
                    {
                        throw new RefTypeException("Ref types are not supported");
                    }

                    typeChecker!.AssertTypeIsNotVoid(tpe);
                    return tpe;
                }).ToArray();

                var arrType = typeof(int[]);

                var returnType = store.TypeDefLookup(method.ReturnType);

                var definedMethod = type.DefineMethod(method.Name, methodAttributes, returnType, parameters, method.IsEntryPoint);

                if (method.IsEntryPoint)
                {
                    if (entryPoint != null)
                    {
                        throw new EntryPointException("Can only have 1 entry point");
                    }
                    entryPoint = definedMethod;
                }

                definedMethods.Add(definedMethod);

                store.MethodParameters.Add(definedMethod, parameters);
                methodsDictionary.Add(definedMethod, method);

                int offset = 0;
                //if (!method.IsStatic)
                //{
                //    offset = 1;
                //}

                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    definedMethod.DefineParameter(i + 1 + offset, ParameterAttributes.None, method.Parameters[i].Name);
                }
            }
        }

        private IReadOnlyList<ConstructorSyntaxNode> GenerateClassConstructors(ITypeBuilder type, IReadOnlyList<ConstructorSyntaxNode> constructors, CodeGenerationStore store,
            IReadOnlyList<StatementSyntaxNode> fieldInitializers,
            Dictionary<IConstructorBuilder, ConstructorSyntaxNode> constructorsDictionary, ISyntaxNode parent)
        {
            var initialConstructors = new List<ConstructorSyntaxNode>(constructors);
            var mutatedConstructors = new List<ConstructorSyntaxNode>();

            var definedConstructors = new List<IConstructorInfo>();
            store.Constructors.Add(type, definedConstructors);

            if (constructors.Count == 0)
            {
                var statementList = new List<StatementSyntaxNode>();
                initialConstructors.Add(new ConstructorSyntaxNode(parent, Array.Empty<ParameterDefinitionSyntaxNode>(), statementList));
            }

            foreach (var constructor in initialConstructors)
            {
                var newStatements = new List<StatementSyntaxNode>(fieldInitializers);
                foreach (var toAdd in constructor.Statements)
                {
                    newStatements.Add(toAdd);
                }

                var mutatedConstructor = constructor.MutateStatements(newStatements);
                mutatedConstructors.Add(mutatedConstructor);

                var methodAttributes = MethodAttributes.Public;

                var parameters = mutatedConstructor.Parameters.Select(x =>
                {
                    var tpe = store.TypeDefLookup(x.Type);
                    if (x.IsRef)
                    {
                        throw new RefTypeException("Ref types are not supported");
                    }
                    typeChecker!.AssertTypeIsNotVoid(tpe);
                    return tpe;
                }).ToArray();

                var definedConstructor = type.DefineConstructor(methodAttributes, parameters);

                definedConstructors.Add(definedConstructor);

                store.ConstructorParameters.Add(definedConstructor, parameters);
                constructorsDictionary.Add(definedConstructor, mutatedConstructor);

                int offset = 0;

                for (int i = 0; i < mutatedConstructor.Parameters.Count; i++)
                {
                    definedConstructor.DefineParameter(i + 1 + offset, ParameterAttributes.None, mutatedConstructor.Parameters[i].Name);
                }
            }
            return mutatedConstructors;
        }

        private void GenerateClassPlaceholders(GeneratedData toGenerate, CodeGenerationStore store, ref IMethodInfo? entryPoint)
        {
            var patchedClasses = new Dictionary<ITypeBuilder, ImmutableClassSyntaxNode>();

            foreach (var classToGenerate in toGenerate.Classes)
            {
                var type = classToGenerate.Key;
                var node = classToGenerate.Value;
                var fieldsToInitialize = GenerateClassFields(classToGenerate.Key, node.Fields, store, node);

                var patchedConstructors = GenerateClassConstructors(type, node.Constructors, store, fieldsToInitialize, toGenerate.Constructors, node);
                patchedClasses.Add(type, node.MutateConstructors(patchedConstructors));

                GenerateClassMethods(type, node.Methods, store, toGenerate.Methods, ref entryPoint);
            }

            foreach (var patchedClass in patchedClasses)
            {
                toGenerate.Classes[patchedClass.Key] = patchedClass.Value;
            }
            ;
        }

        private void GenerateMethod(ILGeneration generator, IReadOnlyList<StatementSyntaxNode> statements)
        {
            bool wasLastReturn = false;

            foreach (var stmt in statements)
            {
                wasLastReturn = generator.WriteStatement(stmt);
            }

            if (!wasLastReturn)
            {
                generator.EmitRet();
            }
        }

        private void GenerateMethods(GeneratedData toGenerate, CodeGenerationStore store)
        {
            foreach (var cls in toGenerate.Classes)
            {
                var type = cls.Key;
                var node = cls.Value;

                var fields = store.Fields[type].Select(x => (x, x.Name))
                    .ToDictionary(x => x.Name, x => x.x);

                foreach (IMethodBuilder method in store.Methods[type])
                {
                    var generator = method.GetILGenerator();

                    int offset = method.IsStatic ? 0 : 1;

                    var parameters = toGenerate.Methods[method].Parameters
                        .Select((p, i) => (i + offset, store.Types[p.Type], p.Name))
                        .ToDictionary(x => x.Name, x => ((short)x.Item1, x.Item2));



                    var methodInfo = new CurrentMethodInfo(type, method.ReturnType, method.IsStatic,
                        parameters, fields);

                    var generation = new ILGeneration(generator, store, methodInfo, delegateConstructorTypes!, baseConstructorInfo!, typeChecker!);



                    GenerateMethod(generation, toGenerate.Methods[method].Statements);
                }
            }
        }

        private void GenerateConstructors(GeneratedData toGenerate, CodeGenerationStore store)
        {
            foreach (var cls in toGenerate.Classes)
            {
                var type = cls.Key;
                var node = cls.Value;

                var fields = store.Fields[type].Select(x => (x, x.Name))
                    .ToDictionary(x => x.Name, x => x.x);

                foreach (IConstructorBuilder constructor in store.Constructors[type])
                {
                    var generator = constructor.GetILGenerator();

                    var parameters = toGenerate.Constructors[constructor].Parameters
                        .Select((p, i) => (i, store.Types[p.Type], p.Name))
                        .ToDictionary(x => x.Name, x => ((short)x.i, x.Item2));



                    var methodInfo = new CurrentMethodInfo(type, typeChecker!.VoidType, false,
                        parameters, fields);

                    var generation = new ILGeneration(generator, store, methodInfo, delegateConstructorTypes!, baseConstructorInfo!, typeChecker!);

                    GenerateMethod(generation, toGenerate.Constructors[constructor].Statements);
                }
            }
        }

        public IMethodInfo? GenerateAssembly(IReadOnlyList<ImmutableRootSyntaxNode> rootNodes)
        {
            if (rootNodes == null)
            {
                throw new ArgumentNullException(nameof(rootNodes));
            }

            var store = new CodeGenerationStore();

            IType voidType;

            (delegateConstructorTypes, voidType, baseConstructorInfo) = moduleBuilder.BuiltInTypeProvider.GenerateAssemblyTypes(store);

            typeChecker = new SimpleTypeChecker(voidType);

            tracer.AddEpoch("Dependent Type Load");

            var toGenerate = CreateTypesToGenerate(rootNodes, store);

            tracer.AddEpoch("Generate Types");

            GenerateDelegates(toGenerate, store);

            tracer.AddEpoch("Generate Delegates");

            IMethodInfo? methodInfo = null;

            GenerateClassPlaceholders(toGenerate, store, ref methodInfo);

            tracer.AddEpoch("Generate Class Definitions");

            GenerateMethods(toGenerate, store);

            tracer.AddEpoch("Generate Class Methods");

            GenerateConstructors(toGenerate, store);

            tracer.AddEpoch("Generate Class Constructors");

            foreach (var type in store.Types.Values)
            {
                if (type is ITypeBuilder tb)
                {
                    tb.CreateTypeInfo();
                }
            }

            tracer.AddEpoch("Create Type Infos");

            return methodInfo;
        }
    }
}
