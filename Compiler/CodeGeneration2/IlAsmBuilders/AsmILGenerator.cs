using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.IlAsmBuilders
{
    public static class TypeWritingExtensions
    {
        public static string ToFullTypeString(this IType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return type.FullName switch
            {
                "System.Void" => "void",
                "System.Int32" => "int32",
                "System.String" => "string",
                "System.IntPtr" => "native int",
                "System.Boolean" => "bool",
                _ => $"{(type.IsValueType ? "valuetype" : "class")} [{type.ModuleName}]{type.FullName}",
            };
        }

        public static void AddFullTypeInstruction(this IList<string> list, IType type, string instruction)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            list.Add($"{instruction} {type.ToFullTypeString()}");
        }

        public static string ToMethodSignature(this IMethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
            var methodParameters = methodInfo.GetParameters();
            var parameters = methodParameters.Length > 0 ? methodParameters.Select(x => x.ToFullTypeString()).Aggregate((x, y) => $"{x}, {y}") : "";
            return $"{methodInfo.ReturnType.ToFullTypeString()} {methodInfo.DeclaringType.ToFullTypeString()}::{methodInfo.Name}({parameters})";
        }

        public static void AddCallInstruction(this IList<string> list, IMethodInfo methodInfo, string instruction)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
            list.Add($"{instruction} {methodInfo.ToMethodSignature()}");
        }

        public static string ToMethodSignature(this IConstructorInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
            var methodParameters = methodInfo.GetParameters();
            var parameters = methodParameters.Length > 0 ? methodParameters.Select(x => x.ToFullTypeString()).Aggregate((x, y) => $"{x}, {y}") : "";
            return $"instance void {methodInfo.DeclaringType.ToFullTypeString()}::.ctor({parameters})";
        }

        public static void AddCallInstruction(this IList<string> list, IConstructorInfo methodInfo, string instruction)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
            list.Add($"{instruction} {methodInfo.ToMethodSignature()}");
        }

        public static string ToFieldSignature(this IFieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                throw new ArgumentNullException(nameof(fieldInfo));
            }
            return $"{fieldInfo.FieldType.ToFullTypeString()} {fieldInfo.DeclaringType.ToFullTypeString()}::{fieldInfo.Name}";
        }

        public static void AddFieldInstruction(this IList<string> list, IFieldInfo fieldInfo, string instruction)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            if (fieldInfo == null)
            {
                throw new ArgumentNullException(nameof(fieldInfo));
            }
            list.Add($"{instruction} {fieldInfo.ToFieldSignature()}");
        }
    }

    public class AsmILGenerator : IILGenerator
    {
        public List<string> OpCodes { get; } = new List<string>();
        public List<AsmLocalBuilder> Locals { get; } = new List<AsmLocalBuilder>();

        public AsmILGenerator(bool isEntryPoint = false)
        {
            if (isEntryPoint)
            {
                OpCodes.Add(".entrypoint");
            }
        }

        public ILocalBuilder DeclareLocal(IType type, string name)
        {
            var local = new AsmLocalBuilder(type, Locals.Count, name);
            Locals.Add(local);
            return local;
        }

        public Label DefineLabel()
        {
            throw new NotImplementedException();
        }

        public void EmitAdd()
        {
            OpCodes.Add("add");
        }

        public void EmitBox(IType type)
        {
            OpCodes.AddFullTypeInstruction(type, "box");
        }

        public void EmitBr(Label label)
        {
            throw new NotImplementedException();
        }

        public void EmitBrfalse(Label label)
        {
            throw new NotImplementedException();
        }

        public void EmitBrtrue(Label label)
        {
            throw new NotImplementedException();
        }

        public void EmitCall(IMethodInfo methodInfo)
        {
            OpCodes.AddCallInstruction(methodInfo, "call");
        }

        public void EmitCallvirt(IMethodInfo methodInfo)
        {
            OpCodes.AddCallInstruction(methodInfo, "callvirt");
        }

        public void EmitCeq()
        {
            throw new NotImplementedException();
        }

        public void EmitCgt()
        {
            throw new NotImplementedException();
        }

        public void EmitClt()
        {
            throw new NotImplementedException();
        }

        public void EmitConstructorCall(IConstructorInfo constructorInfo)
        {
            OpCodes.AddCallInstruction(constructorInfo, "call");
        }

        public void EmitDiv()
        {
            throw new NotImplementedException();
        }

        public void EmitDup()
        {
            throw new NotImplementedException();
        }

        public void EmitFalse()
        {
            throw new NotImplementedException();
        }

        public void EmitLdarg(short idx)
        {
            throw new NotImplementedException();
        }

        public void EmitLdarga(short idx)
        {
            throw new NotImplementedException();
        }

        public void EmitLdcI4(int value)
        {
            OpCodes.Add($"ldc.i4 {value}");
        }

        public void EmitLdcI40()
        {
            OpCodes.Add("ldc.i4.0");
        }

        public void EmitLdelem(IType type)
        {
            throw new NotImplementedException();
        }

        public void EmitLdfld(IFieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }
            OpCodes.AddFieldInstruction(field, "ldfld");
        }

        public void EmitLdflda(IFieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }
            OpCodes.AddFieldInstruction(field, "ldflda");
        }

        public void EmitLdftn(IMethodInfo method)
        {
            throw new NotImplementedException();
        }

        public void EmitLdloc(ILocalBuilder local)
        {
            if (local == null)
            {
                throw new ArgumentNullException(nameof(local));
            }
            OpCodes.Add($"ldloc {local.LocalIndex}");
        }

        public void EmitLdloca(ILocalBuilder local)
        {
            if (local == null)
            {
                throw new ArgumentNullException(nameof(local));
            }
            OpCodes.Add($"ldloca {local.LocalIndex}");
        }

        public void EmitLdnull()
        {
            throw new NotImplementedException();
        }

        public void EmitLdstr(string value)
        {
            throw new NotImplementedException();
        }

        public void EmitLdthis()
        {
            OpCodes.Add("ldarg.0");
        }

        public void EmitLdvirtftn(IMethodInfo method)
        {
            throw new NotImplementedException();
        }

        public void EmitMul()
        {
            throw new NotImplementedException();
        }

        public void EmitNewarr(IType type)
        {
            throw new NotImplementedException();
        }

        public void EmitNewobj(IConstructorInfo constructor)
        {
            OpCodes.AddCallInstruction(constructor, "newobj");
        }

        public void EmitRet()
        {
            OpCodes.Add("ret");
        }

        public void EmitStarg(short idx)
        {
            throw new NotImplementedException();
        }

        public void EmitStelem(IType type)
        {
            throw new NotImplementedException();
        }

        public void EmitStfld(IFieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }
            OpCodes.AddFieldInstruction(field, "stfld");
        }

        public void EmitStloc(ILocalBuilder local)
        {
            if (local == null)
            {
                throw new ArgumentNullException(nameof(local));
            }
            OpCodes.Add($"stloc {local.LocalIndex}");
        }

        public void EmitSub()
        {
            throw new NotImplementedException();
        }

        public void EmitTrue()
        {
            throw new NotImplementedException();
        }

        public void MarkLabel(Label label)
        {
            throw new NotImplementedException();
        }
    }
}
