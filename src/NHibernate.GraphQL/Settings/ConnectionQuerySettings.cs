namespace NHibernate.GraphQL
{
    /// <summary>
    /// Settings of generating GraphQL Connection instance
    /// </summary>
    public sealed class ConnectionQuerySettings : IConnectionQuerySettings
    {
        /// <summary>
        /// Default value for the settings
        /// </summary>
        public static IConnectionQuerySettings Default { get; set; } = new ConnectionQuerySettings();

        /// <summary>
        /// Cursor value formatter
        /// </summary>
        public ICursorFormatter CursorFormatter { get; set; } = new CursorJsonFormatter();
    }
}
