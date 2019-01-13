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
        private readonly ICursorFormatter _cursorFormatter;

        public ConnectionExpressionBuilder(ICursorFormatter cursorFormatter)
        {
            _cursorFormatter = cursorFormatter ?? throw new ArgumentNullException(nameof(cursorFormatter));
        }

        public IQueryable<OrderedItem> BuildAfterQuery(
            IQueryable<TDbObject> query,
            Expression<Func<TDbObject, TOrder>> orderBy,
            Expression<Func<TOrder, TOrder, bool>> filter,
            Expression<Func<TDbObject, TResult>> select,
            Cursor after)
        {
            MemberInitExpression orderedItemInit = Expression.MemberInit(
                Expression.New(typeof(OrderedItem)),
                Expression.Bind(OrderMember, ParameterReplacer.RepalceParameter(orderBy.Body)),
                Expression.Bind(ValueMember, ParameterReplacer.RepalceParameter(select.Body)));

            Expression queryExpression = query.Expression;

            if (_cursorFormatter.HasValue(after))
            {
                TOrder order = _cursorFormatter.ParseAs<TOrder>(after);

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

        public IQueryable<OrderedItem> LimitSizeQuery(
            IQueryable<OrderedItem> query,
            int? size)
        {
            if (size > 0)
            {
                query = query.Take(size.Value);
            }
            return query;
        }

        public EdgesList<TResult> GetEdges(IEnumerable<OrderedItem> result, int? size)
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
            var expression = new RemapParametersVisitor(
                filter.Parameters[0],
                mainMember,
                mappingExpression).RemapParmeters(filter.Body);
            return new ParameterVisitor(
                filter.Parameters[1],
                Expression.Constant(value)).ChangeParameter(expression);
        }

        private EdgesList<TResult> GetEdgesList(
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

        private Edge<TResult> GetEdge(OrderedItem item)
        {
            return new Edge<TResult>
            {
                Cursor = _cursorFormatter.Format(item.Order),
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
