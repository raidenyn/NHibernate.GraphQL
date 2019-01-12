using System.Collections.Generic;

namespace NHibernate.GraphQL
{
    public interface ICursorRequest
    {
        int? First { get; }

        string After { get; }
    }

    public interface IReversedCursorRequest
    {
        int? Last { get; }

        string Before { get; }
    }

    public struct Connection<TNode>
    {
        public List<Edge<TNode>> Edges { get; set; }

        public PageInfo PageInfo { get; set; }

        public int TotalCount { get; set; }
    }

    public struct Edge<TNode>
    {
        public Cursor Cursor { get; set; }

        public TNode Node { get; set; }
    }

    public struct PageInfo
    {
        public bool HasNextPage { get; set; }

        public bool HasPreviousPage { get; set; }

        public Cursor StartCursor { get; set; }

        public Cursor EndCursor { get; set; }
    }

    public struct Cursor
    {
        private readonly string _value;

        public Cursor(string value)
        {
            _value = value;
        }

        public static implicit operator string(Cursor cursor)
        {
            return cursor._value;
        }

        public static implicit operator Cursor(string value)
        {
            return new Cursor(value);
        }
    }
}
