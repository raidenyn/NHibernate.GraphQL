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
    public static class BulkSelectManyExtension
    {
        private const int DefaultBatchSize = 500;

        /// <summary>
        /// Bulk select from database for junced objects
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TJunctionId">Junced id type</typeparam>
        /// <typeparam name="TResultId">Result id type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="filtering">Function for creating filtered query</param>
        /// <param name="selectResult">Select result statement</param>
        /// <param name="selectJunction">Select junction statement</param>
        /// <param name="getId">Returns result id from result object</param>
        /// <param name="ids">List of requested ids</param>
        /// <param name="batchSize">Maximum cid count in one select batch</param>
        /// <returns>List of result objects mapped to id</returns>
        public static ILookup<TJunctionId, TResult> BulkSelectMany<TDbObject, TResult, TJunctionId, TResultId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TJunctionId>, IQueryable<TDbObject>> filtering,
            Func<IQueryable<TDbObject>, IQueryable<TResult>> selectResult,
            Func<IQueryable<TDbObject>, IQueryable<Junction<TResultId, TJunctionId>>> selectJunction,
            Func<TResult, TResultId> getId,
            IReadOnlyCollection<TJunctionId> ids,
            int batchSize = DefaultBatchSize)
        {
            Task<(IEnumerable<TResult>, IEnumerable<Junction<TResultId, TJunctionId>>)> Executor(
                IFutureEnumerable<TResult> resultQuery,
                IFutureEnumerable<Junction<TResultId, TJunctionId>> junctionQuery)
            {
                return Task.FromResult((resultQuery.GetEnumerable(), junctionQuery.GetEnumerable()));
            }

            return BulkSelectManyAsync(query, filtering, selectResult, selectJunction, getId, ids, DefaultBatchSize, Executor).Result;
        }

        /// <summary>
        /// Bulk select from database for junced objects
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TJunctionId">Junced id type</typeparam>
        /// <typeparam name="TResultId">Result id type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="filtering">Function for creating filtered query</param>
        /// <param name="selectResult">Select result statement</param>
        /// <param name="selectJunction">Select junction statement</param>
        /// <param name="getId">Returns result id from result object</param>
        /// <param name="ids">List of requested ids</param>
        /// <param name="batchSize">Maximum cid count in one select batch</param>
        /// <param name="cancellationToken">Cancellation tocken for sql requests</param>
        /// <returns>List of result objects mapped to id</returns>
        public static Task<ILookup<TJunctionId, TResult>> BulkSelectManyAsync<TDbObject, TResult, TJunctionId, TResultId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TJunctionId>, IQueryable<TDbObject>> filtering,
            Func<IQueryable<TDbObject>, IQueryable<TResult>> selectResult,
            Func<IQueryable<TDbObject>, IQueryable<Junction<TResultId, TJunctionId>>> selectJunction,
            Func<TResult, TResultId> getId,
            IReadOnlyCollection<TJunctionId> ids,
            int batchSize = DefaultBatchSize,
            CancellationToken cancellationToken = default)
        {
            async Task<(IEnumerable<TResult>, IEnumerable<Junction<TResultId, TJunctionId>>)> ExecutorAsync(
                IFutureEnumerable<TResult> resultQuery,
                IFutureEnumerable<Junction<TResultId, TJunctionId>> junctionQuery)
            {
                return (await resultQuery.GetEnumerableAsync().ConfigureAwait(false),
                        await junctionQuery.GetEnumerableAsync().ConfigureAwait(false));
            }

            return BulkSelectManyAsync(query, filtering, selectResult, selectJunction, getId, ids, DefaultBatchSize, ExecutorAsync);
        }

        /// <summary>
        /// Bulk select from database for junced objects
        /// </summary>
        /// <typeparam name="TDbObject">Database mapped object type</typeparam>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <typeparam name="TJunctionId">Junced id type</typeparam>
        /// <typeparam name="TResultId">Result id type</typeparam>
        /// <param name="query">Original database LINQ query</param>
        /// <param name="filtering">Function for creating filtered query</param>
        /// <param name="selectResult">Select result statement</param>
        /// <param name="selectJunction">Select junction statement</param>
        /// <param name="getId">Returns result id from result object</param>
        /// <param name="ids">List of requested ids</param>
        /// <param name="cancellationToken">Cancellation tocken for sql requests</param>
        /// <returns>List of result objects mapped to id</returns>
        public static Task<ILookup<TJunctionId, TResult>> BulkSelectManyAsync<TDbObject, TResult, TJunctionId, TResultId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TJunctionId>, IQueryable<TDbObject>> filtering,
            Func<IQueryable<TDbObject>, IQueryable<TResult>> selectResult,
            Func<IQueryable<TDbObject>, IQueryable<Junction<TResultId, TJunctionId>>> selectJunction,
            Func<TResult, TResultId> getId,
            IReadOnlyCollection<TJunctionId> ids,
            CancellationToken cancellationToken = default)
        {
            return BulkSelectManyAsync(query, filtering, selectResult, selectJunction, getId, ids, DefaultBatchSize, cancellationToken);
        }

        private static async Task<ILookup<TJunctionId, TResult>> BulkSelectManyAsync<TDbObject, TResult, TJunctionId, TResultId>(
            this IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TJunctionId>, IQueryable<TDbObject>> filtering,
            Func<IQueryable<TDbObject>, IQueryable<TResult>> selectResult,
            Func<IQueryable<TDbObject>, IQueryable<Junction<TResultId, TJunctionId>>> selectJunction,
            Func<TResult, TResultId> id,
            IReadOnlyCollection<TJunctionId> values,
            int batchSize,
            Func<IFutureEnumerable<TResult>, IFutureEnumerable<Junction<TResultId, TJunctionId>>, Task<(IEnumerable<TResult>, IEnumerable<Junction<TResultId, TJunctionId>>)>> executor)
        {
            var results = new List<IEnumerable<TResult>>(capacity: values.Count / batchSize + 1);
            var junctions = new List<IEnumerable<Junction<TResultId, TJunctionId>>>(capacity: results.Count);

            for (int offset = 0; offset < results.Count; offset += batchSize)
            {
                TJunctionId[] batchValues = values.Skip(offset).Take(batchSize).ToArray();

                IQueryable<TDbObject> batchQuery = filtering(query, batchValues);

                IFutureEnumerable<TResult> resultQuery = selectResult(batchQuery).Distinct().ToFuture();
                IFutureEnumerable<Junction<TResultId, TJunctionId>> junctionQuery = selectJunction(batchQuery).Distinct().ToFuture();

                var (result, junction) = await executor(resultQuery, junctionQuery).ConfigureAwait(false);

                results.Add(result);
                junctions.Add(junction);
            }

            Dictionary<TResultId, TResult> dic = results.SelectMany(result => result).ToDictionary(id);

            return junctions
                .SelectMany(junction => junction)
                .Select(item =>
                {
                    if (dic.TryGetValue(item.ResultId, out var result))
                    {
                        return new { item.JunctionId, Result = result };
                    }
                    return null;
                })
                .Where(item => item != null)
                .ToLookup(item => item.JunctionId, item => item.Result);
        }
    }
}
