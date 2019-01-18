using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Linq;

namespace NHibernate.GraphQL
{
    internal class BulkSelectExpressionBuilder<TDbObject, TResult, TJunction, TJoinedId>
    {
        private static readonly ParameterExpression junction = Expression.Parameter(typeof(TJunction), "junction");
        private static readonly ParameterReplacer ParameterReplacer = new ParameterReplacer(junction);

        private static readonly ConstructorInfo IdResultPairConstructor = typeof(IdResultPair).GetConstructor(new[] {
            typeof(TJoinedId),
            typeof(TResult)
        }) ?? throw new NotSupportedException($"Required constructor for {nameof(IdResultPair)} is not found.");

        public IQueryable<IdResultPair> GetSelectResultQuery(
            IQueryable<TJunction> query,
            Expression<Func<TJunction, TResult>> select,
            Expression<Func<TJunction, TJoinedId>> getJoinedId)
        {
            var selectBody = Expression.New(IdResultPairConstructor,
                ParameterReplacer.RepalceParameter(getJoinedId.Body),
                ParameterReplacer.RepalceParameter(select.Body));

            var selectExpression = Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Select),
                new System.Type[] { typeof(TJunction), typeof(IdResultPair) },
                query.Expression,
                Expression.Lambda<Func<TJunction, IdResultPair>>(selectBody, junction));

            return query.Provider.CreateQuery<IdResultPair>(selectExpression);
        }

        public async Task<IEnumerable<IdResultPair>> LoadDataAsync(
            IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TJoinedId>, IQueryable<TJunction>> filter,
            Expression<Func<TJunction, TResult>> select,
            Expression<Func<TJunction, TJoinedId>> getJoinedId,
            IReadOnlyCollection<TJoinedId> ids,
            int batchSize,
            CancellationToken cancellationToken = default)
        {
            var results = new List<List<IdResultPair>>(capacity: ids.Count / batchSize + 1);

            for (int offset = 0; offset < ids.Count; offset += batchSize)
            {
                var idsBatch = ids.Skip(offset).Take(batchSize).ToArray();

                var batchQuery = filter(query, idsBatch);

                var idItemsQuery = GetSelectResultQuery(batchQuery, select, getJoinedId);

                results.Add(await idItemsQuery.ToListAsync(cancellationToken).ConfigureAwait(false));
            }

            return results.SelectMany(result => result);
        }

        public IEnumerable<IdResultPair> LoadData(
            IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TJoinedId>, IQueryable<TJunction>> filter,
            Expression<Func<TJunction, TResult>> select,
            Expression<Func<TJunction, TJoinedId>> getJoinedId,
            IReadOnlyCollection<TJoinedId> ids,
            int batchSize)
        {
            var results = new List<List<IdResultPair>>(capacity: ids.Count / batchSize + 1);

            for (int offset = 0; offset < ids.Count; offset += batchSize)
            {
                var idsBatch = ids.Skip(offset).Take(batchSize).ToArray();

                var batchQuery = filter(query, idsBatch);

                var idItemsQuery = GetSelectResultQuery(batchQuery, select, getJoinedId);

                results.Add(idItemsQuery.ToList());
            }

            return results.SelectMany(result => result);
        }

        public IDictionary<TJoinedId, TResult> ToDictionary(IEnumerable<IdResultPair> pairs)
        {
            return pairs.ToDictionary(pair => pair.Id, pair => pair.Item);
        }

        internal struct IdResultPair
        {
            public IdResultPair(TJoinedId id, TResult item)
            {
                Id = id;
                Item = item;
            }

            public TJoinedId Id { get; }

            public TResult Item { get; }
        }
    }
}
