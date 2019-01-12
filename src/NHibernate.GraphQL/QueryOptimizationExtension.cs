using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NHibernate.GraphQL
{
    public static class QueryOptimizationExtension
    {
        public static IQueryable<T> OptimizeQuery<T>(this IQueryable<T> query, IEnumerable<MemberInfo> keepMembers)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (keepMembers == null) throw new ArgumentNullException(nameof(keepMembers));

            Expression expression = new MemberRemoverModifier(keepMembers).RemoveFields(query.Expression);

            return query.Provider.CreateQuery<T>(expression);
        }

        public static IQueryable<T> OptimizeQuery<T>(this IQueryable<T> query, IReadOnlyCollection<string> keepMembers)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (keepMembers == null) throw new ArgumentNullException(nameof(keepMembers));

            System.Type type = typeof(T);
            IEnumerable<MemberInfo> members = keepMembers.SelectMany(memberName => type.GetMember(memberName));

            return query.OptimizeQuery(members);
        }
    }
}
