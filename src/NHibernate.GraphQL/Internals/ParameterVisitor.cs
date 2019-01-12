using System.Linq.Expressions;

namespace NHibernate.GraphQL
{
    internal class ParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParam;
        private readonly Expression _newParam;

        public ParameterVisitor(ParameterExpression oldParam, Expression newParam)
        {
            _oldParam = oldParam;
            _newParam = newParam;
        }

        public Expression ChangeParameter(Expression expression)
        {
            return Visit(expression);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Name == _oldParam.Name)
            {
                return _newParam;
            }

            return base.VisitParameter(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var result = base.VisitMember(node);
            if (result is MemberExpression member) {
                if (member.Expression == _newParam)
                {
                    return Expression.Constant(Expression.Lambda(member).Compile().DynamicInvoke());
                }
            }
            return result;
        }
    }
}
