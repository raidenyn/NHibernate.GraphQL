using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Linq;

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
        /// <param name="settings">Setting data. Default if null.</param>
        /// <returns>Loaded structure of requested connection</returns>
        public static Connection<TResult> ToConnection<TResult, TDbObject, TOrder>(
            this IQueryable<TDbObject> query,
            Expression<Func<TDbObject, TOrder>> orderBy,
            Expression<Func<TOrder, TOrder, bool>> filter,
            Expression<Func<TDbObject, TResult>> select,
            ICursorRequest request,
            IConnectionQuerySettings settings = null)
        {
            settings = settings ?? ConnectionQuerySettings.Default;
            var builder = new ConnectionExpressionBuilder<TResult, TDbObject, TOrder>(settings.CursorFormatter);

            var connectionQuery = builder.BuildAfterQuery(query, orderBy, filter, select, request.After);

            connectionQuery = builder.LimitSizePlusOneQuery(connectionQuery, request.First);

            var totalCount = query.ToFutureValue(items => items.Count());
            var connectionItems = connectionQuery.ToFuture().GetEnumerable();

            (List<Edge<TResult>> edges, bool hasNext) = builder.GetEdges(connectionQuery, request.First);

            bool hasPrevious = request.After != null;

            return GetConnection(edges, query.Count(), hasNext, hasPrevious);
        }

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
        /// <param name="settings">Setting data. Default if null.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Loaded structure of requested connection</returns>
        public static async Task<Connection<TResult>> ToConnectionAsync<TResult, TDbObject, TOrder>(
            this IQueryable<TDbObject> query,
            Expression<Func<TDbObject, TOrder>> orderBy,
            Expression<Func<TOrder, TOrder, bool>> filter,
            Expression<Func<TDbObject, TResult>> select,
            ICursorRequest request,
            IConnectionQuerySettings settings = null,
            CancellationToken cancellationToken = default)
        {
            settings = settings ?? ConnectionQuerySettings.Default;
            var builder = new ConnectionExpressionBuilder<TResult, TDbObject, TOrder>(settings.CursorFormatter);

            var connectionQuery = builder.BuildAfterQuery(query, orderBy, filter, select, request.After);

            connectionQuery = builder.LimitSizePlusOneQuery(connectionQuery, request.First);

            var totalCount = query.ToFutureValue(items => items.Count());
            var connectionItems = await connectionQuery.ToFuture().GetEnumerableAsync(cancellationToken).ConfigureAwait(false);

            (List<Edge<TResult>> edges, bool hasNext) = builder.GetEdges(connectionItems, request.First);

            bool hasPrevious = request.After != null;

            return GetConnection(edges, totalCount.Value, hasNext, hasPrevious);
        }

        private static Connection<TResult> GetConnection<TResult>(
            List<Edge<TResult>> edges,
            int totalCount,
            bool hasNext,
            bool hasPrevious)
        {
            bool any = edges.Count > 0;

            return new Connection<TResult>
            {
                Edges = edges,
                PageInfo = new PageInfo
                {
                    HasNextPage = hasNext,
                    HasPreviousPage = hasPrevious,
                    StartCursor = any ? edges[0].Cursor : null,
                    EndCursor = any ? edges[edges.Count - 1].Cursor : null,
                },
                TotalCount = totalCount,
            };
        }
    }
}
