// <copyright file="TenantSessionSettingUpdateModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using Newtonsoft.Json;
    using UBind.Domain;

    /// <summary>
    /// A view model for a tenant sesson setting.
    /// </summary>
    public class TenantSessionSettingUpdateModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantSessionSettingUpdateModel"/> class.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        public TenantSessionSettingUpdateModel(Tenant tenant)
        {
            if (tenant != null)
            {
                this.SessionExpiryMode = tenant.Details.SessionExpiryMode.ToString();
                this.IdleTimeoutPeriodType = tenant?.Details?.IdleTimeoutPeriodType;
                this.FixLengthTimeoutInPeriodType = tenant?.Details?.FixLengthTimeoutInPeriodType ?? TokenSessionPeriodType.Day;

                if (tenant?.Details != null)
                {
                    this.IdleTimeout = tenant.Details.IdleTimeoutByPeriod();
                    this.FixLengthTimeout = tenant.Details.FixLengthTimeoutByPeriod();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantSessionSettingUpdateModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for JSON deserializer.
        /// </remarks>
        [JsonConstructor]
        public TenantSessionSettingUpdateModel()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the session should expiry after a period of
        /// inactivity, or after a fixed period.
        /// </summary>
        [JsonProperty]
        public string SessionExpiryMode { get; private set; }

        /// <summary>
        /// Gets the period type of the idle timeout value.
        /// </summary>
        [JsonProperty]
        public string IdleTimeoutPeriodType { get; private set; }

        /// <summary>
        /// Gets the period type of the fix length timeout value.
        /// </summary>
        [JsonProperty]
        public string FixLengthTimeoutInPeriodType { get; private set; }

        /// <summary>
        /// Gets the idle timeout in milliseconds for the specific tenant for security purposes.
        /// </summary>
        [JsonProperty]
        public long IdleTimeout { get; private set; }

        /// <summary>
        /// Gets the fix length timeout in milliseconds for the specific tenant for security purposes.
        /// </summary>
        [JsonProperty]
        public long FixLengthTimeout { get; private set; }
    }
}
