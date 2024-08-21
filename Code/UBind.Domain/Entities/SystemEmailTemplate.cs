// <copyright file="SystemEmailTemplate.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// A template for a uBind system email.
    /// </summary>
    public class SystemEmailTemplate : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEmailTemplate"/> class.
        /// </summary>
        /// <param name="tenantId">The string Tenant ID.</param>
        /// <param name="type">The type of email the template is for.</param>
        /// <param name="productId">The string Product ID.</param>
        /// <param name="portalId">The Portal ID.</param>
        /// <param name="data">The actual data for creating and sending the email.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        private SystemEmailTemplate(
            Guid tenantId,
            SystemEmailType type,
            Guid? productId,
            Guid? portalId,
            SystemEmailTemplateData data,
            Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Type = type;
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.PortalId = portalId;
            this.Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEmailTemplate"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        private SystemEmailTemplate()
            : base(Guid.NewGuid(), default(Instant))
        {
        }

        /// <summary>
        /// Gets the email type.
        /// </summary>
        public SystemEmailType Type { get; private set; }

        /// <summary>
        /// Gets the Product ID.
        /// </summary>
        public Guid? ProductId { get; private set; }

        /// <summary>
        /// Gets the Tenant ID.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the Portal ID.
        /// </summary>
        public Guid? PortalId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this email template is active.
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// Gets the actual data to use for the email.
        /// </summary>
        public SystemEmailTemplateData Data { get; private set; }

        /// <summary>
        /// Create a Product Email Template Setting.
        /// </summary>
        /// <param name="tenantId">The string Tenant ID.</param>
        /// <param name="productId">The string Product ID.</param>
        /// <param name="type">The type of email the template is for.</param>
        /// <param name="data">The actual data for creating and sending the email.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        /// <returns>EmailTemplateSetting.</returns>
        public static SystemEmailTemplate CreateProductEmailTemplateSetting(
            Guid tenantId,
            Guid productId,
            SystemEmailType type,
            SystemEmailTemplateData data,
            Instant createdTimestamp)
        {
            return new SystemEmailTemplate(
                tenantId,
                type,
                productId,
                null,
                data,
                createdTimestamp);
        }

        /// <summary>
        /// Create a Portal Email Template Setting.
        /// </summary>
        /// <param name="tenantId">The Tenant ID.</param>
        /// <param name="type">The type of email the template is for.</param>
        /// <param name="portalId">The Portal ID.</param>
        /// <param name="data">The actual data for creating and sending the email.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        /// <returns>EmailTemplateSetting.</returns>
        public static SystemEmailTemplate CreatePortalEmailTemplateSetting(
            Guid tenantId,
            SystemEmailType type,
            Guid? portalId,
            SystemEmailTemplateData data,
            Instant createdTimestamp)
        {
            return new SystemEmailTemplate(
                tenantId,
                type,
                null,
                portalId,
                data,
                createdTimestamp);
        }

        /// <summary>
        /// Create a Tenant Email Template Setting.
        /// </summary>
        /// <param name="tenantId">The Tenant ID.</param>
        /// <param name="type">The type of email the template is for.</param>
        /// <param name="data">The actual data for creating and sending the email.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        /// <returns>EmailTemplateSetting.</returns>
        public static SystemEmailTemplate CreateTenantEmailTemplateSetting(
            Guid tenantId,
            SystemEmailType type,
            SystemEmailTemplateData data,
            Instant createdTimestamp)
        {
            return new SystemEmailTemplate(
                tenantId,
                type,
                null,
                null,
                data,
                createdTimestamp);
        }

        /// <summary>
        /// Update the settings.
        /// </summary>
        /// <param name="newData">The new data to update with.</param>
        public void Update(SystemEmailTemplateData newData)
        {
            this.Data = newData;
        }

        /// <summary>
        /// Enable Email Template.
        /// </summary>
        public void Enable()
        {
            this.Enabled = true;
        }

        /// <summary>
        /// Disable Email Template.
        /// </summary>
        public void Disable()
        {
            this.Enabled = false;
        }
    }
}
