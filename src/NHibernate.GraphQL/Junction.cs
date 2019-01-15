using System;

namespace NHibernate.GraphQL
{
    /// <summary>
    /// Represents air of id for junctioned objects
    /// </summary>
    /// <typeparam name="TResultId">Resulted object key type</typeparam>
    /// <typeparam name="TJunctionId">Relative object key type</typeparam>
    public struct Junction<TResultId, TJunctionId> : IEquatable<Junction<TResultId, TJunctionId>>
    {
        /// <summary>
        /// Create new instance of the key pair
        /// </summary>
        /// <param name="resultId">Resulted object key</param>
        /// <param name="junctionId">Relative object key</param>
        public Junction(TResultId resultId, TJunctionId junctionId)
        {
            if (resultId == null) throw new ArgumentNullException(nameof(resultId));
            if (junctionId == null) throw new ArgumentNullException(nameof(junctionId));

            ResultId = resultId;
            JunctionId = junctionId;
        }

        /// <summary>
        /// Resulted object key
        /// </summary>
        public TResultId ResultId { get; }

        /// <summary>
        /// Relative object key
        /// </summary>
        public TJunctionId JunctionId { get; }

        /// <summary>
        /// Checks object equality
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Junction<TResultId, TJunctionId> junction)
            {
                return Equals(junction);
            }

            return false;
        }

        /// <summary>
        /// Returns junction hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ResultId.GetHashCode() * 13 ^ JunctionId.GetHashCode();
        }

        /// <summary>
        /// ==
        /// </summary>
        public static bool operator ==(Junction<TResultId, TJunctionId> left, Junction<TResultId, TJunctionId> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// !=
        /// </summary>
        public static bool operator !=(Junction<TResultId, TJunctionId> left, Junction<TResultId, TJunctionId> right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Checks object equality
        /// </summary>
        public bool Equals(Junction<TResultId, TJunctionId> other)
        {
            return ResultId.Equals(other.ResultId) && JunctionId.Equals(other.JunctionId);
        }
    }
}
