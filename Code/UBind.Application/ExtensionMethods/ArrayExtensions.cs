// <copyright file="ArrayExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ExtensionMethods;

/// <summary>
/// Extension methods for arrays.
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    /// Searches the index of the first instance of the given value from the array.
    /// </summary>
    /// <param name="array">This array.</param>
    /// <param name="value">The value to search for.</param>
    /// <returns>The index of the first instance.</returns>
    public static int IndexOf<T>(this T[] array, T value)
    {
        return Array.IndexOf(array, value);
    }

    /// <summary>
    /// Get a value from the array at the specified index.
    /// </summary>
    /// <param name="array">This array.</param>
    /// <param name="index">The index of the value to retrieve.</param>
    /// <param name="value">The object container of the value retrieved.</param>
    /// <returns>True if a value is retrieved successfully, false otherwise.</returns>
    public static bool TryGetValue<T>(this T[] array, int index, out T? value)
    {
        value = default;
        if (array == null)
        {
            return false;
        }

        if (array.Length <= index)
        {
            return false;
        }

        value = array[index];
        return true;
    }
}
