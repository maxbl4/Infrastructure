using System;
using System.IO;

namespace maxbl4.Infrastructure
{
    public static class RollingFile
    {
        private static string Find(string originalFilename, int numberOfDigits, bool returnExisting)
        {
            if (!File.Exists(originalFilename)) return originalFilename;
            var dir = Path.GetDirectoryName(originalFilename) ?? "";
            var ext = Path.GetExtension(originalFilename) ?? "";
            var name = Path.GetFileNameWithoutExtension(originalFilename) ?? "";
            var prevName = originalFilename;
            for (var n = numberOfDigits; n < 10; n++)
            {
                for (var i = 1; i < Math.Pow(10, n); i++)
                {
                    var num = i.ToString(new string('0', n));
                    var nextName = Path.Combine(dir, $"{name}_{num}{ext}");
                    if (!File.Exists(nextName))
                        return returnExisting ? prevName : nextName;
                    prevName = nextName;
                }
            }

            throw new IndexOutOfRangeException("Could not file free file name"); 
        }

        /// <summary>
        /// Returns next rolling file name.
        /// E.g my.log > my_001.log; my_005.log > my_006.log
        /// </summary>
        /// <param name="originalFilename"></param>
        /// <param name="numberOfDigits"></param>
        /// <returns></returns>
        public static string GetNext(string originalFilename, int numberOfDigits = 3)
        {
            return Find(originalFilename, numberOfDigits, false);
        }

        public static string GetCurrent(string originalFilename, int numberOfDigits = 3)
        {
            return Find(originalFilename, numberOfDigits, true);
        }
    }
}