using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SL.EntityFramework.Models
{
    public class Cache
    {
        //TODO: тут создается кластерный индекс а у него ограничение на 900 bytes .. поправить
        //TODO: Warning! The maximum key length for a clustered index is 900 bytes. The index 'PK_dbo.Cache' has maximum length of 4336 bytes. For some combination of large values, the insert/update operation will fail.

        /// <summary>
        /// Наименование
        /// </summary>
        /// <remarks>используется для группировки запросов одного кэша</remarks>
        [MaxLength(120)]
        [Key, Column(Order = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Ключ
        /// </summary>
        [MaxLength(2048)]
        [Key, Column(Order = 2)]
        public string Key { get; set; }

        /// <summary>
        /// Значение
        /// </summary>
        public string Value { get; set; }

        public DateTimeOffset CreateDate { get; set; }
    }
}