using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF
{
    public static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int size)
        {
            while (source.Any())
            {
                yield return source.Take(size);
                source = source.Skip(size);
            }
        }
    }
}
