using System;
using System.Text;

namespace WorkingTools.Classes
{
    public static class StringToIntEncoding
    {
        /// <summary>
        /// Кодирует строку int32
        /// </summary>
        /// <param name="value"></param>
        /// <returns>ВНИМАНИЕ!! если строка слишком длинная - часть данных будет утеряна</returns>
        public static int EncodeInt32(string value)
        {
            byte[] bytesValue = Encoding.UTF8.GetBytes(value);
            var bytesValuePrepare = new byte[4];
            for (int i = 0; i < bytesValuePrepare.Length && i < bytesValue.Length; i++)
                bytesValuePrepare[i] = bytesValue[i];

            var intValue = BitConverter.ToInt32(bytesValuePrepare, 0);

            return intValue;
        }

        /// <summary>
        /// Декодирует строку из int 32
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DecodeInt32(int value)
        {
            var bytesValue = BitConverter.GetBytes(value);
            var strValue = Encoding.UTF8.GetString(bytesValue);
            return strValue;
        }
    }
}