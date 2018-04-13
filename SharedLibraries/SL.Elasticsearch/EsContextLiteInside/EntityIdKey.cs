using Nest;
using System;

namespace SL.Elasticsearch.EsContextLiteInside
{
    /// <summary>
    /// Ключ для индекса и типа
    /// </summary>
    public class EntityIdKey
    {
        public EntityIdKey(string index, Type type)
        {
            Index = index;
            Type = type;
        }

        public string Index { get; }
        public Type Type { get; }

        public override bool Equals(object obj) => !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || obj.GetType() == this.GetType() && Equals((EntityIdKey) obj));
        protected bool Equals(EntityIdKey other) => string.Equals(Index, other.Index) && Type == other.Type;

        public override int GetHashCode() { unchecked { return ((Index?.GetHashCode() ?? 0) * 397) ^ (Type?.GetHashCode() ?? 0); } }
    }
}