namespace WorkingTools.Extensions
{
    public static class IntExtension
    {
        /// <summary>
        /// Привести к string, возвращает null если выполнить преобразование не удалось
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AsString(this int? value)
        {
            return value.Get(v => v.ToString());
        }
    }
}
