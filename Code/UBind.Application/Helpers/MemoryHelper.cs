// <copyright file="MemoryHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Helpers;

using System.Diagnostics;

/// <summary>
/// Provides methods for working with system memory, including retrieving available physical memory
/// and formatting memory sizes in human-readable formats.
/// </summary>
public class MemoryHelper
{
    /// <summary>
    /// Retrieves the amount of available physical memory in bytes.
    /// </summary>
    /// <returns>
    /// The available physical memory in bytes.
    /// </returns>
    public static ulong GetAvailablePhysicalMemory()
    {
        using (var pc = new PerformanceCounter("Memory", "Available Bytes"))
        {
            return Convert.ToUInt64(pc.NextValue());
        }
    }

    /// <summary>
    /// Formats the given memory size in bytes into a human-readable string.
    /// </summary>
    /// <param name="bytes">The size of memory in bytes.</param>
    /// <returns>
    /// A formatted string representing the memory size in a human-readable format (e.g., KB, MB).
    /// </returns>
    public static string FormatMemorySize(ulong bytes)
    {
        string[] sizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        int i = 0;
        double memorySize = bytes;
        while (memorySize >= 1024 && i < sizeSuffixes.Length - 1)
        {
            memorySize /= 1024;
            i++;
        }

        return $"{memorySize:0.##} {sizeSuffixes[i]}";
    }
}
