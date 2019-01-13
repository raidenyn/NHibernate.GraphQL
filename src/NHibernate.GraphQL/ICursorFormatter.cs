namespace NHibernate.GraphQL
{
    /// <summary>
    /// Formatter for cursor values
    /// </summary>
    public interface ICursorFormatter
    {
        /// <summary>
        /// Parse passed string into a object instance.
        /// <page>Should be formattable in <see cref="Format"/></page>
        /// </summary>
        /// <typeparam name="TOrder">Object instance type</typeparam>
        /// <param name="cursor">Cursor string representation</param>
        /// <returns>Parsed object instance</returns>
        TOrder ParseAs<TOrder>(Cursor cursor);

        /// <summary>
        /// Format passed object to string representation.
        /// <page>Should be parsable in <see cref="ParseAs"/></page>
        /// </summary>
        /// <typeparam name="TOrder">Object instance type</typeparam>
        /// <param name="order">Object instance</param>
        /// <returns>Cursor string representation</returns>
        Cursor Format<TOrder>(TOrder order);

        /// <summary>
        /// Check that the cursor has any value
        /// </summary>
        /// <param name="cursor">Cursor string representation</param>
        /// <returns>True if the cursor has a value</returns>
        bool HasValue(Cursor cursor);
    }
}
