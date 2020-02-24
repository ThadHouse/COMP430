using System;
using System.Collections.Generic;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2
{
    public class CodeGenerationStore
    {
        public List<(IType returnType, IType[] parameters, IType type, IConstructorBuilder constructor)> Delegates { get; } = new List<(IType returnType, IType[] parameters, IType type, IConstructorBuilder constructor)>();

        public Dictionary<string, IType> Types { get; } = new Dictionary<string, IType>();

        public IType TypeDefLookup(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (Types.TryGetValue(value, out var type))
            {
                return type;
            }
            if (value.EndsWith("[]", StringComparison.InvariantCultureIgnoreCase))
            {
                var subType = value.Substring(0, value.Length - 2);
                type = Types[subType];
                type = type.MakeArrayType();
                Types.Add(value, type);
                return type;
            }
            throw new KeyNotFoundException(nameof(value));
        }

        public Dictionary<IType, IReadOnlyList<IFieldInfo>> Fields { get; } = new Dictionary<IType, IReadOnlyList<IFieldInfo>>();
        public Dictionary<IType, IReadOnlyList<IMethodInfo>> Methods { get; } = new Dictionary<IType, IReadOnlyList<IMethodInfo>>();
        public Dictionary<IMethodInfo, IReadOnlyList<IType>> MethodParameters { get; } = new Dictionary<IMethodInfo, IReadOnlyList<IType>>();

        public Dictionary<IType, IReadOnlyList<IConstructorInfo>> Constructors { get; } = new Dictionary<IType, IReadOnlyList<IConstructorInfo>>();
        public Dictionary<IConstructorInfo, IReadOnlyList<IType>> ConstructorParameters { get; } = new Dictionary<IConstructorInfo, IReadOnlyList<IType>>();
    }
}
