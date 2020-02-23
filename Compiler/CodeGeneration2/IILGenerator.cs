using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.CodeGeneration2.Builders;

namespace Compiler.CodeGeneration2
{
    public interface IILGenerator
    {
        void EmitConstructorCall(IConstructorInfo constructorInfo);

        void EmitCall(IMethodInfo methodInfo);

        void EmitCallvirt(IMethodInfo methodInfo);

        void EmitLdnull();

        void EmitDup();

        void EmitLdftn(IMethodInfo method);

        void EmitLdvirtftn(IMethodInfo method);

        void EmitNewobj(IConstructorInfo constructor);

        void EmitLdfld(IFieldInfo field);

        void EmitStfld(IFieldInfo field);

        void EmitLdflda(IFieldInfo field);

        void EmitLdarg(short idx);

        void EmitLdarga(short idx);

        void EmitStarg(short idx);

        void EmitStloc(ILocalBuilder local);

        void EmitLdloc(ILocalBuilder local);

        void EmitLdloca(ILocalBuilder local);

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

        ILocalBuilder DeclareLocal(IType type);

        void EmitBox(IType type);

        void EmitNewarr(IType type);

        void EmitLdelem(IType type);

        void EmitStelem(IType type);
    }
}
