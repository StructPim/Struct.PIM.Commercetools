namespace Struct.PIM.Commercetools.Helpers;

internal static class EnumerableExtension
{
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> enumerator, int size)
    {
        var enumerable = enumerator as T[] ?? enumerator.ToArray();
        var length = enumerable.Count();
        var pos = 0;
        do
        {
            yield return enumerable.Skip(pos).Take(size);
            pos = pos + size;
        } while (pos < length);
    }
}