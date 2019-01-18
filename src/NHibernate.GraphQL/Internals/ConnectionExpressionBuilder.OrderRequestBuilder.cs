using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NHibernate.GraphQL
{
    internal partial class ConnectionExpressionBuilder<TResult, TDbObject, TOrder>
    {
        private static class OrderRequestBuilder
        {
            public static Expression BuildOrderExpression(
                Expression queryExpression,
                OrderingFields orderingFields)
            {
                queryExpression = GetOrderBy(queryExpression, orderingFields.Fields[0]);

                foreach (OrderingField field in orderingFields.Fields.Skip(1))
                {
                    queryExpression = GetThenBy(queryExpression, field);
                }

                return queryExpression;
            }

            public static OrderingFields GetOrdering(Expression<Func<TDbObject, TOrder>> orderBy)
            {
                var result = new OrderingFields
                {
                    OrderParameter = orderBy.Parameters[0]
                };
                result.Fields =
                    (from item in new OrderingMappingVisitor(orderBy).GetMappingList()
                     where item.Member.DeclaringType == typeof(TOrder)
                     let property = (PropertyInfo)item.Member
                     let sortDirection = new SortDirectionVisitor(item.Expression)
                     select new OrderingField
                     {
                         Type = property.PropertyType,
                         Expression = sortDirection.Expression,
                         IsDescending = sortDirection.IsDescending,
                         Property = property,
                     }).ToArray();

                // if order is simple direct field access like (item => item.id)
                if (result.Fields.Count == 0)
                {
                    var sortDirection = new SortDirectionVisitor(orderBy.Body);
                    result.Fields = new[]
                    {
                        new OrderingField
                        {
                            Type = typeof(TOrder),
                            Expression = sortDirection.Expression,
                            IsDescending = sortDirection.IsDescending,
                        }
                    };
                    result.IsDirect = true;
                }

                return result;
            }

            private static Expression GetOrderBy(
                Expression queryExpression,
                OrderingField field)
            {
                string methodName = field.IsDescending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);

                return Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new System.Type[] { typeof(TDbObject), field.Type },
                    queryExpression,
                    Expression.Lambda(ParameterReplacer.RepalceParameter(field.Expression), DbItem));
            }

            private static Expression GetThenBy(
                Expression queryExpression,
                OrderingField field)
            {
                string methodName = field.IsDescending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy);

                return Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new System.Type[] { typeof(TDbObject), field.Type },
                    queryExpression,
                    Expression.Lambda(ParameterReplacer.RepalceParameter(field.Expression), DbItem));
            }
        }

        private struct OrderingField
        {
            public PropertyInfo Property { get; set; }

            public System.Type Type { get; set; }

            public bool IsDescending { get; set; }

            public Expression Expression { get; set; }
        }

        private struct OrderingFields
        {
            public bool IsDirect { get; set; }

            public ParameterExpression OrderParameter { get; set; }

            public IList<OrderingField> Fields { get; set; }

            internal IEnumerable<object> Skip(int v)
            {
                throw new NotImplementedException();
            }
        }
    }
}
