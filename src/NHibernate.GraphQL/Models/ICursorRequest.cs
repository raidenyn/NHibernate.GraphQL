namespace NHibernate.GraphQL
{
    /// <summary>
    /// Data for requesting unidirectional cursor
    /// </summary>
    public interface ICursorRequest
    {
        /// <summary>
        /// Count of first elements just after the cursor <see cref="After"/>
        /// </summary>
        int? First { get; }

        /// <summary>
        /// Element should be represented after the cursor value
        /// </summary>
        Cursor After { get; }
    }
}
