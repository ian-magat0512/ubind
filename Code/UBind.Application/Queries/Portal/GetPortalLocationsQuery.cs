// <copyright file="GetPortalLocationsQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Portal
{
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Portal;

    public class GetPortalLocationsQuery : IQuery<PortalLocations>
    {
        public GetPortalLocationsQuery(PortalReadModel portal, bool includeEnvironmentQueryParameter = true)
        {
            this.Portal = portal;
            this.IncludeEnvironmentQueryParameter = includeEnvironmentQueryParameter;
        }

        public PortalReadModel Portal { get; }

        /// <summary>
        /// Gets a value indicating whether to include the environment query parameter in the URL.
        /// The environment query parameter would only be included if the environment is not Production,
        /// and a default portal URL is being used.
        /// <remarks>
        /// For automations, we dont usually include the environment query parameter because
        /// the URL is usually added to. The path returned is used as a base path to be built upon.
        /// If we were to include it, then it would be difficult to add to without splitting the string.
        /// Automations can detect the environment and append it manually after constructing the URL.
        /// </remarks>
        /// </summary>
        public bool IncludeEnvironmentQueryParameter { get; }
    }
}
