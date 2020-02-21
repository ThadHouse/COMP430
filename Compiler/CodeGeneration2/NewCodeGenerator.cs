using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.CodeGeneration;
using Compiler.Parser.Nodes;
using Compiler.Parser.Nodes.Statements;

namespace Compiler.CodeGeneration2
{
    public class CodeGenerationStore
    {
        public List<(Type returnType, Type[] parameters, Type type, ConstructorBuilder constructor)> Delegates { get; } = new List<(Type returnType, Type[] parameters, Type type, ConstructorBuilder constructor)>();

        public Dictionary<string, Type> Types { get; } = new Dictionary<string, Type>();
        public Dictionary<Type, IReadOnlyList<FieldInfo>> Fields { get; } = new Dictionary<Type, IReadOnlyList<FieldInfo>>();
        public Dictionary<Type, IReadOnlyList<MethodInfo>> Methods { get; } = new Dictionary<Type, IReadOnlyList<MethodInfo>>();
        public Dictionary<MethodInfo, IReadOnlyList<Type>> MethodParameters { get; } = new Dictionary<MethodInfo, IReadOnlyList<Type>>();

        public Dictionary<Type, IReadOnlyList<ConstructorInfo>> Constructors { get; } = new Dictionary<Type, IReadOnlyList<ConstructorInfo>>();
        public Dictionary<ConstructorInfo, IReadOnlyList<Type>> ConstructorParameters { get; } = new Dictionary<ConstructorInfo, IReadOnlyList<Type>>();
    }

    public class GeneratedData
    {
        public Dictionary<TypeBuilder, ClassSyntaxNode> Classes { get; } = new Dictionary<TypeBuilder, ClassSyntaxNode>();
        public Dictionary<TypeBuilder, DelegateSyntaxNode> Delegates { get; } = new Dictionary<TypeBuilder, DelegateSyntaxNode>();

        public Dictionary<MethodBuilder, MethodSyntaxNode> Methods { get; } = new Dictionary<MethodBuilder, MethodSyntaxNode>();

        public Dictionary<ConstructorBuilder, ConstructorSyntaxNode> Constructors { get; } = new Dictionary<ConstructorBuilder, ConstructorSyntaxNode>();
    }

    public class NewCodeGenerator
    {
        private readonly Type[] delegateConstructorTypes = new Type[] { typeof(object), typeof(IntPtr) };

        private void WriteDependentTypes(CodeGenerationStore store, Assembly[] dependentAssemblies)
        {
            foreach (var assembly in dependentAssemblies)
            {
                var types = assembly.GetTypes().Where(x => x.IsPublic);

                foreach (var type in types)
                {

                    store.Types.Add(type.FullName, type);
                    store.Fields.Add(type, type.GetFields(BindingFlags.Public | BindingFlags.Instance));

                    var methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                    store.Methods.Add(type, methodInfos);

                    foreach (var method in methodInfos)
                    {
                        store.MethodParameters.Add(method, method.GetParameters().Select(x => x.ParameterType).ToArray());
                    }

                    var constructorInfos = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

                    store.Constructors.Add(type, constructorInfos);

                    foreach (var constructor in constructorInfos)
                    {
                        store.ConstructorParameters.Add(constructor, constructor.GetParameters().Select(x => x.ParameterType).ToArray());
                    }
                }
            }
        }

        private GeneratedData CreateTypesToGenerate(RootSyntaxNode rootNode, ModuleBuilder module, CodeGenerationStore store)
        {
            var toGenerate = new GeneratedData();

            foreach (var node in rootNode.Delegates)
            {
                var type = module.DefineType(node.Name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.AutoLayout, typeof(MulticastDelegate));

                toGenerate.Delegates.Add(type, node);

                store.Types.Add(node.Name, type);
            }

            foreach (var node in rootNode.Classes)
            {
                var type = module.DefineType(node.Name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.AutoLayout);

                store.Types.Add(node.Name, type);

                toGenerate.Classes.Add(type, node);
            }

            return toGenerate;
        }

