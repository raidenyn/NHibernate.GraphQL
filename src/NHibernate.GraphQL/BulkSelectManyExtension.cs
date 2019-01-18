using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace NHibernate.GraphQL
{
    /// <summary>
    /// Provides splitting database requests to limit sql request body size
    /// </summary>
    public static class BulkSelectManyExtension
    {
        /// <summary>
        /// Default maximum count of ids passed to a database in one sql query in
        /// <see cref="BulkSelectMany{TDbObject, TResult, TJunction, TJuncedId, TResultId}(IQueryable{TDbObject}, Func{IQueryable{TDbObject}, IReadOnlyCollection{TJuncedId}, IQueryable{TJunction}}, Expression{Func{TJunction, TResult}}, Expression{Func{TJunction, TResultId}}, Expression{Func{TJunction, TJuncedId}}, IReadOnlyCollection{TJuncedId}, int)"/>
        /// and 
        /// <see cref="BulkSelectManyAsync{TDbObject, TResult, TJunction, TJuncedId, TResultId}(IQueryable{TDbObject}, Func{IQueryable{TDbObject}, IReadOnlyCollection{TJuncedId}, IQueryable{TJunction}}, Expression{Func{TJunction, TResult}}, Expression{Func{TJunction, TResultId}}, Expression{Func{TJunction, TJuncedId}}, IReadOnlyCollection{TJuncedId}, CancellationToken)"/>
        /// </summary>
        public const int DefaultBatchSize = 500;

        /// <summary>
        /// Bulk select from database for junced objects
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TJunction">Type for joining result and junced types</typeparam>
        /// <typeparam name="TJuncedId">Junced id type</typeparam>
        /// <typeparam name="TResultId">Result id type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="filter">Function for creating filtered query</param>
        /// <param name="select">Expression with selecting target result values</param>
        /// <param name="getResultId">Expression with extracting id of result object</param>
        /// <param name="getJuncedId">Expression with extracting id of junced object</param>
        /// <param name="ids">List of requested ids</param>
        /// <param name="batchSize">Maximum cid count in one select batch</param>
        /// <returns>List of result objects mapped to id</returns>
        public static ILookup<TJuncedId, TResult> BulkSelectMany<TDbObject, TResult, TJunction, TJuncedId, TResultId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TJuncedId>, IQueryable<TJunction>> filter,
            Expression<Func<TJunction, TResult>> select,
            Expression<Func<TJunction, TResultId>> getResultId,
            Expression<Func<TJunction, TJuncedId>> getJuncedId,
            IReadOnlyCollection<TJuncedId> ids,
            int batchSize = DefaultBatchSize)
            where TResult: class
        {
            var builder = new BulkSelectManyExpressionBuilder<TDbObject, TResult, TJunction, TJuncedId, TResultId>();

            var data = builder.LoadData(
                query,
                filter,
                select,
                getResultId,
                getJuncedId,
                ids,
                batchSize);

            return builder.ToLookup(data);
        }

        /// <summary>
        /// Bulk select from database for junced objects
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TJunction">Type for joining result and junced types</typeparam>
        /// <typeparam name="TJuncedId">Junced id type</typeparam>
        /// <typeparam name="TResultId">Result id type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="filter">Function for creating filtered query</param>
        /// <param name="select">Expression with selecting target result values</param>
        /// <param name="getResultId">Expression with extracting id of result object</param>
        /// <param name="getJuncedId">Expression with extracting id of junced object</param>
        /// <param name="ids">List of requested ids</param>
        /// <param name="batchSize">Maximum cid count in one select batch</param>
        /// <param name="cancellationToken">Cancellation tocken for sql requests</param>
        /// <returns>List of result objects mapped to id</returns>
        public static async Task<ILookup<TJuncedId, TResult>> BulkSelectManyAsync<TDbObject, TResult, TJunction, TJuncedId, TResultId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TJuncedId>, IQueryable<TJunction>> filter,
            Expression<Func<TJunction, TResult>> select,
            Expression<Func<TJunction, TResultId>> getResultId,
            Expression<Func<TJunction, TJuncedId>> getJuncedId,
            IReadOnlyCollection<TJuncedId> ids,
            int batchSize = DefaultBatchSize,
            CancellationToken cancellationToken = default)
            where TResult : class
        {
            var builder = new BulkSelectManyExpressionBuilder<TDbObject, TResult, TJunction, TJuncedId, TResultId>();

            var data = await builder.LoadDataAsync(
                query,
                filter,
                select,
                getResultId,
                getJuncedId,
                ids,
                batchSize,
                cancellationToken).ConfigureAwait(false);

            return builder.ToLookup(data);
        }

        /// <summary>
        /// Bulk select from database for junced objects
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TJunction">Type for joining result and junced types</typeparam>
        /// <typeparam name="TJuncedId">Junced id type</typeparam>
        /// <typeparam name="TResultId">Result id type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="filter">Function for creating filtered query</param>
        /// <param name="select">Expression with selecting target result values</param>
        /// <param name="getResultId">Expression with extracting id of result object</param>
        /// <param name="getJuncedId">Expression with extracting id of junced object</param>
        /// <param name="ids">List of requested ids</param>
        /// <param name="cancellationToken">Cancellation tocken for sql requests</param>
        /// <returns>List of result objects mapped to id</returns>
        public static Task<ILookup<TJuncedId, TResult>> BulkSelectManyAsync<TDbObject, TResult, TJunction, TJuncedId, TResultId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TJuncedId>, IQueryable<TJunction>> filter,
            Expression<Func<TJunction, TResult>> select,
            Expression<Func<TJunction, TResultId>> getResultId,
            Expression<Func<TJunction, TJuncedId>> getJuncedId,
            IReadOnlyCollection<TJuncedId> ids,
            CancellationToken cancellationToken = default)
            where TResult : class
        {
            return BulkSelectManyAsync(query, filter, select, getResultId, getJuncedId, ids, DefaultBatchSize, cancellationToken);
        }
    }
}
