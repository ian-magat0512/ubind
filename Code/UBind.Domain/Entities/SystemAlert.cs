// <copyright file="SystemAlert.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// A uBind SystemAlert.
    /// </summary>
    public class SystemAlert : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemAlert"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the alert is for.</param>
        /// <param name="productId">The ID of the product the alert is for.</param>
        /// <param name="alertType">The type of alert.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        public SystemAlert(Guid tenantId, Guid? productId, SystemAlertType alertType, Instant createdTimestamp)
            : this(tenantId, alertType, createdTimestamp)
        {
            this.ProductId = productId;
            this.Type = alertType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemAlert"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the alert is for.</param>
        /// <param name="systemAlertType">The type of alert.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        public SystemAlert(Guid tenantId, SystemAlertType systemAlertType, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.TenantId = tenantId;
            this.Type = systemAlertType;
            this.Disabled = true;
        }

        // Parameterless constructor for EF.
        private SystemAlert()
            : base(default, default)
        {
        }

        /// <summary>
        /// Gets the SystemAlertType .
        /// </summary>
        public SystemAlertType Type { get; private set; }

        /// <summary>
        /// Gets the parent Tenant Id for the tenant system alert.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the Product Id.
        /// </summary>
        public Guid? ProductId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the system alert is disabled.
        /// </summary>
        public bool Disabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the SystemAlert is in WarningThreshold.
        /// </summary>
        public int? WarningThreshold { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the SystemAlert is in CriticalThreshold.
        /// </summary>
        public int? CriticalThreshold { get; private set; }

        /// <summary>
        /// Update this alert.
        /// </summary>
        /// <param name="warningThreshold">Warning Threshold.</param>
        /// <param name="criticalThreshold">Critical Threshold.</param>
        public void Update(int? warningThreshold, int? criticalThreshold)
        {
            this.WarningThreshold = warningThreshold;
            this.CriticalThreshold = criticalThreshold;
            this.Disabled = false;
        }

        /// <summary>
        /// Disable System Alert.
        /// </summary>
        public void DisableSystemAlert()
        {
            this.Disabled = true;
        }

        /// <summary>
        /// Enable System Alert.
        /// </summary>
        public void EnableSystemAlert()
        {
            this.Disabled = false;
        }

        /// <summary>
        /// Gets a value indicating whether this alert specifies a warning threshold.
        /// </summary>
        /// <returns>true, if a warning threshold is set, otherwise false.</returns>
        public bool HasWarningThreshold()
        {
            return this.WarningThreshold.HasValue;
        }

        /// <summary>
        /// Gets a value indicating whether this alert specifies a critical threshold.
        /// </summary>
        /// <returns>true, if a critical threshold is set, otherwise false.</returns>
        public bool HasCriticalThreshold()
        {
            return this.CriticalThreshold.HasValue;
        }

        /// <summary>
        /// Returns a value indicating whether the current count of a resource is at the warning threshold of this alert.
        /// </summary>
        /// <param name="currentCount">The current count.</param>
        /// <returns><c>true</c> if the current count is at the warning threshold, otherwise <c>false</c>.</returns>
        public bool IsAtWarningThreshold(int currentCount)
        {
            return currentCount == this.WarningThreshold;
        }

        /// <summary>
        /// Returns a value indicating whether the current count of a resource is at the critical threshold of this alert.
        /// </summary>
        /// <param name="currentCount">The current count.</param>
        /// <returns><c>true</c> if the current count is at the critical threshold, otherwise <c>false</c>.</returns>
        public bool IsAtOrBelowCriticalThreshold(int currentCount)
        {
            return currentCount <= this.CriticalThreshold;
        }
    }
}
