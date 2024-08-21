// <copyright file="IntExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System.Collections.Generic;

    /// <summary>
    /// Extension methods for integers.
    /// </summary>
    public static class IntExtensions
    {
        /// <summary>
        /// Convert an integer to its base 26 representation, using the full English alphabet where A = 0, B = 1, .... Z = 25.
        /// </summary>
        /// <param name="value">The integer to convert.</param>
        /// <returns>A string containing the base26 representation of the integer.</returns>
        public static string ToBase26(this int value)
        {
            const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var output = new Stack<char>();
            do
            {
                output.Push(Alphabet[value % 26]);
                value /= 26;
            }
            while (value > 0);

            return new string(output.ToArray());
        }
    }
}
