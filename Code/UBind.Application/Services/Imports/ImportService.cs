// <copyright file="ImportService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Imports
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Hangfire.Server;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Services.Imports.MappingObjects;
    using UBind.Domain.Imports;
    using UBind.Domain.Loggers;
    using UBind.Domain.Processing;
    using UBind.Domain.Product;

    /// <summary>
    /// Represents the service for import request.
    /// </summary>
    public class ImportService : IImportService
    {
        private readonly IJobClient jobClient;
        private readonly IProgressLoggerFactory progressLoggerFactory;
        private readonly IMappingTransactionService transactionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportService"/> class.
        /// </summary>
        /// <param name="client">Client for queuing background jobs.</param>
        /// <param name="factory">The progress logger factory.</param>
        /// <param name="trans">The mapping transaction service.</param>
        public ImportService(
            IJobClient client,
            IProgressLoggerFactory factory,
            IMappingTransactionService trans)
        {
            this.jobClient = client;
            this.progressLoggerFactory = factory;
            this.transactionService = trans;
        }

        /// <inheritdoc/>
        public void QueueBackgroundImport(ImportBaseParam baseParam, string importData, bool updateEnabled)
        {
            var jobId = this.jobClient.Enqueue<IImportService>(
                s => s.ImportHandler(baseParam, importData, updateEnabled, null),
                new ProductContext(
                    baseParam.TenantId,
                    baseParam.ProductId,
                    baseParam.Environment));
        }

        /// <inheritdoc/>
        public void ImportHandler(ImportBaseParam baseParam, string importDataJson, bool updateEnabled, PerformContext context)
        {
            IProgressLogger logger = this.progressLoggerFactory.Invoke(context);
            this.ThrowIfNull(logger);

            ImportData data = JsonConvert.DeserializeObject<ImportData>(importDataJson);
            ImportConfiguration config = JsonConvert.DeserializeObject<ImportConfiguration>(importDataJson);
            ImportDataContainer dataContainer = new ImportDataContainer(baseParam, data, config);

            var tasks = new List<Task>();
            var jobId = context.BackgroundJob.Id;
            foreach (JObject entry in data.Data)
            {
                CustomerImportData customerData = config.CustomerMapping != null ?
                    new CustomerImportData(dataContainer.TenantId, dataContainer.OrganisationId, entry, config.CustomerMapping) :
                    null;

                PolicyImportData policyData = config.PolicyMapping != null ?
                    new PolicyImportData(entry, config.PolicyMapping) :
                    null;

                ClaimImportData claimData = config.ClaimMapping != null ?
                    new ClaimImportData(entry, config.ClaimMapping)
                    : null;

                QuoteImportData quoteData = config.QuoteMapping != null ?
                    new QuoteImportData(entry, config.QuoteMapping) :
                    null;

                var requestBody = new ImportDataRequestModel(
                    dataContainer.TenantId,
                    dataContainer.OrganisationId,
                    dataContainer.ProductId,
                    dataContainer.Environment,
                    jobId,
                    customerData,
                    policyData,
                    claimData,
                    quoteData,
                    updateEnabled);
                var requestString = JsonConvert.SerializeObject(requestBody);
                jobId = this.jobClient.ContinueJobWith<IImportService>(requestBody.JobId, s => s.ImportPersistenceHandler(requestString, null), Hangfire.JobContinuationOptions.OnAnyFinishedState);
            }
        }

        /// <inheritdoc/>
        public async Task ImportPersistenceHandler(string json, PerformContext context)
        {
            IProgressLogger logger = this.progressLoggerFactory.Invoke(context);
            this.ThrowIfNull(logger);

            logger.Log(LogLevel.Information, $"Deserializing the current json object: {json}");

            var model = JsonConvert.DeserializeObject<ImportDataRequestModel>(json);
            var baseParam = new ImportBaseParam(
                model.TenantId, model.OrganisationId, model.ProductId, model.Environment);

            if (model.Customer != null)
            {
                await this.transactionService.HandleCustomers(logger, baseParam, model.Customer);
            }

            if (model.Policy != null)
            {
                await this.transactionService.HandlePolicies(logger, baseParam, model.Policy);
            }

            if (model.Claim != null)
            {
                await this.transactionService.HandleClaims(logger, baseParam, model.Claim);
            }

            if (model.Quote != null)
            {
                await this.transactionService.HandleQuotes(logger, baseParam, model.Quote);
            }
        }

        private void ThrowIfNull(IProgressLogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("System is expecting an instance of IProgressLogger.");
            }
        }
    }
}
