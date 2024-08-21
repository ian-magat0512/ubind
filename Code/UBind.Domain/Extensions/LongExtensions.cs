// <copyright file="LongExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions;

using System.Collections.Generic;

/// <summary>
/// Extension methods for long number.
/// </summary>
public static class LongExtensions
{

    /// <summary>
    /// Convert a long to its base 36 representation, using the full English alphabet and numbers
    /// where 0 = 0, A = 11, B = 1, .... Z = 35.
    /// </summary>
    /// <param name="value">The long value to convert.</param>
    /// <returns>A string containing the base36 representation of the long number.</returns>
    public static string ToBase36(this long value)
    {
        const string Alphanumeric = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var output = new Stack<char>();
        do
        {
            output.Push(Alphanumeric[(int)(value % 36)]);
            value /= 36;
        }
        while (value > 0);

        return new string(output.ToArray());
    }
}
