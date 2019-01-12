namespace NHibernate.GraphQL
{
    /// <summary>
    /// Represent information about current page in a connection
    /// </summary>
    public struct PageInfo
    {
        /// <summary>
        /// Does the connection have next page
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Does the connection have previous page
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// First cursor value in the page
        /// </summary>
        public Cursor StartCursor { get; set; }

        /// <summary>
        /// Last cursor value in the page
        /// </summary>
        public Cursor EndCursor { get; set; }
    }
}
