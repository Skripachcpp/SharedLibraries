using System;

namespace SL.Elasticsearch.EsContextLiteInside
{
    /// <summary>
    /// Автопростановка даты создания записи
    /// </summary>
    public interface ICreateDate
    {
        DateTime? CreateDate { get; set; }
    }

    /// <summary>
    /// Если идентификатор меньше нуля, то он будет автоматически создан и присвое объекту
    /// </summary>
    public interface IAutoId
    {
        int Id { get; set; }
    }
}