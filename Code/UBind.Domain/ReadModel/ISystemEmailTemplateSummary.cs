// <copyright file="ISystemEmailTemplateSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using NodaTime;
    using UBind.Domain;

    public interface ISystemEmailTemplateSummary
    {
        /// <summary>
        /// Gets the system email template ID.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the instant in time the entity was created.
        /// </summary>
        Instant CreatedTimestamp
        {
            get;
        }

        /// <summary>
        /// Gets or sets the entity created time (in ticks since Epoch).
        /// </summary>
        /// <remarks> Primitive typed property for EF to store created time.</remarks>
        long CreatedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets the email type.
        /// </summary>
        SystemEmailType Type { get; }

        /// <summary>
        /// Gets the Product ID.
        /// </summary>
        Guid? ProductId { get; }

        /// <summary>
        /// Gets the Tenant ID.
        /// </summary>
        Guid TenantId { get; }

        /// <summary>
        /// Gets the Portal ID.
        /// </summary>
        Guid? PortalId { get; }

        /// <summary>
        /// Gets a value indicating whether this email template is active.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Gets the actual data to use for the email.
        /// </summary>
        SystemEmailTemplateData Data { get; }
    }
}
