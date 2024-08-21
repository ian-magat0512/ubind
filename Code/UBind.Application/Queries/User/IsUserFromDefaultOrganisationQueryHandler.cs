// <copyright file="IsUserFromDefaultOrganisationQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Checks if the user is in the default org for their tenant.
    /// </summary>
    public class IsUserFromDefaultOrganisationQueryHandler : IQueryHandler<IsUserFromDefaultOrganisationQuery, bool>
    {
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="IsUserFromDefaultOrganisationQueryHandler"/> class.
        /// </summary>
        public IsUserFromDefaultOrganisationQueryHandler(ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public async Task<bool> Handle(IsUserFromDefaultOrganisationQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tenant = await this.cachingResolver.GetTenantOrThrow(new Domain.Helpers.GuidOrAlias(request.User.GetTenantId()));
            return tenant.Details.DefaultOrganisationId == request.User.GetOrganisationId();
        }
    }
}
