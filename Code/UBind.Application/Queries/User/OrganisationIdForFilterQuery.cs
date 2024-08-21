// <copyright file="OrganisationIdForFilterQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using System;
    using System.Security.Claims;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Gets the organisation ID to use for the query.
    /// If the organisation is not set, and the user is not in the default org, set it from
    /// the user's organisation.
    /// </summary>
    public class OrganisationIdForFilterQuery : IQuery<Guid?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationIdForFilterQuery"/> class.
        /// </summary>
        /// <param name="organisationId">The organistion ID, if set.</param>
        /// <param name="performingUser">The performing user.</param>
        public OrganisationIdForFilterQuery(Guid? organisationId, ClaimsPrincipal performingUser)
        {
            this.OrganisationId = organisationId;
            this.PerformingUser = performingUser;
        }

        /// <summary>
        /// Gets the organistion ID, if set.
        /// </summary>
        public Guid? OrganisationId { get; }

        /// <summary>
        /// Gets the performing user.
        /// </summary>
        public ClaimsPrincipal PerformingUser { get; }
    }
}
