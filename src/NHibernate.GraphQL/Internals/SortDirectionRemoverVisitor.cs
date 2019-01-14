using System.Linq.Expressions;
using System.Reflection;

namespace NHibernate.GraphQL
{
    internal class SortDirectionRemoverVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo AscendingMethod = typeof(SortBy).GetMethod(nameof(SortBy.Ascending)).GetGenericMethodDefinition();
        private static readonly MethodInfo DescendingMethod = typeof(SortBy).GetMethod(nameof(SortBy.Descending)).GetGenericMethodDefinition();

        public SortDirectionRemoverVisitor(Expression expression)
        {
            Expression = Visit(expression);
        }

        public Expression Expression { get; }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method.GetGenericMethodDefinition();
            if (method == DescendingMethod)
            {
                return node.Arguments[0];
            }
            if (method == AscendingMethod)
            {
                return node.Arguments[0];
            }

            return base.VisitMethodCall(node);
        }
    }
}
