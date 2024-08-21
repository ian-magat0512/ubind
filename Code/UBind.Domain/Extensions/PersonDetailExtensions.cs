// <copyright file="PersonDetailExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System.Linq;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Extension methods for Person detail related classes.
    /// </summary>
    public static class PersonDetailExtensions
    {
        /// <summary>
        /// Sets the full name property to a value in basic first name and last name format.
        /// </summary>
        /// <param name="propertyObject">The object containing the common properties of a person.</param>
        public static void SetBasicFullName(this PersonCommonProperties propertyObject)
        {
            if (!string.IsNullOrEmpty(propertyObject.FirstName))
            {
                propertyObject.FullName = propertyObject.LastName.IsNullOrWhitespace()
                    ? propertyObject.FirstName.Trim()
                    : $"{propertyObject.FirstName.Trim()} {propertyObject.LastName.Trim()}";
            }
        }

        /// <summary>
        /// Check and Fix person name components.
        /// </summary>
        /// <param name="propertyObject">The object containing the common properties of a person.</param>
        /// <remarks>
        /// Since previously we have been collecting only fullname and preferred name, and now want to use
        /// name components, this method is used to populate the name components from the full name.
        /// </remarks>
        public static void SetNameComponentsFromFullNameIfNoneAlreadySet(this PersonCommonProperties propertyObject)
        {
            if (string.IsNullOrWhiteSpace(propertyObject.FullName))
            {
                // We can't do anything if there is no full name.
                return;
            }

            if (propertyObject.FirstName.IsNotNullOrEmpty() ||
                propertyObject.LastName.IsNotNullOrEmpty() ||
                propertyObject.MiddleNames.IsNotNullOrEmpty())
            {
                // If any of these name components are already set, then it indicates the name
                // was created or edited after we introduced name components, so we do not want
                // to set ANY components from the full name.
                return;
            }

            var fullNameParts = propertyObject.FullName.Trim().Split(' ');
            if (fullNameParts.Length > 0)
            {
                propertyObject.FirstName = fullNameParts.First().Trim();
            }

            if (fullNameParts.Length > 1)
            {
                propertyObject.LastName = fullNameParts.Last().Trim();
            }

            if (fullNameParts.Length > 2)
            {
                var middleNameParts = fullNameParts.Skip(1).SkipLast(1).Select(n => n.Trim());
                propertyObject.MiddleNames = string.Join(" ", middleNameParts);
            }
        }
    }
}
