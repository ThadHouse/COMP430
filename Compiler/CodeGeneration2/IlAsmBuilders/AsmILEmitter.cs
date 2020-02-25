using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2.IlAsmBuilders
{
    public class AsmILEmitter
    {
        private readonly List<string> lines = new List<string>();

        public AsmILEmitter(string name)
        {
            lines.Add(".assembly extern mscorlib { auto }");
            lines.Add($".assembly {name} {{}}");
            lines.Add($".module {name}.exe");
            lines.Add("");
        }

        private string GetMethodAttributes(MethodAttributes attributes)
        {
            StringBuilder builder = new StringBuilder();
            if (attributes.HasFlag(MethodAttributes.Public))
            {
                builder.Append("public ");
            }
            else
            {
                builder.Append("private ");
            }
            if (attributes.HasFlag(MethodAttributes.SpecialName))
            {
                builder.Append("specialname ");
            }
            if (attributes.HasFlag(MethodAttributes.HideBySig))
            {
                builder.Append("hidebysig ");
            }
            if (attributes.HasFlag(MethodAttributes.RTSpecialName))
            {
                builder.Append("rtspecialname ");
            }
            if (attributes.HasFlag(MethodAttributes.Static))
            {
                builder.Append("static ");
            }
            else
            {
                builder.Append("instance ");
            }
            return builder.ToString();
        }

        private string GetMethodImplAttributes(MethodImplAttributes attribtues)
        {
            StringBuilder builder = new StringBuilder();
            if (attribtues.HasFlag(MethodImplAttributes.Runtime))
            {
                builder.Append("runtime ");
            }
            else
            {
                builder.Append("cil ");
            }
            builder.Append("managed ");
            return builder.ToString();
        }

        private string GetMethodParameters(IReadOnlyList<(IType type, string name)> parameters)
        {
            if (parameters.Count == 0)
            {
                return "";
            }
            return parameters.Select(x => $"{x.type.ToFullTypeString()} {x.name}").Aggregate((x, y) => $"{x}, {y}");
        }

        private void WriteConstructors(IReadOnlyList<AsmConstructorBuilder> methods)
        {
            foreach (var method in methods)
            {
                lines.Add($"\t.method {GetMethodAttributes(method.MethodAttributes)} void .ctor({GetMethodParameters(method.MethodParameters)}) {GetMethodImplAttributes(method.MethodImplAttributes)}");
                lines.Add("\t{");
                var generator = method.GetILGenerator();
                WriteLocals(generator.Locals);
                foreach (var opCode in generator.OpCodes.Select(x => $"\t\t{x}"))
                {
                    lines.Add(opCode);
                }
                lines.Add("\t}");
                lines.Add("");
            }
        }

        private void WriteFields(IReadOnlyList<AsmFieldBuilder> fields)
        {
            foreach (var field in fields)
            {
                lines.Add($"\t.field public {field.FieldType.ToFullTypeString()} {field.Name}");
            }
        }

        private void WriteMethods(IReadOnlyList<AsmMethodBuilder> methods)
        {
            foreach (var method in methods)
            {
                lines.Add($"\t.method {GetMethodAttributes(method.MethodAttributes)} {method.ReturnType.ToFullTypeString()} {method.Name}({GetMethodParameters(method.MethodParameters)}) {GetMethodImplAttributes(method.MethodImplAttributes)}");
                lines.Add("\t{");
                var generator = method.GetILGenerator();
                WriteLocals(generator.Locals);
                foreach (var opCode in generator.OpCodes.Select(x => $"\t\t{x}"))
                {
                    lines.Add(opCode);
                }

                lines.Add("\t}");
                lines.Add("");
            }
        }

        private void WriteLocals(IReadOnlyList<ILocalBuilder> locals)
        {
            if (locals.Count == 0)
            {
                return;
            }

            lines.Add("\t\t.locals init(");
            foreach (var local in locals)
            {
                lines.Add($"\t\t\t{local.LocalType.ToFullTypeString()} {local.Name}");
            }
            lines.Add("\t\t)");
        }

        public void WriteType(AsmTypeBuilder typeBuilder)
        {
            if (typeBuilder == null)
            {
                throw new ArgumentNullException(nameof(typeBuilder));
            }

            string baseType = "";

            if (typeBuilder.BaseType == "System.MulticastDelegate")
            {
                baseType = "extends [mscorlib]System.MulticastDelegate";
            }
            else if (typeBuilder.BaseType.Length == 0)
            {
                throw new NotSupportedException("No base types are supported");
            }

            lines.Add($".class public beforefieldinit sealed {typeBuilder.FullName} {baseType}");
            lines.Add("{");

            WriteFields(typeBuilder.TypeFields);

            WriteConstructors(typeBuilder.TypeConstructors);

            WriteMethods(typeBuilder.TypeMethods);

            lines.Add("}");
        }

        public string[] Finalize()
        {
            return lines.ToArray();
        }
    }
}
