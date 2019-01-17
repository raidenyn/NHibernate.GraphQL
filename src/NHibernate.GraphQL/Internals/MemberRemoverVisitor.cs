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

        private readonly List<MemberInfo> _usedMembers;

        public MemberRemoverVisitor(IEnumerable<MemberInfo> keepMembers, Expression expression)
        {
            if (keepMembers == null) throw new ArgumentNullException(nameof(keepMembers));
            _keepMembers = new HashSet<MemberInfo>(keepMembers);
            _usedMembers = new List<MemberInfo>(capacity: _keepMembers.Count);

            ClearedExpression = Visit(expression);

            _keepMembers.ExceptWith(_usedMembers);
            _usedMembers.Clear();
        }

        public Expression ClearedExpression { get; }

        public ICollection<MemberInfo> UnusedMembers
        {
            get
            {
                return _keepMembers;
            }
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            _usedMembers.Add(node.Member);

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