using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

    internal class ParameterChanger : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParam;
        private readonly Expression _newParam;

        public ParameterChanger(ParameterExpression oldParam, Expression newParam)
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

    internal class RemapParameters : ExpressionVisitor
    {
        private readonly Dictionary<MemberInfo, Expression> _memberExpressionMapping;
        private readonly ParameterExpression _parameter;
        private readonly MemberInfo _mainMember;

        public RemapParameters(
            ParameterExpression parameter,
            MemberInfo mainMember,
            Expression mapingExpression)
        {
            _memberExpressionMapping = new MappingExtractor(mapingExpression).GetMapping();
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

    internal class MappingExtractor : ExpressionVisitor
    {
        private readonly Dictionary<MemberInfo, Expression> _mapping = new Dictionary<MemberInfo, Expression>();

        public MappingExtractor(Expression expression)
        {
            Visit(expression);
        }

        public Dictionary<MemberInfo, Expression> GetMapping()
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
