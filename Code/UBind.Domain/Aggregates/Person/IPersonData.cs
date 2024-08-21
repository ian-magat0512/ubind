// <copyright file="IPersonData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person
{
    /// <summary>
    /// Interface for a person's data.
    /// </summary>
    /// <remarks>
    /// This interface is for the data that belongs directly to a person (e.g. name and contact details)
    /// and unlike <see cref="IPersonalDetails"/> does not include other data about relation to tenant etc.
    /// </remarks>
    public interface IPersonData
    {
        /// <summary>
        /// Gets a person's full name.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets the user's name prefix.
        /// </summary>
        string NamePrefix { get; }

        /// <summary>
        /// Gets the user's first name.
        /// </summary>
        string FirstName { get; }

        /// <summary>
        /// Gets the user's middle names.
        /// </summary>
        string MiddleNames { get; }

        /// <summary>
        /// Gets the user's last name.
        /// </summary>
        string LastName { get; }

        /// <summary>
        /// Gets the user's first and last name.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the user's name suffix.
        /// </summary>
        string NameSuffix { get; }

        /// <summary>
        /// Gets the user's company.
        /// </summary>
        string Company { get; }

        /// <summary>
        /// Gets the user's company.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets a person's preferred name.
        /// </summary>
        string PreferredName { get; }

        /// <summary>
        /// Gets a person's email.
        /// </summary>
        string Email { get; }

        /// <summary>
        /// Gets a person's alternative email.
        /// </summary>
        string AlternativeEmail { get; }

        /// <summary>
        /// Gets a person's mobile phone number.
        /// </summary>
        string MobilePhone { get; }

        /// <summary>
        /// Gets a person's home phone number.
        /// </summary>
        string HomePhone { get; }

        /// <summary>
        /// Gets a person's work phone number.
        /// </summary>
        string WorkPhone { get; }

        /// <summary>
        /// Sets the email, regardless of its previous value.
        /// </summary>
        void SetEmail(string email);

        /// <summary>
        /// Sets the alternative email, regardless of its previous value.
        /// </summary>
        void SetAlternativeEmail(string email);
    }
}
