using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeamlessRepeater.Helper
{
    public static class ListExtensions
    {
        public static void Swap<T>(this List<T> list, int indexA, int indexB)
        {
            var item = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = item;
        }
    }
}
