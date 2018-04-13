using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SL.EntityFramework.Models;
using WorkingTools.Classes;
using WorkingTools.Extensions;

namespace SL.EntityFramework
{
    public static class EfContextLiteExtensions
    {
        public static async Task<string> GetFromCacheAsync(this EfContextLite ef, DbSet<Cache> dbSetCache, string cacheName, Func<JsonKey, Task<string>> getter, params object[] args)
        {
            var key = new JsonKey(args);
            var dbValue = await dbSetCache.FirstOrDefaultAsync(a => a.Name == cacheName && a.Key == key.Key);
            if (dbValue != null)
            {
                return dbValue.Value;
            }
            else
            {
                var value = await getter(key);
                dbSetCache.AddOrUpdate(new Cache() { Name = cacheName, Key = key.Key, Value = value, CreateDate = DateTimeOffset.UtcNow });
                ef.SaveChanges();
                return value;
            }
        }

        public static string GetFromCache(this EfContextLite ef, DbSet<Cache> dbSetCache, string cacheName, Func<JsonKey, string> getter, params object[] args)
        {
            var key = new JsonKey(args);
            var dbValue = dbSetCache.FirstOrDefault(a => a.Name == cacheName && a.Key == key.Key);
            if (dbValue != null)
            {
                return dbValue.Value;
            }
            else
            {
                var value = getter(key);
                dbSetCache.AddOrUpdate(new Cache() { Name = cacheName, Key = key.Key, Value = value, CreateDate = DateTimeOffset.UtcNow });
                ef.SaveChanges();
                return value;
            }
        }
        
        public static IEnumerable<T> Merge<T>(this EfContextLite ef, T entity, params Expression<Func<T, object>>[] mgrBy) => Merge(ef, ary.Union(entity), mgrBy);
        public static IEnumerable<T> Merge<T>(this EfContextLite ef, IEnumerable<T> entities, params Expression<Func<T, object>>[] mgrBy) => ef.DbConnection.Merge(entities, mgrBy);

        public static IEnumerable<T> InsertIfNotExist<T>(this EfContextLite ef, T entity, params Expression<Func<T, object>>[] mgrBy) => ef.DbConnection.Merge(ary.Union(entity), mgrBy, update: false);
        public static IEnumerable<T> InsertIfNotExist<T>(this EfContextLite ef, IEnumerable<T> entities, params Expression<Func<T, object>>[] mgrBy) => ef.DbConnection.Merge(entities, mgrBy, update: false);
    }
}