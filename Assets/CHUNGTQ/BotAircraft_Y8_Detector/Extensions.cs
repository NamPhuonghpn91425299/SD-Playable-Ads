using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
    {
        return new HashSet<T>(source);
    }
}
