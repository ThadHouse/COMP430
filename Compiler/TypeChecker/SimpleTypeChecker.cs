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
                throw new InvalidOperationException($"Types must be equal: {leftType.FullName} - {rightType.FullName}");
            }

            if (leftType.FullName != typeof(int).FullName)
            {
                throw new InvalidOperationException($"Cannot perform arithmatic on {leftType.FullName}");
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
                if (VoidType == null)
                {
                    throw new InvalidOperationException("Void type must be set");
                }
                rightType = VoidType;
            }

            if (rightType.FullName == "System.Void" && !leftType.IsValueType)
            {
                return;
            }

            if (!leftType.IsAssignableFrom(rightType))
            {
                throw new InvalidOperationException($"Invalid Type Assignment, attempting to assign {rightType.FullName} to {leftType.FullName}");
            }
        }


    }
}
