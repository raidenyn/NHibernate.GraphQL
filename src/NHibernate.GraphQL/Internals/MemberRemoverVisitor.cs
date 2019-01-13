using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Mapping.ByCode;

namespace NHibernate.GraphQL
{
    internal class MemberRemoverVisitor : ExpressionVisitor  
    {
        private readonly HashSet<MemberInfo> _keepMembers;

        public MemberRemoverVisitor(IEnumerable<MemberInfo> keepMembers)
        {
            if (keepMembers == null) throw new ArgumentNullException(nameof(keepMembers));
            _keepMembers = new HashSet<MemberInfo>(keepMembers);
        }

        public Expression RemoveFields(Expression expression)  
        {  
            return Visit(expression);
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            if (!_keepMembers.Contains(node.Member))
            {
                var type = node.Member.GetPropertyOrFieldType();

                return node.Update(Expression.Constant(GetDefaultValue(type), type));
            }

            return base.VisitMemberAssignment(node);
        }

        private static object GetDefaultValue(System.Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }
    }
}