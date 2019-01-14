using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NHibernate.GraphQL
{
    internal class FilteringMappingVisitor : ExpressionVisitor
    {
        private readonly Dictionary<MemberInfo, Expression> _mapping = new Dictionary<MemberInfo, Expression>();

        public FilteringMappingVisitor(Expression expression)
        {
            Visit(expression);
        }

        public IReadOnlyDictionary<MemberInfo, Expression> GetMappingDictionary()
        {
            return _mapping;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            _mapping.Add(node.Member, node.Expression);

            return base.VisitMemberAssignment(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Members != null)
            {
                for (int i = 0; i < node.Members.Count; i++)
                {
                    _mapping.Add(node.Members[i], node.Arguments[i]);
                }
            }

            return base.VisitNew(node);
        }
    }
}
