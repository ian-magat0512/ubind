// <copyright file="Sms.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    using System;
    using NodaTime;
    using UBind.Domain.Entities;

    public class Sms : Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sms"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="organisationId">The orgranisation id.</param>
        /// <param name="productId">The product id.</param>
        /// <param name="id">The id.</param>
        /// <param name="to">The sms recipient.</param>
        /// <param name="from">The sms sender.</param>
        /// <param name="message">The sms content.</param>
        /// <param name="createdTimestamp">The timestamp.</param>
        public Sms(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            Guid id,
            string to,
            string from,
            string message,
            Instant createdTimestamp)
            : base(id, createdTimestamp)
        {
            this.To = to;
            this.From = from;
            this.Message = message;
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.OrganisationId = organisationId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sms"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        public Sms()
            : base(default, default)
        {
        }

        public string To { get; private set; }

        public string From { get; private set; }

        public string Message { get; private set; }

        public Guid TenantId { get; private set; }

        public Guid ProductId { get; private set; }

        public Guid OrganisationId { get; private set; }
    }
}
