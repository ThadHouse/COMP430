using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.EmitBuilders
{
    public class EmitBuiltInTypeProvider : IBuiltInTypeProvider
    {
        private readonly Assembly[] dependentAssemblies;

        public EmitBuiltInTypeProvider(Assembly[] dependentAssemblies)
        {
            this.dependentAssemblies = dependentAssemblies;
        }

        public (IType[] delegateConstructorTypes, IType voidType, IConstructorInfo objectConstructorInfo) GenerateAssemblyTypes(CodeGenerationStore store)
        {
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }

            foreach (var assembly in dependentAssemblies)
            {
                var types = assembly.GetTypes().Where(x => x.IsPublic).Select(x => new EmitType(x));

                foreach (var type in types)
                {

                    store.Types.Add(type.FullName, type);
                }
            }

            foreach (var type in store.Types.Values)
            {

                var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

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
    }
}
