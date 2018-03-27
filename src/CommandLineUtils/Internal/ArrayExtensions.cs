using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace McMaster.Extensions.CommandLineUtils.Internal
{
    internal static class ArrayExtensions
    {
        public static T[] Merge<T>(this T[] m1, T[] m2)
        {
            var newArray = new T[m1.Length + m2.Length];
            m1.CopyTo(newArray,0);
            m2.CopyTo(newArray,m1.Length);
            return newArray;
        }
        
    }
}
