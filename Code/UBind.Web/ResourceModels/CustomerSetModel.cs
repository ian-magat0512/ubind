// <copyright file="CustomerSetModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Linq;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;

    /// <summary>
    /// Resource model for serving the list of customers available.
    /// </summary>
    public class CustomerSetModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerSetModel"/> class.
        /// </summary>
        /// <param name="customer">The customer read model.</param>
        public CustomerSetModel(ICustomerReadModelSummary customer)
        {
            this.PrimaryPersonId = customer.PrimaryPersonId;
            this.Id = customer.Id;
            var primaryPerson = customer.People?.FirstOrDefault(p => p.Id == customer.PrimaryPersonId);
            this.UserId = primaryPerson?.UserId;
            this.Status = this.UserId.IsNullOrEmpty()
                ? "New"
                : customer.UserIsBlocked
                    ? "Disabled"
                    : customer.UserHasBeenActivated
                        ? "Active"
                        : customer.UserHasBeenInvitedToActivate
                            ? "Invited"
                            : "Unknown";
            this.IsTestData = customer.IsTestData;
            this.CreatedDateTime = customer.CreatedTimestamp.ToExtendedIso8601String();
            this.LastModifiedDateTime = customer.LastModifiedTimestamp.ToExtendedIso8601String();
            var personCommonProperties = new PersonCommonProperties
            {
                FullName = customer.FullName,
                PreferredName = customer.PreferredName,
                NamePrefix = customer.NamePrefix,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                NameSuffix = customer.NameSuffix,
                MiddleNames = customer.MiddleNames,
            };
            personCommonProperties.SetNameComponentsFromFullNameIfNoneAlreadySet();
            if (string.IsNullOrWhiteSpace(personCommonProperties.FullName))
            {
                personCommonProperties.SetBasicFullName();
            }

            this.FullName = string.IsNullOrWhiteSpace(personCommonProperties.FullName)
                ? primaryPerson?.FullName
                : personCommonProperties.FullName;
            this.DisplayName = string.IsNullOrWhiteSpace(personCommonProperties.DisplayName)
                ? primaryPerson?.DisplayName
                : personCommonProperties.DisplayName;
        }

        /// <summary>
        /// Gets the ID of the customer record.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the name of the customer.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Gets the display name of the customer.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the status of the customer.
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// Gets the date the customer was created.
        /// </summary>
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the date the customer was last modified.
        /// </summary>
        public string LastModifiedDateTime { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the data is for test.
        /// </summary>
        public bool IsTestData { get; private set; }

        /// <summary>
        /// Gets the customers's profile picture id, if any, otherwise null.
        /// </summary>
        public string ProfilePictureId { get; private set; }

        /// <summary>
        /// Gets the user id of the customer.
        /// </summary>
        public Guid? UserId { get; private set; }

        /// <summary>
        /// Gets the primary person Id of the customer.
        /// </summary>
        public Guid PrimaryPersonId { get; private set; }
    }
}
