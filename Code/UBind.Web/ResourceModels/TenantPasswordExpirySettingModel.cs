// <copyright file="TenantPasswordExpirySettingModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using Newtonsoft.Json;
    using UBind.Domain;

    public class TenantPasswordExpirySettingModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantPasswordExpirySettingModel"/> class.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        public TenantPasswordExpirySettingModel(Tenant tenant)
        {
            if (tenant?.Details != null)
            {
                this.PasswordExpiryEnabled = tenant.Details.PasswordExpiryEnabled;
                this.MaxPasswordAgeDays = tenant.Details.MaxPasswordAgeDays;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantPasswordExpirySettingModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for JSON deserializer.
        /// </remarks>
        [JsonConstructor]
        public TenantPasswordExpirySettingModel()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the password should expiry after a period of
        /// inactivity, or after a fixed period.
        /// </summary>
        [JsonProperty]
        public bool PasswordExpiryEnabled { get; private set; }

        /// <summary>
        /// Gets the period type of the idle timeout value.
        /// </summary>
        [JsonProperty]
        public decimal MaxPasswordAgeDays { get; private set; }
    }
}
