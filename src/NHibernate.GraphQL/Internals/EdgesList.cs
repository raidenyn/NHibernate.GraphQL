using System.Collections.Generic;

namespace NHibernate.GraphQL
{
    internal struct EdgesList<TResult>
    {
        public List<Edge<TResult>> Edges;
        public bool HasNext;

        public EdgesList(List<Edge<TResult>> edges, bool hasNext)
        {
            Edges = edges;
            HasNext = hasNext;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EdgesList<TResult>))
            {
                return false;
            }

            var other = (EdgesList<TResult>)obj;
            return EqualityComparer<List<Edge<TResult>>>.Default.Equals(Edges, other.Edges) &&
                   HasNext == other.HasNext;
        }

        public override int GetHashCode()
        {
            var hashCode = 69731839;
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Edge<TResult>>>.Default.GetHashCode(Edges);
            hashCode = hashCode * -1521134295 + HasNext.GetHashCode();
            return hashCode;
        }

        public void Deconstruct(out List<Edge<TResult>> edges, out bool hasNext)
        {
            edges = Edges;
            hasNext = HasNext;
        }
    }
}
