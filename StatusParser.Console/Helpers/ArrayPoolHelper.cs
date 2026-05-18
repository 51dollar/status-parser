using System.Buffers;

namespace StatusParser.Console.Helpers;

public static class ArrayPoolHelper
{
    public static object[] RentObjectArray(int minimumLength)
    {
        return ArrayPool<object>.Shared.Rent(minimumLength);
    }

    public static void ReturnObjectArray(object[] array, bool clearArray = false)
    {
        ArrayPool<object>.Shared.Return(array, clearArray);
    }
}
