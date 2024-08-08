using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authorization.SSO.Extensions
{
    public static class AsyncEnumerableExtensions
    {
        public static Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
        {
            return source == null ? throw new ArgumentNullException(nameof(source)) : ExecuteAsync();

            async Task<List<T>> ExecuteAsync()
            {
                var list = new List<T>();

                await foreach (var element in source)
                {
                    list.Add(element);
                }

                return list;
            }
        }
    }
}
