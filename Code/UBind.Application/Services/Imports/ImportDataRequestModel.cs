// <copyright file="ImportDataRequestModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Imports
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Imports;

    /// <summary>
    /// Represents the import data request model.
    /// </summary>
    public class ImportDataRequestModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportDataRequestModel"/> class.
        /// </summary>
        /// <param name="tenantId">The target tenant ID.</param>
        /// <param name="organisationId">The target organisation ID.</param>
        /// <param name="productId">The target product ID.</param>
        /// <param name="env">The target deployment environment.</param>
        /// <param name="jobId">The hangfire job ID to use for enqueueing.</param>
        /// <param name="customer">Represents the customer import object.</param>
        /// <param name="policy">Represents the policy import object.</param>
        /// <param name="claim">Represents the claim import object.</param>
        /// <param name="updateEnabled">The tag if updates on import is allowed. The default value is false.</param>
        public ImportDataRequestModel(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment env,
            string jobId,
            CustomerImportData customer,
            PolicyImportData policy,
            ClaimImportData claim,
            QuoteImportData quote,
            bool updateEnabled = false)
        {
            this.JobId = jobId;
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.ProductId = productId;
            this.Environment = env;
            this.Customer = customer;
            this.Policy = policy;
            this.Claim = claim;
            this.Quote = quote;
            this.UpdateEnabled = updateEnabled;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportDataRequestModel"/> class.
        /// </summary>
        [JsonConstructor]
        public ImportDataRequestModel()
        {
        }

        /// <summary>
        /// Gets the import hangfire job ID.
        /// </summary>
        [JsonProperty]
        public string JobId { get; private set; }

        /// <summary>
        /// Gets the import target tenant ID.
        /// </summary>
        [JsonProperty]
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the import target organisation ID.
        /// </summary>
        [JsonProperty]
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the import target product ID.
        /// </summary>
        [JsonProperty]
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the update status is enabled.
        /// </summary>
        [JsonProperty]
        public bool UpdateEnabled { get; private set; }

        /// <summary>
        /// Gets the import target deployment environment.
        /// </summary>
        [JsonProperty]
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the customer import data object.
        /// </summary>
        [JsonProperty]
        public CustomerImportData Customer { get; private set; }

        /// <summary>
        /// Gets the policy import data object.
        /// </summary>
        [JsonProperty]
        public PolicyImportData Policy { get; private set; }

        /// <summary>
        /// Gets the claim import data object.
        /// </summary>
        [JsonProperty]
        public ClaimImportData Claim { get; private set; }

        [JsonProperty]
        public QuoteImportData Quote { get; private set; }
    }
}
