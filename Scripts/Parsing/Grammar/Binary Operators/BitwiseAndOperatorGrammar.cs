﻿using System.Linq.Expressions;

namespace Hikaria.QC.Grammar
{
    public class BitwiseAndOperatorGrammar : BinaryOperatorGrammar
    {
        public override int Precedence => 6;

        protected override char OperatorToken => '&';
        protected override string OperatorMethodName => "op_bitwiseAnd";

        protected override Func<Expression, Expression, BinaryExpression> PrimitiveExpressionGenerator => Expression.And;
    }
}
