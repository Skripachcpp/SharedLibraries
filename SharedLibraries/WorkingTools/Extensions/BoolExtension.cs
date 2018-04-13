namespace WorkingTools.Extensions
{
    public static class BoolExtension
    {
        /// <summary>
        /// Привести к int
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToInt(this bool value)
        {
            return value ? 1 : 0;
        }

        /// <summary>
        /// Привести к int
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int? AsInt(this bool? value)
        {
            return value == null
                ? (int?)null
                : (bool)value ? 1 : 0;
        }
    }
}
