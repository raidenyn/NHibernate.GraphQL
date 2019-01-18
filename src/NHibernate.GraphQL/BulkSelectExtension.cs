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
    public static class BulkSelectExtension
    {
        /// <summary>
        /// Default maximum count of ids passed to a database in one sql query in
        /// <see cref="BulkSelect{TDbObject, TResult, TJunction, TJoinedId}(IQueryable{TDbObject}, Func{IQueryable{TDbObject}, IReadOnlyCollection{TJoinedId}, IQueryable{TJunction}}, Expression{Func{TJunction, TResult}}, Expression{Func{TJunction, TJoinedId}}, IReadOnlyCollection{TJoinedId}, int)"/>
        /// and 
        /// <see cref="BulkSelectAsync{TDbObject, TResult, TJunction, TJoinedId}(IQueryable{TDbObject}, Func{IQueryable{TDbObject}, IReadOnlyCollection{TJoinedId}, IQueryable{TJunction}}, Expression{Func{TJunction, TResult}}, Expression{Func{TJunction, TJoinedId}}, IReadOnlyCollection{TJoinedId}, int)"/> methods.
        /// </summary>
        public const int DefaultBatchSize = 1000;

        /// <summary>
        /// Synchroniosly requests data in databse
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TJunction">Intermediate object type to represent result object and joined id</typeparam>
        /// <typeparam name="TJoinedId">Parameter object type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="filter">Function to difine filteration by the ids result objects</param>
        /// <param name="select">Expression to extract result objects</param>
        /// <param name="getJoinedId">Expression to extract joined id</param>
        /// <param name="ids">Collection of the id objects</param>
        /// <param name="batchSize">Maximum count of ids in one select</param>
        /// <returns>Dictionary of the result selection</returns>
        public static IDictionary<TJoinedId, TResult> BulkSelect<TDbObject, TResult, TJunction, TJoinedId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TJoinedId>, IQueryable<TJunction>> filter,
            Expression<Func<TJunction, TResult>> select,
            Expression<Func<TJunction, TJoinedId>> getJoinedId,
            IReadOnlyCollection<TJoinedId> ids,
            int batchSize = DefaultBatchSize)
            where TResult : class
        {
            var builder = new BulkSelectExpressionBuilder<TDbObject, TResult, TJunction, TJoinedId>();

            var data = builder.LoadData(
                query,
                filter,
                select,
                getJoinedId,
                ids,
                batchSize);

            return builder.ToDictionary(data);
        }

        /// <summary>
        /// Asynchroniosly requests data in database
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TJunction">Intermediate object type to represent result object and joined id</typeparam>
        /// <typeparam name="TJoinedId">Parameter object type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="filter">Function to difine filteration by the ids result objects</param>
        /// <param name="select">Expression to extract result objects</param>
        /// <param name="getJoinedId">Expression to extract joined id</param>
        /// <param name="ids">Collection of the id objects</param>
        /// <param name="batchSize">Maximum count of ids in one select</param>
        /// <returns>Dictionary of the result selection</returns>
        public static async Task<IDictionary<TJoinedId, TResult>> BulkSelectAsync<TDbObject, TResult, TJunction, TJoinedId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TJoinedId>, IQueryable<TJunction>> filter,
            Expression<Func<TJunction, TResult>> select,
            Expression<Func<TJunction, TJoinedId>> getJoinedId,
            IReadOnlyCollection<TJoinedId> ids,
            int batchSize)
            where TResult : class
        {
            var builder = new BulkSelectExpressionBuilder<TDbObject, TResult, TJunction, TJoinedId>();

            var data = await builder.LoadDataAsync(
                query,
                filter,
                select,
                getJoinedId,
                ids,
                batchSize).ConfigureAwait(false);

            return builder.ToDictionary(data);
        }

        /// <summary>
        /// Asynchroniosly requests data in database
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TJunction">Intermediate object type to represent result object and joined id</typeparam>
        /// <typeparam name="TJoinedId">Parameter object type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="filter">Function to difine filteration by the ids result objects</param>
        /// <param name="select">Expression to extract result objects</param>
        /// <param name="getJoinedId">Expression to extract joined id</param>
        /// <param name="ids">Collection of the id objects</param>
        /// <param name="cancellationToken">Token to cancel the requests</param>
        /// <returns>Dictionary of the result selection</returns>
        public static Task<IDictionary<TJoinedId, TResult>> BulkSelectAsync<TDbObject, TResult, TJunction, TJoinedId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TJoinedId>, IQueryable<TJunction>> filter,
            Expression<Func<TJunction, TResult>> select,
            Expression<Func<TJunction, TJoinedId>> getJoinedId,
            IReadOnlyCollection<TJoinedId> ids,
            CancellationToken cancellationToken = default)
            where TResult : class
        {
            return BulkSelectAsync(query, filter, select, getJoinedId, ids, DefaultBatchSize, cancellationToken);
        }

        /// <summary>
        /// Asynchroniosly requests data in database
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TJunction">Intermediate object type to represent result object and joined id</typeparam>
        /// <typeparam name="TJoinedId">Parameter object type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="filter">Function to difine filteration by the ids result objects</param>
        /// <param name="select">Expression to extract result objects</param>
        /// <param name="getJoinedId">Expression to extract joined id</param>
        /// <param name="ids">Collection of the id objects</param>
        /// <param name="batchSize">Maximum count of ids in one select</param>
        /// <param name="cancellationToken">Token to cancel the requests</param>
        /// <returns>Dictionary of the result selection</returns>
        public static Task<IDictionary<TJoinedId, TResult>> BulkSelectAsync<TDbObject, TResult, TJunction, TJoinedId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TJoinedId>, IQueryable<TJunction>> filter,
            Expression<Func<TJunction, TResult>> select,
            Expression<Func<TJunction, TJoinedId>> getJoinedId,
            IReadOnlyCollection<TJoinedId> ids,
            int batchSize,
            CancellationToken cancellationToken = default)
            where TResult : class
        {
            return BulkSelectAsync(query, filter, select, getJoinedId, ids, DefaultBatchSize, cancellationToken);
        }
    }
}
