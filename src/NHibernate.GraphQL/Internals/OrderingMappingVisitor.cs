using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NHibernate.GraphQL
{
    internal class OrderingMappingVisitor : ExpressionVisitor
    {
        private readonly List<(MemberInfo, Expression)> _mapping = new List<(MemberInfo, Expression)>();

        public OrderingMappingVisitor(Expression expression)
        {
            Visit(expression);
        }

        public IReadOnlyList<(MemberInfo Member, Expression Expression)> GetMappingList()
        {
            return _mapping;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            _mapping.Add((node.Member, node.Expression));

            return base.VisitMemberAssignment(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Members != null)
            {
                for (int i = 0; i < node.Members.Count; i++)
                {
                    _mapping.Add((node.Members[i], node.Arguments[i]));
                }
            }

            return base.VisitNew(node);
        }
    }
}
