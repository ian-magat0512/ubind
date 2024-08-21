// <copyright file="GetFeatureSettingsQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Feature
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Query Handler for getting Feature settings.
    /// </summary>
    public class GetFeatureSettingsQueryHandler : IQueryHandler<GetFeatureSettingsQuery, IEnumerable<Setting>>
    {
        private readonly ICachingResolver cachingResolver;

        public GetFeatureSettingsQueryHandler(ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Setting>> Handle(GetFeatureSettingsQuery request, CancellationToken cancellationToken)
        {
            var settings = this.cachingResolver.GetSettingsOrThrow(request.TenantId);
            return Task.FromResult(settings);
        }
    }
}
