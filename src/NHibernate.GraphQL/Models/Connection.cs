using System.Collections.Generic;
using System.Linq;

namespace NHibernate.GraphQL
{
    /// <summary>
    /// Part of elements set restricted by user request params
    /// </summary>
    /// <typeparam name="TNode">Type of target elements</typeparam>
    public struct Connection<TNode>
    {
        /// <summary>
        /// Items and cursor values 
        /// </summary>
        public IReadOnlyList<Edge<TNode>> Edges { get; set; }

        /// <summary>
        /// Enumeration of nodes from <see cref="Edges"/>
        /// </summary>
        public IEnumerable<TNode> Items => Edges.Select(edge => edge.Node);

        /// <summary>
        /// Additional information about current page
        /// </summary>
        public PageInfo PageInfo { get; set; }

        /// <summary>
        /// Total count of elements in all pages
        /// </summary>
        public int TotalCount { get; set; }
    }
}
