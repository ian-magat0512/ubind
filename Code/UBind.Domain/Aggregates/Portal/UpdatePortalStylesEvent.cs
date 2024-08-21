// <copyright file="UpdatePortalStylesEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Portal
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    public partial class PortalAggregate
    {
        public class UpdatePortalStylesEvent : Event<PortalAggregate, Guid>
        {
            public UpdatePortalStylesEvent(
                Guid tenantId,
                Guid portalId,
                string stylesheetUrl,
                string styles,
                Guid? performingUserId,
                Instant timestamp)
                : base(tenantId, portalId, performingUserId, timestamp)
            {
                this.StylesheetUrl = stylesheetUrl;
                this.Styles = styles;
            }

            [JsonProperty]
            public string StylesheetUrl { get; private set; }

            [JsonProperty]
            public string Styles { get; private set; }
        }
    }
}
