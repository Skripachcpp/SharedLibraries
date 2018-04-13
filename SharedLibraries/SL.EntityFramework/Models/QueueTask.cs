using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FP.DataLayer.EfDb
{
    /// <summary>
    /// Очередь задач
    /// </summary>
    public class QueueTask
    {
        /// <summary>
        /// Код очереди
        /// </summary>
        [Key, Column(Order = 1)]
        [MaxLength(128)]
        public string QueueCode { get; set; }

        [Key, Column(Order = 2)]
        public Guid Key { get; set; }

        /// <summary>
        /// Параметры
        /// </summary>
        public string Prms { get; set; }

        //public QState State { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? CompliteDate { get; set; }

        public string Message { get; set; }
    }

    //public enum QState
    //{
    //    New = 1,
    //    Complite = 100,
    //}
}