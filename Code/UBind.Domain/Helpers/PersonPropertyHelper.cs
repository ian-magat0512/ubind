// <copyright file="PersonPropertyHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Helper to modify person properties.
    /// </summary>
    public static class PersonPropertyHelper
    {
        /// <summary>
        /// Gets the person's Full Name for name parts.
        /// </summary>
        /// <param name="preferredName">The person's preferred name.</param>
        /// <param name="namePrefix">The person's name prefix.</param>
        /// <param name="firstName">The person's first name.</param>
        /// <param name="lastName">The person's last name.</param>
        /// <param name="nameSuffix">The person's name suffix.</param>
        /// <param name="middleNames">The person's middle names.</param>
        /// <returns>The assembled fullname from name parts.</returns>
        public static string GetFullNameFromParts(
            string preferredName,
            string namePrefix,
            string firstName,
            string lastName,
            string nameSuffix,
            string middleNames)
        {
            var prefName = !string.IsNullOrEmpty(preferredName) ? "(" + preferredName + ")" : string.Empty;
            var formattedFullname = $"{namePrefix} {firstName} {prefName} {middleNames} {lastName} {nameSuffix}";
            return Regex.Replace(formattedFullname, @"\s+", " ").Trim();
        }

        /// <summary>
        /// Gets the person's name in "FirstName LastName" format.
        /// </summary>
        /// <param name="rawFullName">The raw full name from the database as it was entered by the customer.</param>
        /// <returns>The display name.</returns>
        public static string GetDisplayName(string rawFullName)
        {
            var firstName = string.Empty;
            var lastName = string.Empty;

            if (rawFullName is null)
            {
                return string.Empty;
            }

            var nameComponents = rawFullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (nameComponents.Length >= 1)
            {
                firstName = nameComponents.First();
            }

            if (nameComponents.Length >= 2)
            {
                lastName = nameComponents.Last();
            }

            // The trim ensures that the output string is empty if firstName and LastName is empty.
            return $"{firstName} {lastName}".Trim();
        }

        /// <summary>
        /// Evaluates the display name based from the personal details.
        /// See the logic of this method in terms of priority:
        ///     1. If the persons firstname and lastname are there
        ///     2. next, use the preferred name if available
        ///     3. next, if only the firstname is there, use that
        ///     4. next, if only the lastname is there use that
        ///     5. next, use the FullName if not empty
        ///     6. next, use the email address if not empty
        ///     7. next, use the mobile phone number, if not empty.
        /// </summary>
        /// <param name="personalDetails">The details of the person to get display name.</param>
        /// <returns>The display name.</returns>
        public static string GetDisplayName(IPersonalDetails personalDetails)
        {
            return GetDisplayName(
                    personalDetails.FirstName,
                    personalDetails.LastName,
                    personalDetails.PreferredName,
                    personalDetails.FullName,
                    personalDetails.Email,
                    personalDetails.MobilePhoneNumber);
        }

        public static string GetDisplayName(
            string firstName,
            string lastName,
            string preferredName,
            string fullName,
            string email,
            string mobilePhoneNumber)
        {
            // First priority
            var hasFirstName = firstName.IsNotNullOrWhitespace();
            var hasLastName = lastName.IsNotNullOrWhitespace();
            if (hasFirstName && hasLastName)
            {
                return $"{firstName.Trim()} {lastName.Trim()}";
            }

            // Second priority
            var hasPreferredName = preferredName.IsNotNullOrWhitespace();
            if (hasPreferredName)
            {
                return preferredName.Trim();
            }

            // Third priority
            if (hasFirstName)
            {
                return firstName.Trim();
            }

            // Fourth priority
            if (hasLastName)
            {
                return lastName.Trim();
            }

            // Fifth priority
            var hasFullName = fullName.IsNotNullOrWhitespace();
            if (hasFullName)
            {
                return fullName.Trim();
            }

            // Sixth priority
            var hasEmailAddress = email.IsNotNullOrWhitespace();
            if (hasEmailAddress)
            {
                return email.Trim();
            }

            // Last priority
            var hasMobilePhoneNumber = mobilePhoneNumber.IsNotNullOrWhitespace();
            if (hasMobilePhoneNumber)
            {
                return mobilePhoneNumber;
            }

            return string.Empty;
        }
    }
}
