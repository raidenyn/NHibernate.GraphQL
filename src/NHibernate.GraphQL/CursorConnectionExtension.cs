using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;

namespace NHibernate.GraphQL
{
    public static class CursorConnectionExtension
    {
        public static Connection<TResult> ToConnection<TResult, TDbObject, TOrder>(
            this IQueryable<TDbObject> query,
            Expression<Func<TDbObject, TOrder>> orderBy,
            Expression<Func<TOrder, TOrder, bool>> filter,
            Expression<Func<TDbObject, TResult>> select,
            ICursorRequest request)
        {
            ParameterExpression pe = Expression.Parameter(typeof(TDbObject), "dbItem");
            var newOrderedItem = Expression.New(typeof(OrderedItem<TResult, TOrder>));

            MemberInfo orderMember = typeof(OrderedItem<TResult, TOrder>).GetMember(nameof(OrderedItem<TResult, TOrder>.Order))[0];
            MemberInfo valueMember = typeof(OrderedItem<TResult, TOrder>).GetMember(nameof(OrderedItem<TResult, TOrder>.Value))[0];

            MemberBinding orderBinding = Expression.Bind(
                orderMember,
                new ParameterReplacer(pe).RepalceParameter(orderBy.Body));
            MemberBinding resultBinding = Expression.Bind(
                valueMember,
                new ParameterReplacer(pe).RepalceParameter(select.Body));

            MemberInitExpression orderedItemInit = Expression.MemberInit(
                newOrderedItem,
                orderBinding,
                resultBinding);

            Expression<Func<TDbObject, OrderedItem<TResult, TOrder>>> newSelect
                = Expression.Lambda<Func<TDbObject, OrderedItem<TResult, TOrder>>>(orderedItemInit, new ParameterExpression[] { pe });

            Expression queryExpression = query.Expression;

            if (!String.IsNullOrWhiteSpace(request.After))
            {
                TOrder order = JsonConvert.DeserializeObject<TOrder>(request.After);

                queryExpression = Expression.Call(
                    typeof(Queryable),
                    nameof(Queryable.Where),
                    new System.Type[] { typeof(TDbObject) },
                    queryExpression,
                    Expression.Lambda<Func<TDbObject, bool>>(RemapFilter(orderedItemInit, orderMember, filter, order), pe)
                    );
            }

            queryExpression = Expression.Call(
                typeof(Queryable),
                nameof(Queryable.OrderBy),
                new System.Type[] { typeof(TDbObject), typeof(TOrder) },
                queryExpression,
                new ParameterReplacer(pe).RepalceParameter(orderBy));

            queryExpression = Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Select),
                new System.Type[] { typeof(TDbObject), typeof(OrderedItem<TResult, TOrder>) },
                queryExpression,
                newSelect);

            var orderedValueQuery = query.Provider.CreateQuery<OrderedItem<TResult, TOrder>>(queryExpression);

            if (request.First > 0) {
                orderedValueQuery = orderedValueQuery.Take(request.First.Value);
            }

            var (edges, hasNext) = GetEdges(orderedValueQuery, request.First);

            var any = edges.Count > 0;

            return new Connection<TResult>
            {
                Edges = edges,
                PageInfo = new PageInfo
                {
                    HasNextPage = hasNext,
                    HasPreviousPage = request.After != null,
                    StartCursor = any ? edges[0].Cursor : null,
                    EndCursor = any ? edges[edges.Count - 1].Cursor : null,
                },
                TotalCount = query.Count(),
            };
        }

        private static Expression RemapFilter<TOrder>(
            Expression mappingExpression,
            MemberInfo mainMember,
            Expression<Func<TOrder, TOrder, bool>> filter,
            TOrder value)
        {
            var expression = new RemapParameters(filter.Parameters[0],
                mainMember,
                mappingExpression).RemapParmeters(filter.Body);
            return new ParameterChanger(
                filter.Parameters[1],
                Expression.Constant(value)).ChangeParameter(expression);
        }

        private static (List<Edge<TResult>> Edges, bool HasNext) GetEdges<TResult, TOrder>(IQueryable<OrderedItem<TResult, TOrder>> result, int? size)
        {
            if (size == null)
            {
                return (Edges: result.Select(GetEdge).ToList(), HasNext: false);
            }

            return GetEdges(result.AsEnumerable(), size.Value);
        }

        private static (List<Edge<TResult>> Edges, bool HasNext) GetEdges<TResult, TOrder>(IEnumerable<OrderedItem<TResult, TOrder>> result, int size)
        {
            var list = new List<Edge<TResult>>(capacity: size);
            int counter = size;

            var iterator = result.GetEnumerator();

            while(counter > 0 && iterator.MoveNext())
            {
                list.Add(GetEdge(iterator.Current));
                counter--;
            }

            return (list, iterator.MoveNext());
        }

        private static Edge<TResult> GetEdge<TResult, TOrder>(OrderedItem<TResult, TOrder> item)
        {
            return new Edge<TResult>
            {
                Cursor = item.Order?.ToString(),
                Node = item.Value
            };
        }

        class OrderedItem<TResult, TOrder>
        {
            public TResult Value { get; set; }

            public TOrder Order { get; set; }
        }
    }
}
