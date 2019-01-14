namespace NHibernate.GraphQL
{
    /// <summary>
    /// Definitions for sorting directions
    /// </summary>
    public static class SortBy
    {
        /// <summary>
        /// Sort the field by ascending (default)
        /// </summary>
        public static TItem Ascending<TItem>(TItem item)
        {
            return item;
        }

        /// <summary>
        /// Sort the field by descending
        /// </summary>
        public static TItem Descending<TItem>(TItem item)
        {
            return item;
        }
    }
}
