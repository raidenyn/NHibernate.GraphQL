using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;

namespace NHibernate.GraphQL
{
    /// <summary>
    /// Extension methods for generating GraphQL Connections from NHiberante IQuariable query
    /// </summary>
    public static class CursorConnectionExtension
    {
        /// <summary>
        /// Generate Connection structure from NHinerate query
        /// </summary>
        /// <typeparam name="TResult">Type of result object</typeparam>
        /// <typeparam name="TDbObject">Type of database mapped object</typeparam>
        /// <typeparam name="TOrder">Type for representing element ordering</typeparam>
        /// <param name="query">Original NHibernate query</param>
        /// <param name="orderBy">Expression with ordering clause</param>
        /// <param name="filter">Expression with filtering clause</param>
        /// <param name="select">Expression with selecting clause</param>
        /// <param name="request">Unidirectional request</param>
        /// <returns>Loaded structure of requested connection</returns>
        public static Connection<TResult> ToConnection<TResult, TDbObject, TOrder>(
            this IQueryable<TDbObject> query,
            Expression<Func<TDbObject, TOrder>> orderBy,
            Expression<Func<TOrder, TOrder, bool>> filter,
            Expression<Func<TDbObject, TResult>> select,
            ICursorRequest request)
        {
            var builder = new ConnectionExpressionBuilder<TResult, TDbObject, TOrder>();

            var orderedValueQuery = builder.Build(query, orderBy, filter, select, request.After);

            if (request.First > 0) {
                orderedValueQuery = orderedValueQuery.Take(request.First.Value);
            }

            var (edges, hasNext) = builder.GetEdges(orderedValueQuery, request.First);

            bool any = edges.Count > 0;

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
    }
}
