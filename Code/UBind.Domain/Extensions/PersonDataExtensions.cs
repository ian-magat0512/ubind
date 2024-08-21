// <copyright file="PersonDataExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using UBind.Domain.Aggregates.Person;

    /// <summary>
    /// Extension methods for the <see cref="IPersonData"/> interface.
    /// </summary>
    public static class PersonDataExtensions
    {
        /// <summary>
        /// Gets the greeting name of the person.
        /// </summary>
        /// <param name="personalDetail">The person details.</param>
        /// <returns>The greeting name string.</returns>
        public static string GetGreetingName(this IPersonData personalDetail)
        {
            // preferred name
            if (!string.IsNullOrWhiteSpace(personalDetail.PreferredName))
            {
                return personalDetail.PreferredName;
            }

            // first name
            if (!string.IsNullOrWhiteSpace(personalDetail.FirstName))
            {
                return personalDetail.FirstName;
            }

            // name prefix and last name
            if (!string.IsNullOrWhiteSpace(personalDetail.NamePrefix) && !string.IsNullOrWhiteSpace(personalDetail.LastName))
            {
                return $"{personalDetail.NamePrefix} {personalDetail.LastName}";
            }

            // full name
            if (!string.IsNullOrWhiteSpace(personalDetail.FullName))
            {
                return personalDetail.FullName;
            }

            return "Sir/Madam";
        }
    }
}
