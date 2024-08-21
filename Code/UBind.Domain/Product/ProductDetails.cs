// <copyright file="ProductDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Product details.
    /// </summary>
    public class ProductDetails : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDetails"/> class.
        /// </summary>
        /// <param name="name">The product name.</param>
        /// <param name="alias">The products alias.</param>
        /// <param name="disabled">The product status.</param>
        /// <param name="deleted">If the product is deleted.</param>
        /// <param name="createdTimestamp">The current time.</param>
        /// <param name="deploymentSettings">the deployment settings.</param>
        /// <param name="productQuoteExpirySetting">the quote expiry date setting.</param>
        public ProductDetails(
            string name,
            string alias,
            bool disabled,
            bool deleted,
            Instant createdTimestamp,
            ProductDeploymentSetting deploymentSettings = null,
            QuoteExpirySettings productQuoteExpirySetting = null)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Name = name;
            this.Alias = alias;
            this.Deleted = deleted;
            this.Disabled = disabled;
            this.QuoteExpirySetting = productQuoteExpirySetting == null ? new QuoteExpirySettings(30) : productQuoteExpirySetting;

            if (this.QuoteExpirySetting.ExpiryDays < 0)
            {
                throw new InvalidOperationException("quote expiry settings should be greater than 0");
            }

            this.DeploymentSettingJSON = deploymentSettings == null ? null : JsonConvert.SerializeObject(deploymentSettings);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDetails"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        private ProductDetails()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the deployment settings json persisted on the db.
        /// </summary>
        public string DeploymentSettingJSON { get; private set; }

        /// <summary>
        /// Gets the deployment settings.
        /// </summary>
        public ProductDeploymentSetting DeploymentSetting
        {
            get
            {
                if (string.IsNullOrEmpty(this.DeploymentSettingJSON))
                {
                    return null;
                }

                return JsonConvert.DeserializeObject<ProductDeploymentSetting>(this.DeploymentSettingJSON);
            }
        }

        /// <summary>
        /// Gets the product name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the product alias.
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the product is disabled.
        /// </summary>
        public bool Disabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the product is deleted.
        /// </summary>
        public bool Deleted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether quote expiry settings are set.
        /// </summary>
        public QuoteExpirySettings QuoteExpirySetting { get; private set; }

        /// <summary>
        /// Updates the name to a new value.
        /// </summary>
        /// <param name="name">The name.</param>
        public void UpdateName(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Updates the alias to a new value.
        /// </summary>
        /// <param name="alias">The alias.</param>
        public void UpdateAlias(string alias)
        {
            this.Alias = alias;
        }
    }
}
