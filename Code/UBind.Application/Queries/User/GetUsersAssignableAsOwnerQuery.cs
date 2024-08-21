// <copyright file="GetUsersAssignableAsOwnerQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Get users that are assignable as owner of a customers in the given organisation.
    /// When we want to assign an agent user as the owner of a customer, we need to get the list of users
    /// to present so that one can be selected. This query checks which users in the organisation have the
    /// ManageCustomers permission.
    /// </summary>
    public class GetUsersAssignableAsOwnerQuery : IQuery<List<UserReadModel>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetUsersAssignableAsOwnerQuery"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="organisationId">The organisation id.</param>
        public GetUsersAssignableAsOwnerQuery(Guid tenantId, Guid organisationId)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
        }

        /// <summary>
        /// Gets the tenant id.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the organisation id.
        /// </summary>
        public Guid OrganisationId { get; }
    }
}
