namespace NHibernate.GraphQL
{
    /// <summary>
    /// Settings for GraphQL Connection creation process
    /// </summary>
    public interface IConnectionQuerySettings
    {
        /// <summary>
        /// Cursor formatter
        /// </summary>
        ICursorFormatter CursorFormatter { get; }
    }
}