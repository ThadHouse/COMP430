using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.CodeGeneration2.Builders;
using Compiler.Parser.Nodes;

namespace Compiler.TypeChecker
{
    public class SimpleTypeChecker
    {
        public SimpleTypeChecker(IType voidType)
        {
            VoidType = voidType;
        }

        public IType VoidType { get; }

        public void CheckCanArithmaticTypeOperations(IType? leftType, IType? rightType)
        {
            if (leftType == null)
            {
                throw new ArgumentNullException(nameof(leftType), "Left type really cannot be null here");
            }

            if (rightType == null)
            {
                throw new ArgumentNullException(nameof(rightType), "Right type really cannot be null here");
            }

            if (rightType.FullName != leftType.FullName)
            {
                throw new TypeCheckException($"Types must be equal: {leftType.FullName} - {rightType.FullName}");
            }

            if (leftType.FullName != typeof(int).FullName)
            {
                throw new TypeCheckException($"Cannot perform arithmatic on {leftType.FullName}");
            }
        }

        public void AssertTypeIsNotVoid(IType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "Type cannot be null");
            }

            if (type.FullName == "System.Void")
            {
                throw new TypeCheckException("Type cannot be void here");
            }
        }

        public void TypeCheck(IType? leftType, IType? rightType)
        {
            if (leftType == null)
            {
                throw new ArgumentNullException(nameof(leftType), "Left type really cannot be null here");
            }

            // A null right type is void
            if (rightType == null)
            {
                rightType = VoidType;
            }

            if (rightType.FullName == typeof(void).FullName && !leftType.IsValueType)
            {
                return;
            }

            if (!leftType.IsAssignableFrom(rightType))
            {
                throw new TypeCheckException($"Invalid Type Assignment, attempting to assign {rightType.FullName} to {leftType.FullName}");
            }
        }


    }
}
