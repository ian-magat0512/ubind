// <copyright file="OrganisationAuthenticationMethodUpsertEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Organisation
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Organisation.AuthenticationMethod;

    /// <summary>
    /// The aggregate for organisations.
    /// </summary>
    public partial class Organisation
    {
        public class OrganisationAuthenticationMethodUpsertEvent
            : Event<Organisation, Guid>
        {
            public OrganisationAuthenticationMethodUpsertEvent(
                Guid tenantId,
                Guid organisationId,
                IAuthenticationMethod authenticationMethod,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, organisationId, performingUserId, createdTimestamp)
            {
                this.AuthenticationMethod = authenticationMethod;
            }

            [JsonConstructor]
            protected OrganisationAuthenticationMethodUpsertEvent()
            {
            }

            public IAuthenticationMethod AuthenticationMethod { get; set; }
        }
    }
}
