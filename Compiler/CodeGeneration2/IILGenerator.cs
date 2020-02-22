using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Compiler.CodeGeneration2
{
    public interface IILGenerator
    {
        void EmitConstructorCall(ConstructorInfo constructorInfo);

        void EmitCall(MethodInfo methodInfo);

        void EmitCallvirt(MethodInfo methodInfo);

        void EmitLdnull();

        void EmitDup();

        void EmitLdftn(MethodInfo method);

        void EmitLdvirtftn(MethodInfo method);

        void EmitNewobj(ConstructorInfo constructor);

        void EmitLdfld(FieldInfo field);

        void EmitStfld(FieldInfo field);

        void EmitLdflda(FieldInfo field);

        void EmitLdarg(short idx);

        void EmitLdarga(short idx);

        void EmitStarg(short idx);

        void EmitStloc(LocalBuilder local);

        void EmitLdloc(LocalBuilder local);

        void EmitLdloca(LocalBuilder local);

        void EmitLdthis();

        void EmitAdd();

        void EmitSub();

        void EmitMul();

        void EmitDiv();

        void EmitClt();

        void EmitCgt();

        void EmitCeq();

        void EmitLdcI40();

        void EmitLdcI4(int value);

        void EmitLdstr(string value);

        void EmitTrue();

        void EmitFalse();

        Label DefineLabel();

        void MarkLabel(Label label);

        void EmitBr(Label label);

        void EmitBrtrue(Label label);

        void EmitBrfalse(Label label);

        void EmitRet();

        LocalBuilder DeclareLocal(Type type);

        void EmitBox(Type type);

        void EmitNewarr(Type type);

        void EmitLdelem(Type type);

        void EmitStelem(Type type);
    }
}
