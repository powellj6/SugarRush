using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugarRush
{
    public static class Extensions
    {
        public static string Replace(this string originalString, string snippetToReplace, string replaceValue)
        {
            var startIndex = originalString.IndexOf(snippetToReplace);
            var removeCount = snippetToReplace.Length;

            return originalString.Remove(startIndex, removeCount).Insert(startIndex, replaceValue);
        }
    }
}
