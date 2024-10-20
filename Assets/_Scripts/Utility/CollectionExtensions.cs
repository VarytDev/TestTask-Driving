using System.Collections;

public static class CollectionExtensions
{
    public static bool IsInRange(this ICollection collection, int index)
    {
        return index >= 0 && index < collection.Count;
    }
}
