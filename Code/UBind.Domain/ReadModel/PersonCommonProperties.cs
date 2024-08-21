// <copyright file="PersonCommonProperties.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Person's common properties.
    /// </summary>
    public class PersonCommonProperties : EntityReadModel<Guid>
    {
        /// <summary>
        /// Gets or sets the person's full name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the person's name prefix.
        /// </summary>
        public string NamePrefix { get; set; }

        /// <summary>
        /// Gets or sets the person's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the person's middle names.
        /// </summary>
        public string MiddleNames { get; set; }

        /// <summary>
        /// Gets or sets the person's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the person's name suffix.
        /// </summary>
        public string NameSuffix { get; set; }

        /// <summary>
        /// Gets or sets the person's company.
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the person's title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the person's preferred name.
        /// </summary>
        public string PreferredName { get; set; }

        /// <summary>
        /// Gets or sets the person's Email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the person's alternate email.
        /// </summary>
        public string AlternativeEmail { get; set; }

        /// <summary>
        /// Gets or sets the person's mobile phone number.
        /// </summary>
        public string MobilePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the person's home phone number.
        /// </summary>
        public string HomePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the person's work phone number.
        /// </summary>
        public string WorkPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the person's display name.
        /// </summary>
        /// <remarks>This property is calculated based on the first and last name of the person.</remarks>
        [NotMapped]
        public string DisplayName
        {
            get
            {
                var personalDetails = new PersonalDetails(this.TenantId, this);
                return PersonPropertyHelper.GetDisplayName(personalDetails);
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the person's organisation Id.
        /// </summary>
        public Guid OrganisationId { get; set; }
    }
}
