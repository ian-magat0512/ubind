// <copyright file="RegexExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Extension methods for regex.
    /// </summary>
    public static class RegexExtensions
    {
        /// <summary>
        /// converts a string with emails to an extracted email with delimiter.
        /// </summary>
        /// <param name="value">The string with emails on it.</param>
        /// <returns>The enum value.</returns>
        public static List<string> ExtractEmails(this string value)
        {
            if (value == null)
            {
                return null;
            }

            string pattern = @"([a-zA-Z0-9+._-]+@[a-zA-Z0-9._-]+\.[a-zA-Z0-9_-]+)";
            Regex rgx = new Regex(pattern);

            List<string> emails = new List<string>();
            foreach (Match match in rgx.Matches(value))
            {
                emails.Add(match.Value);
            }

            return emails;
        }
    }
}
