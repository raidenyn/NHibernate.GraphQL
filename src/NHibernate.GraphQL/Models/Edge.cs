namespace NHibernate.GraphQL
{
    /// <summary>
    /// Target element structure with Cursor value
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public struct Edge<TNode>
    {
        /// <summary>
        /// <page>Cursor of the element.</page>
        /// <page>It can be used in <see cref="ICursorRequest"/> or <see cref="IReversedCursorRequest"/></page>
        /// </summary>
        public Cursor Cursor { get; set; }

        /// <summary>
        /// Structure of target element
        /// </summary>
        public TNode Node { get; set; }
    }
}
