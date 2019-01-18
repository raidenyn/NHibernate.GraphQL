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
    internal class BulkSelectManyExpressionBuilder<TDbObject, TResult, TJunction, TJuncedId, TResultId>
    {
        private static readonly ParameterExpression Junction = Expression.Parameter(typeof(TJunction), "junction");
        private static readonly ParameterReplacer ParameterReplacer = new ParameterReplacer(Junction);

        private static readonly ConstructorInfo JunctionConstructor = typeof(JunctionIds).GetConstructor(new[] {
            typeof(TResultId),
            typeof(TJuncedId)
        }) ?? throw new NotSupportedException($"Required constructor for {nameof(Junction)} is not found.");
        private static readonly ConstructorInfo IdResultPairConstructor = typeof(IdResultPair).GetConstructor(new[] {
            typeof(TResultId),
            typeof(TResult)
        }) ?? throw new NotSupportedException($"Required constructor for {nameof(IdResultPair)} is not found.");

        public IQueryable<JunctionIds> GetJuncedIdsQuery(
            IQueryable<TJunction> query,
            Expression<Func<TJunction, TResultId>> getResultId,
            Expression<Func<TJunction, TJuncedId>> getJuncedId)
        {
            var selectBody = Expression.New(JunctionConstructor,
                ParameterReplacer.RepalceParameter(getResultId.Body),
                ParameterReplacer.RepalceParameter(getJuncedId.Body));

            var selectExpression = Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Select),
                new System.Type[] { typeof(TJunction), typeof(JunctionIds) },
                query.Expression,
                Expression.Lambda<Func<TJunction, JunctionIds>>(selectBody, Junction));

            return query.Provider.CreateQuery<JunctionIds>(selectExpression);
        }

        public IQueryable<IdResultPair> GetSelectResultQuery(
            IQueryable<TJunction> query,
            Expression<Func<TJunction, TResult>> select,
            Expression<Func<TJunction, TResultId>> getResultId)
        {
            var selectBody = Expression.New(IdResultPairConstructor,
                ParameterReplacer.RepalceParameter(getResultId.Body),
                ParameterReplacer.RepalceParameter(select.Body));

            var selectExpression = Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Select),
                new System.Type[] { typeof(TJunction), typeof(IdResultPair) },
                query.Expression,
                Expression.Lambda<Func<TJunction, IdResultPair>>(selectBody, Junction));

            return query.Provider.CreateQuery<IdResultPair>(selectExpression);
        }

        public async Task<JuncedData> LoadDataAsync(
            IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TJuncedId>, IQueryable<TJunction>> filter,
            Expression<Func<TJunction, TResult>> select,
            Expression<Func<TJunction, TResultId>> getResultId,
            Expression<Func<TJunction, TJuncedId>> getJuncedId,
            IReadOnlyCollection<TJuncedId> values,
            int batchSize,
            CancellationToken cancellationToken = default)
        {
            int batchCount = values.Count / batchSize + 1;

            var results = new List<IEnumerable<IdResultPair>>(capacity: batchCount);
            var junctions = new List<IEnumerable<JunctionIds>>(capacity: batchCount);

            for (int offset = 0; offset < values.Count; offset += batchSize)
            {
                TJuncedId[] batchValues = values.Skip(offset).Take(batchSize).ToArray();

                IQueryable<TJunction> batchQuery = filter(query, batchValues);

                var resultQuery = GetSelectResultQuery(batchQuery, select, getResultId).Distinct().ToFuture();
                var junctionQuery = GetJuncedIdsQuery(batchQuery, getResultId, getJuncedId).Distinct().ToFuture();

                var resultItems = await resultQuery.GetEnumerableAsync(cancellationToken).ConfigureAwait(false);
                var junctionItems = await junctionQuery.GetEnumerableAsync(cancellationToken).ConfigureAwait(false);

                results.Add(resultItems);
                junctions.Add(junctionItems);
            }

            return new JuncedData
            {
                Junctions = junctions.SelectMany(item => item),
                Results = results.SelectMany(item => item).ToArray(),
            };
        }

        public JuncedData LoadData(
            IQueryable<TDbObject> query,
            Func<IQueryable<TDbObject>, IReadOnlyCollection<TJuncedId>, IQueryable<TJunction>> filter,
            Expression<Func<TJunction, TResult>> select,
            Expression<Func<TJunction, TResultId>> getResultId,
            Expression<Func<TJunction, TJuncedId>> getJuncedId,
            IReadOnlyCollection<TJuncedId> values,
            int batchSize)
        {
            int batchCount = values.Count / batchSize + 1;

            var results = new List<IEnumerable<IdResultPair>>(capacity: batchCount);
            var junctions = new List<IEnumerable<JunctionIds>>(capacity: batchCount);

            for (int offset = 0; offset < values.Count; offset += batchSize)
            {
                TJuncedId[] batchValues = values.Skip(offset).Take(batchSize).ToArray();

                IQueryable<TJunction> batchQuery = filter(query, batchValues);

                var resultQuery = GetSelectResultQuery(batchQuery, select, getResultId).Distinct().ToFuture();
                var junctionQuery = GetJuncedIdsQuery(batchQuery, getResultId, getJuncedId).Distinct().ToFuture();

                var resultItems = resultQuery.GetEnumerable();
                var junctionItems = junctionQuery.GetEnumerable();

                results.Add(resultItems);
                junctions.Add(junctionItems);
            }

            return new JuncedData
            {
                Junctions = junctions.SelectMany(item => item),
                Results = results.SelectMany(item => item).ToArray(),
            };
        }

        public ILookup<TJuncedId, TResult> ToLookup(JuncedData data)
        {
            var dic = new Dictionary<TResultId, TResult>(capacity: data.Results.Count);
            foreach (var item in data.Results)
            {
                if (!dic.ContainsKey(item.Id))
                {
                    dic.Add(item.Id, item.Item);
                }
            }

            return data.Junctions
                .Select(item =>
                {
                    if (!dic.TryGetValue(item.ResultId, out var result))
                    {
                        result = default;
                    }
                    return new { item.JunctionId, Result = result };
                })
                .Where(item => item != null)
                .ToLookup(item => item.JunctionId, item => item.Result);
        }

        internal struct IdResultPair
        {
            public IdResultPair(TResultId id, TResult item)
            {
                Id = id;
                Item = item;
            }

            public TResultId Id { get; }

            public TResult Item { get; }
        }

        internal struct JunctionIds
        {
            public JunctionIds(TResultId resultId, TJuncedId junctionId)
            {
                ResultId = resultId;
                JunctionId = junctionId;
            }

            public TResultId ResultId { get; }

            public TJuncedId JunctionId { get; }
        }

        internal struct JuncedData
        {
            public IReadOnlyCollection<IdResultPair> Results { get; set; }

            public IEnumerable<JunctionIds> Junctions { get; set; }
        }
    }
}
