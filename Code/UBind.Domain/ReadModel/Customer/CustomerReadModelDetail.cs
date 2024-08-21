// <copyright file="CustomerReadModelDetail.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Customer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;

    /// <summary>
    /// Details about the customer.
    /// </summary>
    public class CustomerReadModelDetail : ICustomerReadModelSummary
    {
        /// <summary>
        /// Gets or sets the customer's ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the person the user refers to.
        /// </summary>
        public Guid PrimaryPersonId { get; set; }

        /// <summary>
        /// Gets or sets the environment the customer sits in.
        /// </summary>
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user the customer relates to, if any, otherwise default.
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return a test data.
        /// </summary>
        public bool IsTestData { get; set; }

        /// <summary>
        /// Gets or sets the Id of the user who owns this customer.
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the person who owns this customer.
        /// </summary>
        public Guid OwnerPersonId { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person who owns this customer.
        /// </summary>
        public string OwnerFullName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is a user for this customer and
        /// the user is blocked.
        /// </summary>
        public bool UserIsBlocked { get; set; }

        /// <summary>
        /// Gets or sets the ID of the portal which the customer would log into by default,
        /// If there is no specific portal required for a given product.
        /// This would be null if the customer doesn't log into a portal, or the customer
        /// is expected to login to the default portal for the tenanacy.
        /// This needed for the generation of links in emails, e.g. the user activation link.
        /// </summary>
        public Guid? PortalId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is a user for this customer and
        /// the user has been invited to activate.
        /// </summary>
        public bool UserHasBeenInvitedToActivate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is a usr for this customer and
        /// the user has been activated.
        /// </summary>
        public bool UserHasBeenActivated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer is marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the time the customer was created in ticks in the epoch.
        /// </summary>
        /// <remarks>Exposed for efficient querying (in future).</remarks>
        public long CreatedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the time the customer was created.
        /// </summary>
        public Instant CreatedTimestamp
        {
            get
            {
                return Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch);
            }

            set
            {
                this.CreatedTicksSinceEpoch = value.ToUnixTimeTicks();
            }
        }

        /// <summary>
        /// Gets or sets the time the customer was modified in ticks since epoch.
        /// </summary>
        public long LastModifiedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the time the customer was modified in ticks in the epoch.
        /// </summary>
        public Instant LastModifiedTimestamp
        {
            get
            {
                return Instant.FromUnixTimeTicks(this.LastModifiedTicksSinceEpoch);
            }

            set
            {
                this.LastModifiedTicksSinceEpoch = value.ToUnixTimeTicks();
            }
        }

        /// <summary>
        /// Gets or sets the user's full name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the user's name prefix.
        /// </summary>
        public string NamePrefix { get; set; }

        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's middle names.
        /// </summary>
        public string MiddleNames { get; set; }

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the user's name suffix.
        /// </summary>
        public string NameSuffix { get; set; }

        /// <summary>
        /// Gets or sets the user's company.
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the user's title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public string PreferredName { get; set; }

        /// <summary>
        /// Gets or sets the user's Email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's alternate email.
        /// </summary>
        public string AlternativeEmail { get; set; }

        /// <summary>
        /// Gets or sets the user's mobile phone number.
        /// </summary>
        public string MobilePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the user's home phone number.
        /// </summary>
        public string HomePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the user's work phone number.
        /// </summary>
        public string WorkPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the Id of the tenant of the user.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the user's display Name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the user's deserialized repeating fields string.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the organisation name.
        /// </summary>
        public string OrganisationName { get; set; }

        /// <summary>
        /// Gets or sets the enumerable of person.
        /// </summary>
        public ICollection<PersonReadModel> People { get; set; }

        /// <summary>
        /// Gets the primary person.
        /// </summary>
        public PersonReadModel PrimaryPerson => this.People.SingleOrDefault(p => p.Id == this.PrimaryPersonId);
    }
}
