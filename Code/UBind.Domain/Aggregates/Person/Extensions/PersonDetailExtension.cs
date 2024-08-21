// <copyright file="PersonDetailExtension.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

using UBind.Domain.Aggregates.Person;

/// <summary>
/// The person details extension method.
/// </summary>
public static class PersonDetailExtension
{
    /// <summary>
    /// Sets the email if its null, gets the first email on the email addresses
    /// </summary>
    public static void SetEmailIfNull(this IPersonalDetails personalDetails)
    {
        var emailFields = personalDetails.EmailAddresses?.ToList();

        if (emailFields.Any() && personalDetails.Email == null)
        {
            if (emailFields[0].EmailAddressValueObject != null)
            {
                personalDetails.SetEmail(emailFields[0].EmailAddressValueObject?.ToString());
            }

            if (emailFields.Count > 1)
            {
                if (emailFields[1].EmailAddressValueObject != null && personalDetails.AlternativeEmail == null)
                {
                    personalDetails.SetAlternativeEmail(emailFields[1].EmailAddressValueObject?.ToString());
                }
            }
        }
    }
}
