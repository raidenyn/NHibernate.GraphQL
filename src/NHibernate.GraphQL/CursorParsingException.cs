using System;
using System.Runtime.Serialization;

namespace NHibernate.GraphQL
{
    /// <summary>
    /// The exception is rose on curosr value parsing fault
    /// </summary>
    [Serializable]
    public class CursorParsingException : Exception
    {
        /// <summary>
        /// Cursor value that cannot be parsed
        /// </summary>
        public Cursor Cursor { get; }

        /// <summary>
        /// Type to represent curor value
        /// </summary>
        public System.Type TargetType { get; }

        /// <summary>
        /// Create new exception instances
        /// </summary>
        /// <param name="Cursor">Cursor that rose the exception</param>
        /// <param name="type">Target type</param>
        public CursorParsingException(Cursor Cursor, System.Type type)
            : base ($"Cursor value {Cursor} cannot be parsed to type {type.Name}")
        { }

        /// <summary>
        /// Create new exception instances
        /// </summary>
        /// <param name="Cursor">Cursor that rose the exception</param>
        /// <param name="type">Target type</param>
        /// <param name="innerException">Reason of the parsing issue</param>
        public CursorParsingException(Cursor Cursor, System.Type type, Exception innerException)
            : base($"Cursor value {Cursor} cannot be parsed to type {type.Name}", innerException)
        { }

        /// <summary>
        /// Create new exception instances
        /// </summary>
        /// <param name="Cursor">Cursor that rose the exception</param>
        public CursorParsingException(Cursor Cursor)
            : base($"Cursor value {Cursor} cannot be parsed")
        { }

        /// <summary>
        /// Create new exception instances
        /// </summary>
        /// <param name="Cursor">Cursor that rose the exception</param>
        /// <param name="innerException">Reason of the parsing issue</param>
        public CursorParsingException(Cursor Cursor, Exception innerException)
            : base($"Cursor value {Cursor} cannot be parsed", innerException)
        { }

        /// <summary>
        /// Create new exception instances
        /// </summary>
        protected CursorParsingException(string message) : base(message)
        {
        }

        /// <summary>
        /// Create new exception instances
        /// </summary>
        protected CursorParsingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Create new exception instances
        /// </summary>
        protected CursorParsingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
