// <copyright file="QuoteEmailSendingReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using NodaTime;

    /// <summary>
    /// Stores the content of email sent.
    /// </summary>
    public class QuoteEmailSendingReadModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteEmailSendingReadModel"/> class.
        /// </summary>
        /// <param name="createdTimestamp">The time the quote email was created.</param>
        public QuoteEmailSendingReadModel(Instant createdTimestamp)
        {
            this.Id = Guid.NewGuid();
            this.CreatedTimestamp = createdTimestamp;
        }

        /// <summary>
        /// Gets the entity ID.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the quote email read model ID.
        /// </summary>
        public Guid QuoteEmailReadModelId { get; private set; }

        /// <summary>
        /// Gets or sets the created time as the number of ticks since the epoch for persistance in EF.
        /// </summary>
        public long CreatedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets the time the claim was created.
        /// </summary>
        public Instant CreatedTimestamp
        {
            get
            {
                return Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch);
            }

            private set
            {
                this.CreatedTicksSinceEpoch = value.ToUnixTimeTicks();
            }
        }
    }
}
