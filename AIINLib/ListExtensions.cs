using static System.Int32;

namespace AIINLib;

public static class ListExtensions
{
    /// <summary>
    /// Reverses order of elements between indexes in List. 
    /// </summary>
    /// <exception cref="ArgumentNullException">If list is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If index is out of range of list.</exception>
    public static void ExtensionReverse<T>(this List<T> list, int index1, int index2)
    {
        if (list == null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        if (index1 < 0 || index2 < 0 || index1 >= list.Count || index2 >= list.Count)
        {
            throw new ArgumentOutOfRangeException("Index out of range.");
        }
        
        int maxIndex = Max(index1, index2);
        int minIndex = Min(index1, index2);
        int numberOfElementsToReverse = maxIndex - minIndex + 1;
        
        list.Reverse(minIndex, numberOfElementsToReverse);
    }
}