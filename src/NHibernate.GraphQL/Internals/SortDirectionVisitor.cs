
using System.Linq.Expressions;
using System.Reflection;

namespace NHibernate.GraphQL
{
    internal class SortDirectionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo AscendingMethod = typeof(SortBy).GetMethod(nameof(SortBy.Ascending)).GetGenericMethodDefinition();
        private static readonly MethodInfo DescendingMethod = typeof(SortBy).GetMethod(nameof(SortBy.Descending)).GetGenericMethodDefinition();

        public SortDirectionVisitor(Expression expression)
        {
            Expression = Visit(expression);
        }

        public bool IsDescending { get; private set; }

        public Expression Expression { get; }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method.GetGenericMethodDefinition();
            if (method == DescendingMethod)
            {
                IsDescending = true;
                return node.Arguments[0];
            }
            if (method == AscendingMethod)
            {
                IsDescending = false;
                return node.Arguments[0];
            }

            return base.VisitMethodCall(node);
        }
    }
}
