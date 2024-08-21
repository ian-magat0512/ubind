// <copyright file="PolicyViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using Humanizer;
    using NodaTime;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Policy view model for Razor Templates to use.
    /// </summary>
    public class PolicyViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyViewModel"/> class.
        /// </summary>
        /// <param name="policy">The policy to present.</param>
        /// <param name="clock">Clock for obtaining current time.</param>
        public PolicyViewModel(Domain.Aggregates.Quote.Policy policy, IClock clock)
        {
            var inceptionTimestamp = policy.InceptionTimestamp;
            var expiryTimestamp = policy.ExpiryTimestamp;
            var cancellationTimestamp = policy.CancellationEffectiveTimestamp;
            var createdTimestamp = policy.CreatedTimestamp;

            this.IsCancelled = policy.IsCancelled.ToString() == "True" ? "Yes" : "No";
            this.CancellationTime = cancellationTimestamp?.To12HourClockTimeInAet();
            this.CancellationDate = cancellationTimestamp?.ToRfc5322DateStringInAet();
            this.Number = policy.PolicyNumber;
            this.Id = policy.PolicyId.ToString();
            this.CreatedDate = createdTimestamp.ToRfc5322DateStringInAet();
            this.CreatedTime = createdTimestamp.To12HourClockTimeInAet();
            this.InceptionDate = inceptionTimestamp.ToRfc5322DateStringInAet();
            this.InceptionTime = inceptionTimestamp.To12HourClockTimeInAet();
            this.ExpiryDate = expiryTimestamp?.ToRfc5322DateStringInAet();
            this.ExpiryTime = expiryTimestamp?.To12HourClockTimeInAet();
            this.Status = policy.GetPolicyStatus(clock.Now()).Humanize();
        }

        /// <summary>
        /// Gets the policy Id.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the policy number.
        /// </summary>
        public string Number { get; private set; }

        /// <summary>
        /// Gets the created date.
        /// </summary>
        public string CreatedDate { get; private set; }

        /// <summary>
        /// Gets the policy created time.
        /// </summary>
        public string CreatedTime { get; private set; }

        /// <summary>
        /// Gets the created date.
        /// </summary>
        public string CreationDate => this.CreatedDate;

        /// <summary>
        /// Gets the policy created time.
        /// </summary>
        public string CreationTime => this.CreatedTime;

        /// <summary>
        /// Gets the inception date.
        /// </summary>
        public string InceptionDate { get; private set; }

        /// <summary>
        /// Gets the cancellation date.
        /// </summary>
        public string CancellationDate { get; private set; }

        /// <summary>
        /// Gets the inception time.
        /// </summary>
        public string InceptionTime { get; private set; }

        /// <summary>
        /// Gets the policy expiration date.
        /// </summary>
        public string ExpiryDate { get; private set; }

        /// <summary>
        /// Gets the policy expiration date.
        /// </summary>
        public string ExpiryTime { get; private set; }

        /// <summary>
        /// Gets the policy cancellation time.
        /// </summary>
        public string CancellationTime { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the policy has been cancelled.
        /// </summary>
        public string IsCancelled { get; private set; }

        /// <summary>
        /// Gets the policy status.
        /// </summary>
        public string Status { get; private set; }
    }
}
