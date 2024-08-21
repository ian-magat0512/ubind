// <copyright file="AuthenticationMethodBase.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Organisation.AuthenticationMethod
{
    using Flurl;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.JsonConverters;

    public abstract class AuthenticationMethodBase : MutableEntity<Guid>, IAuthenticationMethod
    {
        public string Name { get; set; }

        public abstract string TypeName { get; }

        public bool CanCustomersSignIn { get; set; }

        public bool CanAgentsSignIn { get; set; }

        public bool IncludeSignInButtonOnPortalLoginPage { get; set; }

        public string? SignInButtonBackgroundColor { get; set; }

        [JsonConverter(typeof(UrlConverter))]
        public Url? SignInButtonIconUrl { get; set; }

        public string? SignInButtonLabel { get; set; }

        public bool Disabled { get; set; }

        // This method is necesary because Mapperly doesn't support setting additional constructor params during
        // the mapping process
        public void SetId(Guid id)
        {
            this.Id = id;
        }

        // This method is necesary because Mapperly doesn't support setting additional constructor params during
        // the mapping process
        public void SetCreatedTimestamp(Instant createdTimestamp)
        {
            this.CreatedTimestamp = createdTimestamp;
            this.LastModifiedTimestamp = createdTimestamp;
        }
    }
}
