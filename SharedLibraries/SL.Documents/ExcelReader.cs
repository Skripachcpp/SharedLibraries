using System;
using System.Data;
using System.IO;
using Excel;

namespace SL.Documents
{
    public static class ExcelReader
    {
        public static DataTable GetTable(string path, int tableIndex = 0, int skipRows = 0)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (tableIndex < 0) throw new ArgumentOutOfRangeException(nameof(tableIndex), "значение не может быть отрицательным");

            //что то этот способ не работает
            //var reader = GetReader(path);
            //if (reader == null) throw new Exception("не удалось получить данные");

            //for (int i = 0; i < tableIndex; i++)
            //{
            //    //возможно есть другой способ получить конкретную таблицу
            //    if (!reader.NextResult())
            //        return null;
            //}

            //var table = new DataTable();
            //table.Load(reader);

            var dataSet = GetDataSet(path);
            if (dataSet == null || tableIndex >= dataSet.Tables.Count)
                return null;

            var table = dataSet.Tables[tableIndex];

            if (skipRows > 0)
                for (int i = 0; i < skipRows; i++)
                    table.Rows.RemoveAt(0);

            return table;
        }

        public static DataSet GetDataSet(string path)
        {
            var reader = GetExcelReader(path);
            return reader?.AsDataSet(true);
        }

        public static IDataReader GetReader(string path)
        {
            var excelReader = GetExcelReader(path);
            return excelReader;
        }

        private static IExcelDataReader GetExcelReader(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) return null;

            var fileName = Path.GetFileName(path);
            if (fileName.IndexOf("~", StringComparison.Ordinal) != -1)
                return null;

            var ext = Path.GetExtension(path);
            var stream = new FileStream(path, FileMode.Open);
            IExcelDataReader excelReader;
            if (ext == ".xls") excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            else excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

            return excelReader;
        }
    }
}
