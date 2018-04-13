using System;
using AutoMapper;
using WorkingTools.Classes;

namespace WorkingTools.Extensions.Map
{
    public static class MapperExtension
    {
        private static readonly CacheFixedGetter<MapKey, IMapper> _cache;

        static MapperExtension()
        {
            _cache = new CacheFixedGetter<MapKey, IMapper>(a =>
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap(a.SourceType, a.TargetType);
                    cfg.CreateMap<string, bool>().ConvertUsing((src) => !string.IsNullOrEmpty(src));
                    cfg.CreateMap<Guid, string>().ConvertUsing((src) => src.ToString("N"));
                });
                var mapper = config.CreateMapper();
                return mapper;
            });
        }

        public static TTarget Map<TTarget>(this object source)
            where TTarget : class
        {
            if (source == null) return default(TTarget);
            var mapper = _cache.Get(new MapKey() { SourceType = source.GetType(), TargetType = typeof(TTarget) });
            return mapper.Map<TTarget>(source);
        }

        public static TTarget Map<TSource, TTarget>(this TSource source, TTarget target)
             where TTarget : class
             where TSource : class
        {
            var mapper = _cache.Get(new MapKey() { SourceType = typeof(TSource), TargetType = typeof(TTarget) });
            return mapper.Map(source, target);
        }


        public static TSource Copy<TSource>(this TSource source)
            where TSource : class
        {
            return Map(source, default(TSource));
        }
    }


    public class MapKey
    {
        public Type SourceType { get; set; }
        public Type TargetType { get; set; }

        public override bool Equals(object obj) => !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((MapKey)obj));
        protected bool Equals(MapKey other) => SourceType == other.SourceType && TargetType == other.TargetType;
        public override int GetHashCode() { unchecked { return ((SourceType != null ? SourceType.GetHashCode() : 0) * 397) ^ (TargetType != null ? TargetType.GetHashCode() : 0); } }
        public static bool operator ==(MapKey left, MapKey right) { return Equals(left, right); }
        public static bool operator !=(MapKey left, MapKey right) { return !Equals(left, right); }
    }
}