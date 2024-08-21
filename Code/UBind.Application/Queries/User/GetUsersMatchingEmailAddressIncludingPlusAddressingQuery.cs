// <copyright file="GetUsersMatchingEmailAddressIncludingPlusAddressingQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.User
{
    using System;
    using System.Collections.Generic;
    using UBind.Application.User;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for getting users that matches an email address, including plus addressing.
    /// So if the email address passed is "person@domain", then the user with email address
    /// person+test@domain will also be matched.
    /// </summary>
    public class GetUsersMatchingEmailAddressIncludingPlusAddressingQuery : IQuery<IEnumerable<UserModel>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetUsersMatchingEmailAddressIncludingPlusAddressingQuery"/> class.
        /// </summary>
        /// <param name="tenantId">The tenantId, or null to get users from all tenants.</param>
        /// <param name="emailAddress">The user email address.</param>
        /// <param name="blocked">The indicator whether user is blocked.</param>
        /// <param name="performingUserId">The performing user id.</param>
        public GetUsersMatchingEmailAddressIncludingPlusAddressingQuery(
            Guid? tenantId,
            List<Guid>? organisationIds,
            string emailAddress,
            bool? blocked,
            Guid? performingUserId)
        {
            this.EmailAddress = emailAddress;
            this.TenantId = tenantId;
            this.OrganisationIds = organisationIds;
            this.PerformingUserId = performingUserId;
            this.Blocked = blocked;
        }

        /// <summary>
        /// Gets the user email address.
        /// </summary>
        public string EmailAddress { get; }

        /// <summary>
        /// Gets the tenantId.
        /// </summary>
        public Guid? TenantId { get; }

        public List<Guid>? OrganisationIds { get; }

        /// <summary>
        /// Gets the performing user Id.
        /// </summary>
        public Guid? PerformingUserId { get; }

        /// <summary>
        /// Gets a value indicating whether User is blocked.
        /// </summary>
        public bool? Blocked { get; }
    }
}
