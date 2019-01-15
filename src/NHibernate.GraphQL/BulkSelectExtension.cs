using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Linq;

namespace NHibernate.GraphQL
{
    /// <summary>
    /// Provides splitting database requests to limit sql request body size
    /// </summary>
    public static class BulkQueryExtension
    {
        private const int DefaultBatchSize = 1000;

        /// <summary>
        /// Synchroniosly requests data in databse
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TId">Parameter object type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="select">Function to difine filteration by the ids and selection result objects</param>
        /// <param name="ids">Collection of the id objects</param>
        /// <param name="batchSize">Maximum count of ids in one select</param>
        /// <returns></returns>
        public static IEnumerable<TResult> BulkSelect<TDbObject, TResult, TId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TId>, IQueryable<TResult>> select,
            IReadOnlyCollection<TId> ids,
            int batchSize = DefaultBatchSize)
        {
            return BulkSelectAsync(query, select, ids, batchSize, batch => Task.FromResult(batch.ToList())).Result;
        }

        /// <summary>
        /// Asynchroniosly requests data in databse
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TId">Parameter object type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="select">Function to difine filteration by the ids and selection result objects</param>
        /// <param name="ids">Collection of the id objects</param>
        /// <param name="batchSize">Maximum count of ids in one select</param>
        /// <param name="cancellationToken">Token to cancel the requests</param>
        /// <returns></returns>
        public static Task<IEnumerable<TResult>> BulkSelectAsync<TDbObject, TResult, TId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TId>, IQueryable<TResult>> select,
            IReadOnlyCollection<TId> ids,
            int batchSize = DefaultBatchSize,
            CancellationToken cancellationToken = default)
        {
            return BulkSelectAsync(query, select, ids, batchSize, batch => batch.ToListAsync(cancellationToken));
        }

        /// <summary>
        /// Asynchroniosly requests data in databse
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TId">Parameter object type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="select">Function to difine filteration by the ids and selection result objects</param>
        /// <param name="ids">Collection of the id objects</param>
        /// <param name="cancellationToken">Token to cancel the requests</param>
        /// <returns></returns>
        public static Task<IEnumerable<TResult>> BulkSelectAsync<TDbObject, TResult, TId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TId>, IQueryable<TResult>> select,
            IReadOnlyCollection<TId> ids,
            CancellationToken cancellationToken = default)
        {
            return BulkSelectAsync(query, select, ids, DefaultBatchSize, cancellationToken);
        }

        /// <summary>
        /// Synchroniosly requests data in databse
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TId">Parameter object type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="select">Function to difine filteration by the ids and selection result objects</param>
        /// <param name="getId">Function to return id from result object</param>
        /// <param name="ids">Collection of the id objects</param>
        /// <param name="batchSize">Maximum count of ids in one select</param>
        /// <returns></returns>
        public static IDictionary<TId, TResult> BulkSelect<TDbObject, TResult, TId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TId>, IQueryable<TResult>> select,
            Func<TResult, TId> getId,
            IReadOnlyCollection<TId> ids,
            int batchSize = DefaultBatchSize)
        {
            return BulkSelect(query, select, ids, batchSize).ToDictionary(item => getId(item), item => item);
        }

        /// <summary>
        /// Asynchroniosly requests data in databse
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TId">Parameter object type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="select">Function to difine filteration by the ids and selection result objects</param>
        /// <param name="getId">Function to return id from result object</param>
        /// <param name="ids">Collection of the id objects</param>
        /// <param name="batchSize">Maximum count of ids in one select</param>
        /// <param name="cancellationToken">Token to cancel the requests</param>
        /// <returns></returns>
        public static async Task<IDictionary<TId, TResult>> BulkSelectAsync<TDbObject, TResult, TId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TId>, IQueryable<TResult>> select,
            Func<TResult, TId> getId,
            IReadOnlyCollection<TId> ids,
            int batchSize = DefaultBatchSize,
            CancellationToken cancellationToken = default)
        {
            var results = await BulkSelectAsync(query, select, ids, batchSize, batch => batch.ToListAsync(cancellationToken)).ConfigureAwait(false);

            return results.ToDictionary(item => getId(item), item => item);
        }

        /// <summary>
        /// Asynchroniosly requests data in databse
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TId">Parameter object type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="select">Function to difine filteration by the ids and selection result objects</param>
        /// <param name="getId">Function to return id from result object</param>
        /// <param name="ids">Collection of the id objects</param>
        /// <param name="cancellationToken">Token to cancel the requests</param>
        /// <returns></returns>
        public static Task<IDictionary<TId, TResult>> BulkSelectAsync<TDbObject, TResult, TId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TId>, IQueryable<TResult>> select,
            Func<TResult, TId> getId,
            IReadOnlyCollection<TId> ids,
            CancellationToken cancellationToken = default)
        {
            return BulkSelectAsync(query, select, getId, ids, DefaultBatchSize, cancellationToken);
        }

        private static async Task<IEnumerable<TResult>> BulkSelectAsync<TDbObject, TResult, TId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TId>, IQueryable<TResult>> select,
            IReadOnlyCollection<TId> ids,
            int batchSize,
            Func<IQueryable<TResult>, Task<List<TResult>>> execute)
        {
            var results = new List<List<TResult>>(capacity: ids.Count / batchSize + 1);

            for (int offset = 0; offset < ids.Count; offset += batchSize)
            {
                var idsBatch = ids.Skip(offset).Take(batchSize).ToArray();

                var batchQuery = select(query, idsBatch);

                results.Add(await batchQuery.ToListAsync().ConfigureAwait(false));
            }

            return results.SelectMany(result => result);
        }
    }
}
