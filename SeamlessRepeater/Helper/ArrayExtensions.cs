using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeamlessRepeater.Helper
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Appends the elements of <paramref name="endArray"/> to the end of <paramref name="startArray"/>
        /// </summary>
        public static T[] Append<T>(this T[] startArray, T[] endArray)
        {
            if (startArray == null) startArray = new T[0];

            if (endArray == null) endArray = new T[0];

            var appendedArray = new T[startArray.Length + endArray.Length];
            startArray.CopyTo(appendedArray, 0);
            endArray.CopyTo(appendedArray, startArray.Length);
            return appendedArray;
        }
    }
}
