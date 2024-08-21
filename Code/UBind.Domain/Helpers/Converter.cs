// <copyright file="Converter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Converts various things from one form to another.
    /// </summary>
    public static class Converter
    {
        public static string NameToAlias(string name)
        {
            // Lowercase the string
            name = name.ToLowerInvariant();

            // Replace spaces with hyphens
            name = name.Replace(" ", "-");

            // Remove all non-alphanumeric and non-hyphen characters
            name = Regex.Replace(name, "[^a-z0-9-]", string.Empty);

            // Replace consecutive hyphens with a single hyphen
            name = Regex.Replace(name, "-{2,}", "-");

            // Ensure the alias doesn't start with a hyphen
            if (name.StartsWith("-"))
            {
                name = name.Substring(1);
            }

            // Ensure the alias doesn't end with a hyphen
            if (name.EndsWith("-"))
            {
                name = name.Substring(0, name.Length - 1);
            }

            return name;
        }
    }
}
