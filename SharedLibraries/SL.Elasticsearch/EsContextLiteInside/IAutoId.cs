using System;

namespace SL.Elasticsearch.EsContextLiteInside
{
    /// <summary>
    /// ��������������� ���� �������� ������
    /// </summary>
    public interface ICreateDate
    {
        DateTime? CreateDate { get; set; }
    }

    /// <summary>
    /// ���� ������������� ������ ����, �� �� ����� ������������� ������ � ������� �������
    /// </summary>
    public interface IAutoId
    {
        int Id { get; set; }
    }
}