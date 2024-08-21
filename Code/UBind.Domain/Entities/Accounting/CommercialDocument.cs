// <copyright file="CommercialDocument.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Accounting
{
    using System;
    using NodaTime;

    /// <inheritdoc/>
    public abstract class CommercialDocument : Entity<Guid>, ICommercialDocument<Guid>, IReadModel<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommercialDocument"/> class.
        /// </summary>
        /// <param name="createdTimestamp">The created time of this commercial document.</param>
        /// <param name="number">The reference number.</param>
        /// <param name="dueDateTime">The commercial document due date.</param>
        /// <param name="breakdownId">The commercial document breakdown.</param>
        protected CommercialDocument(
            Guid tenantId,
            Instant createdTimestamp,
            IReferenceNumber number,
            Instant dueDateTime,
            Guid? breakdownId)
              : base(Guid.NewGuid(), createdTimestamp)
        {
            this.TenantId = tenantId;
            this.ReferenceNumber = number;
            this.DueTimestamp = dueDateTime;
            this.BreakdownId = breakdownId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommercialDocument"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        protected CommercialDocument()
        {
        }

        /// <inheritdoc/>
        public Guid? BreakdownId { get; private set; }

        /// <inheritdoc/>
        public IReferenceNumber ReferenceNumber { get; private set; }

        /// <inheritdoc/>
        public Instant DueTimestamp
        {
            get { return Instant.FromUnixTimeTicks(this.DueTicksSinceEpoch); }
            private set { this.DueTicksSinceEpoch = value.ToUnixTimeTicks(); }
        }

        /// <summary>
        /// Gets the due date time (in ticks since Epoch).
        /// </summary>
        /// <remarks> Primitive typed property for EF to store due date time.</remarks>
        public long DueTicksSinceEpoch { get; private set; }

        /// <inheritdoc/>
        public Guid TenantId { get; private set; }
    }
}
