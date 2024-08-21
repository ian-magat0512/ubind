// <copyright file="QuoteExpirySettings.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using UBind.Domain.Aggregates.Quote;

    /// <summary>
    /// Product quote expiry settings.
    /// </summary>
    public class QuoteExpirySettings : IQuoteExpirySettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteExpirySettings"/> class.
        /// </summary>
        /// <param name="expiryDays">Set expiry days.</param>
        /// <param name="enabled">Set enabled property.</param>
        public QuoteExpirySettings(int expiryDays, bool enabled = false)
        {
            if (enabled && expiryDays <= 0)
            {
                throw new ArgumentException("Expiry Days should be a positive value greater than 0.");
            }

            this.ExpiryDays = expiryDays;
            this.Enabled = enabled;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteExpirySettings"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// </remarks>
        private QuoteExpirySettings()
        {
        }

        /// <summary>
        /// Gets an instance of <see cref="QuoteExpirySettings"/> with expiry disabled.
        /// </summary>
        public static QuoteExpirySettings Default => new QuoteExpirySettings(30);

        /// <inheritdoc/>
        /// <remarks>The default expiry hours from start of day if not set.</remarks>
        public static int DefaultExpiryHoursFromStartOfDay => 17;

        /// <inheritdoc/>
        public bool Enabled { get; private set; }

        /// <inheritdoc/>
        public int ExpiryDays { get; private set; }

        public override bool Equals(object? obj)
        {
            // Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                QuoteExpirySettings q = (QuoteExpirySettings)obj;
                return (q.Enabled == this.Enabled) && (q.ExpiryDays == this.ExpiryDays);
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
