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
        public static IQueryable<TResult> OptimizeQuery<TResult>(this IQueryable<TResult> query, IReadOnlyCollection<MemberInfo> keepMembers)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (keepMembers == null) throw new ArgumentNullException(nameof(keepMembers));

            if (keepMembers.Count == 0)
            {
                throw new ArgumentException("Member list should containes at last one field", nameof(keepMembers));
            }

            var fieldRemover = new MemberRemoverVisitor(keepMembers, query.Expression);

            if (fieldRemover.UnusedMembers.Count > 0)
            {
                throw new ArgumentException("Member list contents unexiting in members the query. Check passed memebers.");
            }

            return query.Provider.CreateQuery<TResult>(fieldRemover.ClearedExpression);
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

            if (keepMembers.Count == 0)
            {
                throw new ArgumentException("Selected member list should containes at last one field", nameof(keepMembers));
            }

            System.Type type = typeof(TResult);
            MemberInfo[] members = keepMembers.SelectMany(memberName => type.GetMember(memberName)).ToArray();

            return query.OptimizeQuery(members);
        }

        /// <summary>
        /// Remove all not requested fields from select clause
        /// </summary>
        /// <typeparam name="TDbObject">Type of database mapped object</typeparam>
        /// <typeparam name="TResult">Type of projection</typeparam>
        /// <param name="select">Expression with select clause</param>
        /// <param name="keepMembers">List of members names to select</param>
        /// <returns>Optimized NHibernate query</returns>
        public static Expression<Func<TDbObject, TResult>> OptimizeSelect<TDbObject, TResult>(
            this Expression<Func<TDbObject, TResult>> select,
            IReadOnlyCollection<string> keepMembers)
        {
            if (select == null) throw new ArgumentNullException(nameof(select));
            if (keepMembers == null) throw new ArgumentNullException(nameof(keepMembers));

            if (keepMembers.Count == 0)
            {
                throw new ArgumentException("Selected member list should containes at last one field", nameof(keepMembers));
            }

            System.Type type = typeof(TResult);
            MemberInfo[] members = keepMembers.SelectMany(memberName => type.GetMember(memberName)).ToArray();

            return OptimizeSelect(select, members);
        }

        /// <summary>
        /// Remove all not requested fields from select clause
        /// </summary>
        /// <typeparam name="TDbObject">Type of database mapped object</typeparam>
        /// <typeparam name="TResult">Type of projection</typeparam>
        /// <param name="select">Expression with select clause</param>
        /// <param name="keepMembers">List of members to select</param>
        /// <returns>Optimized NHibernate query</returns>
        public static Expression<Func<TDbObject, TResult>> OptimizeSelect<TDbObject, TResult>(
            this Expression<Func<TDbObject, TResult>> select,
            IReadOnlyCollection<MemberInfo> keepMembers)
        {
            if (select == null) throw new ArgumentNullException(nameof(select));
            if (keepMembers == null) throw new ArgumentNullException(nameof(keepMembers));

            if (keepMembers.Count == 0)
            {
                throw new ArgumentException("Selected member list should containes at last one field", nameof(keepMembers));
            }

            var fieldRemover = new MemberRemoverVisitor(keepMembers, select);

            if (fieldRemover.UnusedMembers.Count > 0)
            {
                throw new ArgumentException("Member list contents unexiting in members the query. Check passed memebers.");
            }

            return (Expression<Func<TDbObject, TResult>>) fieldRemover.ClearedExpression;
        }
    }
}
