using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Compiler.CodeGeneration2
{
    public class NetILGenerator : IILGenerator
    {
        private readonly ILGenerator generator;

        public NetILGenerator(ILGenerator generator)
        {
            this.generator = generator;
        }

        public LocalBuilder DeclareLocal(Type type)
        {
            return generator.DeclareLocal(type);
        }

        public Label DefineLabel()
        {
            return generator.DefineLabel();
        }

        public void EmitAdd()
        {
            generator.Emit(OpCodes.Add);
        }

        public void EmitBox(Type type)
        {

            generator.Emit(OpCodes.Box, type);
        }

        public void EmitBr(Label label)
        {
            generator.Emit(OpCodes.Br, label);
        }

        public void EmitBrfalse(Label label)
        {
            generator.Emit(OpCodes.Brfalse, label);
        }

        public void EmitBrtrue(Label label)
        {
            generator.Emit(OpCodes.Brtrue, label);
        }

        public void EmitCall(MethodInfo methodInfo)
        {
            generator.EmitCall(OpCodes.Call, methodInfo, null);
        }

        public void EmitCallvirt(MethodInfo methodInfo)
        {
            generator.EmitCall(OpCodes.Callvirt, methodInfo, null);
        }

        public void EmitCeq()
        {
            generator.Emit(OpCodes.Ceq);
        }

        public void EmitCgt()
        {
            generator.Emit(OpCodes.Cgt);
        }

        public void EmitClt()
        {
            generator.Emit(OpCodes.Clt);
        }

        public void EmitConstructorCall(ConstructorInfo constructorInfo)
        {
            generator.Emit(OpCodes.Call, constructorInfo);
        }

        public void EmitDiv()
        {
            generator.Emit(OpCodes.Div);
        }

        public void EmitDup()
        {
            generator.Emit(OpCodes.Dup);
        }

        public void EmitFalse()
        {
            generator.Emit(OpCodes.Ldc_I4_0);
        }

        public void EmitLdarg(short idx)
        {
            generator.Emit(OpCodes.Ldarg, idx);
        }

        public void EmitLdarga(short idx)
        {
            generator.Emit(OpCodes.Ldarga, idx);
        }

        public void EmitLdcI4(int value)
        {
            generator.Emit(OpCodes.Ldc_I4, value);
        }

        public void EmitLdcI40()
        {
            generator.Emit(OpCodes.Ldc_I4_0);
        }

        public void EmitLdelem(Type type)
        {
            generator.Emit(OpCodes.Ldelem, type);
        }

        public void EmitLdfld(FieldInfo field)
        {
            generator.Emit(OpCodes.Ldfld, field);
        }

        public void EmitLdflda(FieldInfo field)
        {
            generator.Emit(OpCodes.Ldflda, field);
        }

        public void EmitLdftn(MethodInfo method)
        {
            generator.Emit(OpCodes.Ldftn, method);
        }

        public void EmitLdloc(LocalBuilder local)
        {
            generator.Emit(OpCodes.Ldloc, local);
        }

        public void EmitLdloca(LocalBuilder local)
        {
            generator.Emit(OpCodes.Ldloca, local);
        }

        public void EmitLdnull()
        {
            generator.Emit(OpCodes.Ldnull);
        }

        public void EmitLdstr(string value)
        {
            generator.Emit(OpCodes.Ldstr, value);
        }

        public void EmitLdthis()
        {
            generator.Emit(OpCodes.Ldarg_0);
        }

        public void EmitLdvirtftn(MethodInfo method)
        {
            generator.Emit(OpCodes.Ldvirtftn, method);
        }

        public void EmitMul()
        {
            generator.Emit(OpCodes.Mul);
        }

        public void EmitNewarr(Type type)
        {
            generator.Emit(OpCodes.Newarr, type);
        }

        public void EmitNewobj(ConstructorInfo constructor)
        {
            generator.Emit(OpCodes.Newobj, constructor);
        }

        public void EmitRet()
        {
            generator.Emit(OpCodes.Ret);
        }

        public void EmitStarg(short idx)
        {
            if (idx < 256)
            {
                generator.Emit(OpCodes.Starg_S, (byte)idx);
            }
            else
            {
                generator.Emit(OpCodes.Starg, idx);
            }
        }

        public void EmitStelem(Type type)
        {
            generator.Emit(OpCodes.Stelem, type);
        }

        public void EmitStfld(FieldInfo field)
        {
            generator.Emit(OpCodes.Stfld, field);
        }

        public void EmitStloc(LocalBuilder local)
        {
            if (local == null)
            {
                throw new ArgumentNullException(nameof(local));
            }

            switch (local.LocalIndex)
            {
                case 0:
                    generator.Emit(OpCodes.Stloc_0);
                    break;
                case 1:
                    generator.Emit(OpCodes.Stloc_1);
                    break;
                case 2:
                    generator.Emit(OpCodes.Stloc_2);
                    break;
                case 3:
                    generator.Emit(OpCodes.Stloc_3);
                    break;
                default:
                    if (local.LocalIndex < 256)
                    {
                        generator.Emit(OpCodes.Stloc_S, (byte)local.LocalIndex);
                    }
                    else
                    {
                        generator.Emit(OpCodes.Stloc, local);
                    }
                    break;
            }
        }

        public void EmitSub()
        {
            generator.Emit(OpCodes.Sub);
        }

        public void EmitTrue()
        {
            generator.Emit(OpCodes.Ldc_I4_1);
        }

        public void MarkLabel(Label label)
        {
            generator.MarkLabel(label);
        }
    }
}
