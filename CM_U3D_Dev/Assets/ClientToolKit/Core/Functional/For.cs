using System;
using System.Collections.Generic;

namespace MTool.Core.Functional
{
    public static class For
    {
        public static void ForCall<T>(this IList<T> elements, Action<T, int> call)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                call(element, i);
            }
        }

        public static void ForeachCall<T>(this IEnumerable<T> elements, Action<T> call)
        {
            foreach (var element in elements)
            {
                call(element);
            }
        }

        public static void ForeachCall<T, R, L>(this IEnumerable<T> elements, Action<T, R, L> call, R p1, L p2)
        {
            foreach (var element in elements)
            {
                call(element, p1, p2);
            }
        }

        public static void ForeachCall<T, R, L, M>(this IEnumerable<T> elements, Action<T, R, L, M> call, R p1, L p2, M p3)
        {
            foreach (var element in elements)
            {
                call(element, p1, p2, p3);
            }
        }

        public static IEnumerable<R> IteratorCall<T, R>(this IEnumerable<T> elements, Func<T, R> call)
        {
            foreach (var element in elements)
            {
                yield return call(element);
            }
        }
    }
}
