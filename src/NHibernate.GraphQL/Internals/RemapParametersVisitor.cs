using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NHibernate.GraphQL
{
    internal class RemapParametersVisitor : ExpressionVisitor
    {
        private readonly IReadOnlyDictionary<MemberInfo, Expression> _memberExpressionMapping;
        private readonly ParameterExpression _parameter;
        private readonly MemberInfo _mainMember;

        public RemapParametersVisitor(
            ParameterExpression parameter,
            MemberInfo mainMember,
            Expression mapingExpression)
        {
            _memberExpressionMapping = new FilteringMappingVisitor(mapingExpression).GetMappingDictionary();
            _parameter = parameter;
            _mainMember = mainMember;
        }

        public Expression RemapParmeters(Expression expression)
        {
            return Visit(expression);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Name == _parameter.Name)
            {
                return _memberExpressionMapping[_mainMember];
            }

            return base.VisitParameter(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == _parameter
                && _memberExpressionMapping.TryGetValue(node.Member, out var expression))
            {
                return expression;
            }

            return base.VisitMember(node);
        }
    }
}
