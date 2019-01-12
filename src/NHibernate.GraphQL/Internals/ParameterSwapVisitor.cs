using System.Linq.Expressions;

namespace NHibernate.GraphQL
{
    internal class ParameterReplacer : ExpressionVisitor  
    {
        private readonly ParameterExpression _newParam;

        public ParameterReplacer(ParameterExpression newParam)
        {
            _newParam = newParam;
        }

        public Expression RepalceParameter(Expression expression)
        {
            return Visit(expression);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        { 
            return _newParam;
        }
    }
}
