using System.Collections.Generic;

namespace WorkingTools.Rndm
{
    public static class RandomWt
    {
        private static readonly System.Random _random = new System.Random();

        public static T GetAny<T>(params T[] vs)
        {
            var count = vs.Length;
            var index = _random.Next(count);
            return vs[index];
        }

        public static T GetAny<T>(IList<T> vs)
        {
            var count = vs.Count;
            var index = _random.Next(count);
            return vs[index];
        }
    }
}
