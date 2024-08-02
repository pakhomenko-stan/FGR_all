namespace FGR.Common.Extensions
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (collection?.Any() ?? false)
                foreach (var item in collection) action(item);
        }
    }
}
