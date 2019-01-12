using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;

namespace NHibernate.GraphQL
{
    internal class ConnectionExpressionBuilder<TResult, TDbObject, TOrder>
    {
        private static readonly MemberInfo OrderMember = typeof(OrderedItem).GetMember(nameof(OrderedItem.Order))[0];
        private static readonly MemberInfo ValueMember = typeof(OrderedItem).GetMember(nameof(OrderedItem.Value))[0];

        private static readonly ParameterExpression DbItem = Expression.Parameter(typeof(TDbObject), "dbItem");

        private static readonly ParameterReplacer ParameterReplacer = new ParameterReplacer(DbItem);

        public IQueryable<OrderedItem> Build(
            IQueryable<TDbObject> query,
            Expression<Func<TDbObject, TOrder>> orderBy,
            Expression<Func<TOrder, TOrder, bool>> filter,
            Expression<Func<TDbObject, TResult>> select,
            string after)
        {
            MemberInitExpression orderedItemInit = Expression.MemberInit(
                Expression.New(typeof(OrderedItem)),
                Expression.Bind(OrderMember, ParameterReplacer.RepalceParameter(orderBy.Body)),
                Expression.Bind(ValueMember, ParameterReplacer.RepalceParameter(select.Body)));

            Expression queryExpression = query.Expression;

            if (!String.IsNullOrWhiteSpace(after))
            {
                TOrder order = JsonConvert.DeserializeObject<TOrder>(after);

                // add where filtration
                queryExpression = Expression.Call(
                    typeof(Queryable),
                    nameof(Queryable.Where),
                    new System.Type[] { typeof(TDbObject) },
                    queryExpression,
                    Expression.Lambda<Func<TDbObject, bool>>(RemapFilter(orderedItemInit, OrderMember, filter, order), DbItem)
                    );
            }

            // add passed order
            queryExpression = Expression.Call(
                typeof(Queryable),
                nameof(Queryable.OrderBy),
                new System.Type[] { typeof(TDbObject), typeof(TOrder) },
                queryExpression,
                ParameterReplacer.RepalceParameter(orderBy));

            // extend selection and add order fields
            queryExpression = Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Select),
                new System.Type[] { typeof(TDbObject), typeof(OrderedItem) },
                queryExpression,
                Expression.Lambda<Func<TDbObject, OrderedItem>>(orderedItemInit, DbItem));

            return query.Provider.CreateQuery<OrderedItem>(queryExpression);
        }

        public EdgesList<TResult> GetEdges(IQueryable<OrderedItem> result, int? size)
        {
            if (size == null)
            {
                return new EdgesList<TResult>(result.Select(GetEdge).ToList(), hasNext: false);
            }

            return GetEdgesList(result, size.Value);
        }

        private static Expression RemapFilter(
            Expression mappingExpression,
            MemberInfo mainMember,
            Expression<Func<TOrder, TOrder, bool>> filter,
            TOrder value)
        {
            var expression = new RemapParameters(
                filter.Parameters[0],
                mainMember,
                mappingExpression).RemapParmeters(filter.Body);
            return new ParameterChanger(
                filter.Parameters[1],
                Expression.Constant(value)).ChangeParameter(expression);
        }

        private static EdgesList<TResult> GetEdgesList(
            IEnumerable<OrderedItem> result, int size)
        {
            var list = new List<Edge<TResult>>(capacity: size);
            int counter = size;

            var iterator = result.GetEnumerator();

            while (counter > 0 && iterator.MoveNext())
            {
                list.Add(GetEdge(iterator.Current));
                counter--;
            }

            return new EdgesList<TResult>(list, iterator.MoveNext());
        }

        private static Edge<TResult> GetEdge(OrderedItem item)
        {
            return new Edge<TResult>
            {
                Cursor = item.Order?.ToString(),
                Node = item.Value
            };
        }

        internal class OrderedItem
        {
            public TResult Value { get; set; }

            public TOrder Order { get; set; }
        }
    }
}
