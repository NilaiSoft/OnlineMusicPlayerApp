using System;
using System.Collections.Generic;
using System.Linq.Expressions;

public static class ExpressionCombiner
{
    //public static Expression<Func<T, bool>> CombineWithAnd<T>(params Expression<Func<T, bool>>[] expressions)
    public static Expression<Func<T, bool>> CombineWithAnd<T>(this List<Expression<Func<T, bool>>> expressions)
    {
        if (expressions == null || expressions.Count == 0)
            return x => true;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression body = null;

        foreach (var expr in expressions)
        {
            var replacedBody = new ParameterReplacer(expr.Parameters[0], parameter).Visit(expr.Body);

            body = body == null ? replacedBody : Expression.AndAlso(body, replacedBody);
        }

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ParameterReplacer(ParameterExpression from, ParameterExpression to)
        {
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _from ? _to : base.VisitParameter(node);
        }
    }
}
