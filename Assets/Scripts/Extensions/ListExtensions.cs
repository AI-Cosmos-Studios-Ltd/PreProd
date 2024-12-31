using System.Collections.Generic;

public static class ListExtensions 
{
    public static string ToCSV<T>(this IEnumerable<T> list, string delimiter = ",") {
        return string.Join(delimiter, list);
    }

    public static void AddIfUnique<T>(this List<T> list, T item) {
        if(!list.Contains(item)) {
            list.Add(item);
        }
    }

    public static void AddIfUnique<T>(this List<T> list, IEnumerable<T> item) {
        foreach(T t in item) {
            list.AddIfUnique(t);
        }
    }

    public static T Random<T>(this List<T> list) {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
}
