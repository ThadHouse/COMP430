using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.CodeGeneration2.Builders;
using Compiler.CodeGeneration2.EmitBuilders;

namespace Compiler.CodeGeneration2.EmitBuilders
{
    public class EmitILGenerator : IILGenerator
    {
        private readonly ILGenerator generator;

        public EmitILGenerator(ILGenerator generator)
        {
            this.generator = generator;
        }

        public ILocalBuilder DeclareLocal(IType type, string name)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return new EmitLocalBuilder(generator.DeclareLocal(((EmitType)type).Type), type, name);
        }

        public ILabel DefineLabel()
        {
            return new EmitLabel(generator.DefineLabel());
        }

        public void EmitAdd()
        {
            generator.Emit(OpCodes.Add);
        }

        public void EmitBox(IType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            generator.Emit(OpCodes.Box, ((EmitType)type).Type);
        }

        public void EmitBr(ILabel label)
        {
            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }
            generator.Emit(OpCodes.Br, ((EmitLabel)label).Label);
        }

        public void EmitBrfalse(ILabel label)
        {
            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }
            generator.Emit(OpCodes.Brfalse, ((EmitLabel)label).Label);
        }

        public void EmitBrtrue(ILabel label)
        {
            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }
            generator.Emit(OpCodes.Brtrue, ((EmitLabel)label).Label);
        }

        public void EmitCall(IMethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            generator.EmitCall(OpCodes.Call, ((EmitMethodInfo)method).MethodInfo, null);
        }

        public void EmitCallvirt(IMethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            generator.EmitCall(OpCodes.Callvirt, ((EmitMethodInfo)method).MethodInfo, null);
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

        public void EmitConstructorCall(IConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
            {
                throw new ArgumentNullException(nameof(constructorInfo));
            }
            generator.Emit(OpCodes.Call, ((EmitConstructorInfo)constructorInfo).ConstructorInfo);
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

        public void EmitLdelem(IType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            generator.Emit(OpCodes.Ldelem, ((EmitType)type).Type);
        }

        public void EmitLdfld(IFieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }
            generator.Emit(OpCodes.Ldfld, ((EmitFieldInfo)field).FieldInfo);
        }

        public void EmitLdflda(IFieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }
            generator.Emit(OpCodes.Ldflda, ((EmitFieldInfo)field).FieldInfo);
        }

        public void EmitLdftn(IMethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            generator.Emit(OpCodes.Ldftn, ((EmitMethodInfo)method).MethodInfo);
        }

        public void EmitLdloc(ILocalBuilder local)
        {
            if (local == null)
            {
                throw new ArgumentNullException(nameof(local));
            }
            generator.Emit(OpCodes.Ldloc, ((EmitLocalBuilder)local).LocalBuilder);
        }

        public void EmitLdloca(ILocalBuilder local)
        {
            if (local == null)
            {
                throw new ArgumentNullException(nameof(local));
            }
            generator.Emit(OpCodes.Ldloca, ((EmitLocalBuilder)local).LocalBuilder);
        }

        public void EmitLdnull()
        {
            generator.Emit(OpCodes.Ldnull);
        }

        public void EmitLdsfld(IFieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }
            generator.Emit(OpCodes.Ldsfld, ((EmitFieldInfo)field).FieldInfo);
        }

        public void EmitLdsflda(IFieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }
            generator.Emit(OpCodes.Ldsflda, ((EmitFieldInfo)field).FieldInfo);
        }

        public void EmitLdstr(string value)
        {
            generator.Emit(OpCodes.Ldstr, value);
        }

        public void EmitLdthis()
        {
            generator.Emit(OpCodes.Ldarg_0);
        }

        public void EmitLdvirtftn(IMethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            generator.Emit(OpCodes.Ldvirtftn, ((EmitMethodInfo)method).MethodInfo);
        }

        public void EmitMul()
        {
            generator.Emit(OpCodes.Mul);
        }

        public void EmitNewarr(IType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            generator.Emit(OpCodes.Newarr, ((EmitType)type).Type);
        }

        public void EmitNewobj(IConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
            {
                throw new ArgumentNullException(nameof(constructorInfo));
            }
            generator.Emit(OpCodes.Newobj, ((EmitConstructorInfo)constructorInfo).ConstructorInfo);
        }

        public void EmitPop()
        {
            generator.Emit(OpCodes.Pop);
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

        public void EmitStelem(IType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            generator.Emit(OpCodes.Stelem, ((EmitType)type).Type);
        }

        public void EmitStfld(IFieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }
            generator.Emit(OpCodes.Stfld, ((EmitFieldInfo)field).FieldInfo);
        }

        public void EmitStloc(ILocalBuilder local)
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
                        generator.Emit(OpCodes.Stloc, ((EmitLocalBuilder)local).LocalBuilder);
                    }
                    break;
            }
        }

        public void EmitStsfld(IFieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }
            generator.Emit(OpCodes.Stsfld, ((EmitFieldInfo)field).FieldInfo);
        }

        public void EmitSub()
        {
            generator.Emit(OpCodes.Sub);
        }

        public void EmitTrue()
        {
            generator.Emit(OpCodes.Ldc_I4_1);
        }

        public void MarkLabel(ILabel label)
        {
            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }
            generator.MarkLabel(((EmitLabel)label).Label);
        }
    }
}
