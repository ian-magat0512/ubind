// <copyright file="SystemEmailTemplateSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.ReadModel;

    public class SystemEmailTemplateSummary : ISystemEmailTemplateSummary
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets the instant in time the entity was created.
        /// </summary>
        public Instant CreatedTimestamp
        {
            get { return Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch); }
            private set { this.CreatedTicksSinceEpoch = value.ToUnixTimeTicks(); }
        }

        /// <summary>
        /// Gets or sets the entity created time (in ticks since Epoch).
        /// </summary>
        /// <remarks> Primitive typed property for EF to store created time.</remarks>
        public long CreatedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the email type.
        /// </summary>
        public SystemEmailType Type { get; set; }

        /// <summary>
        /// Gets or sets the Product ID.
        /// </summary>
        public Guid? ProductId { get; set; }

        /// <summary>
        /// Gets or sets the Tenant ID.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the Portal ID.
        /// </summary>
        public Guid? PortalId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this email template is active.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the actual data to use for the email.
        /// </summary>
        public SystemEmailTemplateData Data { get; set; }
    }
}
