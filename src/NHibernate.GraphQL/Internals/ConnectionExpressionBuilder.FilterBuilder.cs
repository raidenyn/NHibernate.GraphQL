using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NHibernate.GraphQL
{
    internal partial class ConnectionExpressionBuilder<TResult, TDbObject, TOrder>
    {
        private static readonly MethodInfo StringCompareToMethod
            = typeof(string).GetMethod(nameof(String.CompareTo), new [] { typeof(string) });

        private static class FilterBuilder
        {
            public static Expression BuildAutoFilter(
                Expression mappingExpression,
                OrderingFields orderingFields,
                TOrder after)
            {
                var filters = new List<FilterValue>(capacity: orderingFields.Fields.Count);

                foreach (OrderingField field in orderingFields.Fields)
                {
                    var value = orderingFields.IsDirect ? after : GetValue(after, field);

                    filters.Add(new FilterValue
                    {
                        Expression = field.Expression,
                        Value = Expression.Constant(value),
                        IsLower = field.IsDescending,
                    });
                }

                Expression filter = null;

                for (var current = 0; current < filters.Count; current++)
                {
                    FilterValue currentValue = filters[current];
                    Expression currentFilter = null;

                    for (var prev = 0; prev < current; prev++)
                    {
                        FilterValue prevValue = filters[prev];

                        Expression equal = Expression.Equal(prevValue.Expression, prevValue.Value);
                        currentFilter = currentFilter != null ? Expression.AndAlso(currentFilter, equal): equal;
                    }

                    Expression compare = GreateCompareExpression(currentValue);
                    currentFilter = currentFilter != null ? Expression.AndAlso(currentFilter, compare) : compare;
                    filter = filter != null ? Expression.OrElse(filter, currentFilter) : currentFilter;
                }

                return ParameterReplacer.RepalceParameter(filter);
            }

            public static Expression RemapFilter(
                Expression mappingExpression,
                Expression<Func<TOrder, TOrder, bool>> filter,
                TOrder value)
            {
                var expression = new RemapParametersVisitor(
                    filter.Parameters[0],
                    OrderMember,
                    mappingExpression).RemapParmeters(filter.Body);
                return new ParameterVisitor(
                    filter.Parameters[1],
                    Expression.Constant(value)).ChangeParameter(expression);
            }

            private static object GetValue(TOrder after, OrderingField field)
            {
                return field.Property.GetValue(after);
            }

            private static Expression GreateCompareExpression(FilterValue currentValue)
            {
                if (currentValue.Value.Type == typeof(string))
                {
                    var comparison = Expression.Call(
                        currentValue.Expression,
                        StringCompareToMethod,
                        currentValue.Value);

                    return currentValue.IsLower
                        ? Expression.LessThan(comparison, Expression.Constant(0))
                        : Expression.GreaterThan(comparison, Expression.Constant(0));
                }

                return currentValue.IsLower
                        ? Expression.LessThan(currentValue.Expression, currentValue.Value)
                        : Expression.GreaterThan(currentValue.Expression, currentValue.Value);
            }

            private struct FilterValue
            {
                public bool IsLower { get; set; }

                public Expression Expression { get; set; }

                public Expression Value { get; set; }
            }
        }
    }
}
