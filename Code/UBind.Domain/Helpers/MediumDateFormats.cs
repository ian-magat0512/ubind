// <copyright file="MediumDateFormats.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System.Collections.Generic;

    /// <summary>
    /// Helper for providing medium date format strings for particular cultures.
    /// </summary>
    public class MediumDateFormats
    {
        private static Dictionary<string, string> mediumDateFormatsByLocale = new Dictionary<string, string>
        {
            { Locales.en_AU, "d MMM yyyy" },
            { Locales.en_GB, "d MMM yyyy" },
            { Locales.en_US, "MMM d, yyyy" },
            { Locales.en_NZ, "d/MM/yyyy" },
        };

        /// <summary>
        /// Gets the format string to use for formatting a date in medium format in a given locale.
        /// </summary>
        /// <remarks>
        /// If a medium format string for the given locale cannot be found, the generic short format "d" will be used.
        /// </remarks>
        /// <param name="locale">The locale code.</param>
        /// <returns>The format string to use.</returns>
        public string this[string locale] => mediumDateFormatsByLocale.TryGetValue(locale, out string format) ? format : "d";
    }
}
