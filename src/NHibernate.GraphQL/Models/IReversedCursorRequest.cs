namespace NHibernate.GraphQL
{
    /// <summary>
    /// Reversed cursor request
    /// </summary>
    public interface IReversedCursorRequest
    {
        /// <summary>
        /// Count of the elements just before the cursor <see cref="Before"/>
        /// </summary>
        int? Last { get; }

        /// <summary>
        /// Element should be represented before the cursor value
        /// </summary>
        Cursor Before { get; }
    }
}