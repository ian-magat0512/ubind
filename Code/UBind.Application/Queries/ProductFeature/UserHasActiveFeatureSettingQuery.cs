// <copyright file="UserHasActiveFeatureSettingQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.FeatureSettings
{
    using System.Security.Claims;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Query for determining if a user has an active Feature setting.
    /// </summary>
    public class UserHasActiveFeatureSettingQuery : IQuery<bool>
    {
        public UserHasActiveFeatureSettingQuery(ClaimsPrincipal user, Feature feature)
        {
            this.User = user;
            this.Feature = feature;
        }

        public ClaimsPrincipal User { get; }

        public Feature Feature { get; }
    }
}
