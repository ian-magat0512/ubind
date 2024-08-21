// <copyright file="EmailRequestRecord.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using NodaTime;

    /// <summary>
    /// base class used for recording email requests.
    /// </summary>
    public abstract class EmailRequestRecord : Entity<Guid>, IEmailRequestRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailRequestRecord"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        public EmailRequestRecord()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailRequestRecord"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <param name="email">The email address.</param>
        /// <param name="clientIpAddress">The client IP address.</param>
        /// <param name="timestamp">The current timestamp.</param>
        protected EmailRequestRecord(
            Guid tenantId, Guid organisationId, string email, string clientIpAddress, Instant timestamp)
            : base(Guid.NewGuid(), timestamp)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.EmailAddress = email;
            this.ClientIpAddress = clientIpAddress;
        }

        /// <summary>
        /// Gets the tenant id the attempt was for.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets or sets the organisation alias used in the attempt.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the email used in the attempt.
        /// </summary>
        [MaxLength(320)]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets the IP address the attempt was made from.
        /// </summary>
        public string ClientIpAddress { get; private set; }
    }
}
