// <copyright file="EmailAddressBlockingStatus.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Helper for tracking login failures and handling account locking rules.
    /// </summary>
    public class EmailAddressBlockingStatus
    {
        private readonly EmailAddressBlockingEvent latestEmailAddressBlockingEvent;
        private readonly IEnumerable<LoginAttemptResult> recentLoginAttemptResults;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddressBlockingStatus"/> class.
        /// </summary>
        /// <param name="lastFiveLoginAttempts">The last 5 login attempts, or as many as are available, if fewer than 5 exist.</param>
        /// <param name="latestEmailAddressBlockingEvent">The latest email address blocking event.</param>
        /// <param name="clock">A clock for obtianing the current time.</param>
        public EmailAddressBlockingStatus(
            IEnumerable<LoginAttemptResult> lastFiveLoginAttempts,
            EmailAddressBlockingEvent latestEmailAddressBlockingEvent,
            IClock clock)
        {
            this.recentLoginAttemptResults = lastFiveLoginAttempts;
            this.latestEmailAddressBlockingEvent = latestEmailAddressBlockingEvent;
            this.clock = clock;
        }

        /// <summary>
        /// Gets a value indicating whether the account is currently blocked.
        /// </summary>
        public bool IsEmailAddressBlocked
        {
            get
            {
                var isBlocked = this.latestEmailAddressBlockingEvent?.IsEmailAddressBlocked ?? false;

                if (isBlocked)
                {
                    // Get the time elapsed since the last Email Address block event, if greater than 30mins then set IsEmailAddressBlocked to false
                    var timeElapsedInMinutes = (this.clock.Now() - this.latestEmailAddressBlockingEvent.CreatedTimestamp).TotalMinutes;
                    isBlocked = timeElapsedInMinutes < 30;
                }

                return isBlocked;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Email Address should be locked on the next failed login.
        /// </summary>
        public bool ShouldEmailAddressBeBlockedOnNextFailedLogin
        {
            get
            {
                if (this.IsEmailAddressBlocked)
                {
                    // No need to block, as the email address is already blocked.
                    return false;
                }

                if (this.recentLoginAttemptResults.Count() < 5)
                {
                    // No need to block, as there have not been too many failed logins at all.
                    return false;
                }

                if (this.recentLoginAttemptResults.Any(r => r.Succeeded))
                {
                    // No need to block, as there have not been too many failed logins sincen the last successful login.
                    return false;
                }

                if (this.latestEmailAddressBlockingEvent != null &&
                    this.recentLoginAttemptResults.Any(r => r.CreatedTicksSinceEpoch < this.latestEmailAddressBlockingEvent.CreatedTicksSinceEpoch))
                {
                    // No need to block, as there have not been too many failed logins since the last unblock.
                    return false;
                }

                return true;
            }
        }
    }
}