        private void GenerateDelegates(GeneratedData toGenerate, CodeGenerationStore store)
        {
            foreach (var delegateToGenerate in toGenerate.Delegates)
            {
                var type = delegateToGenerate.Key;
                var syntaxNode = delegateToGenerate.Value;

                store.Fields.Add(type, Array.Empty<FieldInfo>());

                var constructor = type.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard, delegateConstructorTypes);
                constructor.SetImplementationFlags(MethodImplAttributes.Runtime);

                store.Constructors.Add(type, new ConstructorInfo[] { constructor });

                store.ConstructorParameters.Add(constructor, delegateConstructorTypes);

                var parameterTypes = new Type[syntaxNode.Parameters.Count];

                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    var paramType = store.Types[syntaxNode.Parameters[i].Type];
                    if (syntaxNode.Parameters[i].IsRef)
                    {
                        throw new InvalidOperationException("Ref types are not supported");
                    }
                    parameterTypes[i] = paramType;
                }

                var method = type.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot, store.Types[syntaxNode.ReturnType], parameterTypes);
                method.SetImplementationFlags(MethodImplAttributes.Runtime);

                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    method.DefineParameter(i + 1, ParameterAttributes.None, syntaxNode.Parameters[i].Name);
                }

                store.Methods.Add(type, new MethodInfo[] { method });

                store.MethodParameters.Add(method, parameterTypes);

                store.Delegates.Add((method.ReturnType, parameterTypes, type, constructor));
            }
        }

        private IReadOnlyList<StatementSyntaxNode> GenerateClassFields(TypeBuilder type, IList<FieldSyntaxNode> fields, CodeGenerationStore store,
            ISyntaxNode syntaxNode)
        {
            var definedFields = new List<FieldInfo>();
            var initExpressions = new List<StatementSyntaxNode>();
            store.Fields.Add(type, definedFields);
            foreach (var field in fields)
            {
                var definedField = type.DefineField(field.Name, store.Types[field.Type], FieldAttributes.Public);
                definedFields.Add(definedField);
                if (field.Expression != null)
                {
                    initExpressions.Add(new ExpressionEqualsExpressionSyntaxNode(field, new VariableSyntaxNode(field, field.Name), field.Expression));
                }
            }

            initExpressions.Add(new BaseClassConstructorSyntax(syntaxNode));

            return initExpressions;
        }

        private void GenerateClassMethods(TypeBuilder type, IList<MethodSyntaxNode> methods, CodeGenerationStore store,
            Dictionary<MethodBuilder, MethodSyntaxNode> methodsDictionary, ref MethodInfo? entryPoint)
        {
            var definedMethods = new List<MethodInfo>();
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
                    var tpe = store.Types[x.Type];
                    if (x.IsRef)
                    {
                        tpe = tpe.MakeByRefType();
                    }
                    return tpe;
                }).ToArray();

                var returnType = store.Types[method.ReturnType];

                var definedMethod = type.DefineMethod(method.Name, methodAttributes, returnType, parameters);

                if (method.IsEntryPoint)
                {
                    if (entryPoint != null)
                    {
                        throw new InvalidOperationException("Can only have 1 entry point");
                    }
                    entryPoint = definedMethod;
                }

                definedMethods.Add(definedMethod);

                store.MethodParameters.Add(definedMethod, parameters);
                methodsDictionary.Add(definedMethod, method);

                int offset = 0;
                if (!method.IsStatic)
                {
                    offset = 1;
                }

                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    var p = definedMethod.DefineParameter(i + 1 + offset, ParameterAttributes.None, method.Parameters[i].Name);
                }
            }
        }

        private void GenerateClassConstructors(TypeBuilder type, IList<ConstructorSyntaxNode> constructors, CodeGenerationStore store,
            IReadOnlyList<StatementSyntaxNode> fieldInitializers,
            Dictionary<ConstructorBuilder, ConstructorSyntaxNode> constructorsDictionary, ISyntaxNode parent)
        {
            var definedConstructors = new List<ConstructorInfo>();
            store.Constructors.Add(type, definedConstructors);

            if (constructors.Count == 0)
            {
                var statementList = new List<StatementSyntaxNode>();
                constructors.Add(new ConstructorSyntaxNode(parent, Array.Empty<ParameterDefinitionSyntaxNode>(), statementList));
            }

            foreach (var constructor in constructors)
            {
                var oldStatements = new List<StatementSyntaxNode>(constructor.Statements);
                constructor.Statements.Clear();
                foreach (var toAdd in fieldInitializers)
                {
                    constructor.Statements.Add(toAdd);
                }
                foreach (var toAdd in oldStatements)
                {
                    constructor.Statements.Add(toAdd);
                }

                var methodAttributes = MethodAttributes.Public;

                var parameters = constructor.Parameters.Select(x =>
                {
                    var tpe = store.Types[x.Type];
                    if (x.IsRef)
                    {
                        tpe = tpe.MakeByRefType();
                    }
                    return tpe;
                }).ToArray();

                var definedConstructor = type.DefineConstructor(methodAttributes, CallingConventions.Standard, parameters);

                definedConstructors.Add(definedConstructor);

                store.ConstructorParameters.Add(definedConstructor, parameters);
                constructorsDictionary.Add(definedConstructor, constructor);

                int offset = 0;

                for (int i = 0; i < constructor.Parameters.Count; i++)
                {
                    var p = definedConstructor.DefineParameter(i + 1 + offset, ParameterAttributes.None, constructor.Parameters[i].Name);
                }
            }
        }

        private void GenerateClassPlaceholders(GeneratedData toGenerate, CodeGenerationStore store, ref MethodInfo? entryPoint)
        {
            foreach (var classToGenerate in toGenerate.Classes)
            {
                var type = classToGenerate.Key;
                var node = classToGenerate.Value;
                var fieldsToInitialize = GenerateClassFields(classToGenerate.Key, node.Fields, store, node);

                GenerateClassConstructors(type, node.Constructors, store, fieldsToInitialize, toGenerate.Constructors, node);

                GenerateClassMethods(type, node.Methods, store, toGenerate.Methods, ref entryPoint);
            }
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

                foreach (MethodBuilder method in store.Methods[type])
                {
                    var generator = new NetILGenerator(method.GetILGenerator());

                    var parameters = toGenerate.Methods[method].Parameters
                        .Select((p, i) => (i, store.Types[p.Type], p.Name))
                        .ToDictionary(x => x.Name, x => ((short)x.i, x.Item2));



                    var methodInfo = new CurrentMethodInfo(type, method.ReturnType, method.IsStatic,
                        parameters, fields);

                    var generation = new ILGeneration(generator, store, methodInfo);



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

                foreach (ConstructorBuilder constructor in store.Constructors[type])
                {
                    var generator = new NetILGenerator(constructor.GetILGenerator());

                    var parameters = toGenerate.Constructors[constructor].Parameters
                        .Select((p, i) => (i, store.Types[p.Type], p.Name))
                        .ToDictionary(x => x.Name, x => ((short)x.i, x.Item2));



                    var methodInfo = new CurrentMethodInfo(type, typeof(void), constructor.IsStatic,
                        parameters, fields);

                    var generation = new ILGeneration(generator, store, methodInfo);



                    GenerateMethod(generation, (IReadOnlyList<StatementSyntaxNode>)toGenerate.Constructors[constructor].Statements);
                }
            }
        }

        public MethodInfo? GenerateAssembly(RootSyntaxNode rootNode, ModuleBuilder module, Assembly[] dependentAssemblies,
            Tracer tracer)
        {
            if (rootNode == null)
            {
                throw new ArgumentNullException(nameof(rootNode));
            }

            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            if (dependentAssemblies == null)
            {
                throw new ArgumentNullException(nameof(dependentAssemblies));
            }

            if (tracer == null)
            {
                throw new ArgumentNullException(nameof(tracer));
            }

            var store = new CodeGenerationStore();

            WriteDependentTypes(store, dependentAssemblies);

            tracer.AddEpoch("Dependent Type Load");

            var toGenerate = CreateTypesToGenerate(rootNode, module, store);

            tracer.AddEpoch("Generate Types");

            GenerateDelegates(toGenerate, store);

            tracer.AddEpoch("Generate Delegates");

            MethodInfo? methodInfo = null;

            GenerateClassPlaceholders(toGenerate, store, ref methodInfo);

            tracer.AddEpoch("Generate Class Definitions");

            GenerateMethods(toGenerate, store);

            tracer.AddEpoch("Generate Class Methods");

            GenerateConstructors(toGenerate, store);

            tracer.AddEpoch("Generate Class Constructors");

            foreach (var type in store.Types.Values)
            {
                if (type is TypeBuilder tb)
                {
                    tb.CreateTypeInfo();
                }
            }

            tracer.AddEpoch("Create Type Infos");

            return methodInfo;
        }
    }
}
