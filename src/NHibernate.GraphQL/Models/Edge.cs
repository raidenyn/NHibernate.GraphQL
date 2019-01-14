namespace NHibernate.GraphQL
{
    /// <summary>
    /// Target element structure with Cursor value
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public struct Edge<TNode>
    {
        /// <summary>
        /// <para>Cursor of the element.</para>
        /// <para>It can be used in <see cref="ICursorRequest"/> or <see cref="IReversedCursorRequest"/></para>
        /// </summary>
        public Cursor Cursor { get; set; }

        /// <summary>
        /// Structure of target element
        /// </summary>
        public TNode Node { get; set; }
    }
}
