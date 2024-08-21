// <copyright file="OrganisationIdForFilterQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Gets the organisation ID to use for the query.
    /// If the organisation is not set, and the user is not in the default org, set it from
    /// the user's organisation.
    /// </summary>
    public class OrganisationIdForFilterQueryHandler : IQueryHandler<OrganisationIdForFilterQuery, Guid?>
    {
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationIdForFilterQueryHandler"/> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        public OrganisationIdForFilterQueryHandler(ICqrsMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <inheritdoc/>
        public async Task<Guid?> Handle(OrganisationIdForFilterQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (request.OrganisationId == null)
            {
                bool userInDefaultOrg =
                    await this.mediator.Send(new IsUserFromDefaultOrganisationQuery(request.PerformingUser));
                if (!userInDefaultOrg)
                {
                    return await Task.FromResult(request.PerformingUser.GetOrganisationId());
                }
            }

            return await Task.FromResult(request.OrganisationId);
        }
    }
}
