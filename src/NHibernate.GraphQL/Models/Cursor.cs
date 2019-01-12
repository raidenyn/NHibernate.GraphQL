namespace NHibernate.GraphQL
{
    /// <summary>
    /// Representation of cursor value
    /// </summary>
    public struct Cursor
    {
        private readonly string _value;

        /// <summary>
        /// Creating cursor value from a string
        /// </summary>
        /// <param name="value"></param>
        public Cursor(string value)
        {
            _value = value;
        }

        /// <summary>
        /// Implicit converting the string to a cursor
        /// </summary>
        /// <param name="cursor"></param>
        public static implicit operator string(Cursor cursor)
        {
            return cursor._value;
        }

        /// <summary>
        /// Implicit converting the cursor to a string
        /// </summary>
        public static implicit operator Cursor(string value)
        {
            return new Cursor(value);
        }
    }
}
