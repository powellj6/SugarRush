using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugarRush
{
    public static class Extensions
    {
        public static string Replace(this string originalString, string stringToReplace, string replaceValue)
        {
            var startIndex = originalString.IndexOf(stringToReplace);
            var removeCount = stringToReplace.Length;

            return originalString.Remove(startIndex, removeCount).Insert(startIndex, replaceValue);
        }
    }
}
