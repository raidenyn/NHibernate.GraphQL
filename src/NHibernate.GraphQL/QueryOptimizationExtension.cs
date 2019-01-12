using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NHibernate.GraphQL
{
    /// <summary>
    /// Extension for nHibernate query optimization
    /// </summary>
    public static class QueryOptimizationExtension
    {
        /// <summary>
        /// Remove all not requested fields from SQL query
        /// </summary>
        /// <typeparam name="TResult">Type of projection type</typeparam>
        /// <param name="query">NHibernate query</param>
        /// <param name="keepMembers">List of members to select</param>
        /// <returns>Optimized NHibernate query</returns>
        public static IQueryable<TResult> OptimizeQuery<TResult>(this IQueryable<TResult> query, IEnumerable<MemberInfo> keepMembers)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (keepMembers == null) throw new ArgumentNullException(nameof(keepMembers));

            Expression expression = new MemberRemoverModifier(keepMembers).RemoveFields(query.Expression);

            return query.Provider.CreateQuery<TResult>(expression);
        }

        /// <summary>
        /// Remove all not requested fields from SQL query
        /// </summary>
        /// <typeparam name="TResult">Type of projection type</typeparam>
        /// <param name="query">nHibernate query</param>
        /// <param name="keepMembers">List of members names to select</param>
        /// <returns>Optimized NHibernate query</returns>
        public static IQueryable<TResult> OptimizeQuery<TResult>(this IQueryable<TResult> query, IReadOnlyCollection<string> keepMembers)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (keepMembers == null) throw new ArgumentNullException(nameof(keepMembers));

            System.Type type = typeof(TResult);
            IEnumerable<MemberInfo> members = keepMembers.SelectMany(memberName => type.GetMember(memberName));

            return query.OptimizeQuery(members);
        }
    }
}
