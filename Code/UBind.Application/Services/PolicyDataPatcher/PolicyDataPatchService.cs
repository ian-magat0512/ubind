// <copyright file="PolicyDataPatchService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.PolicyDataPatcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Flurl.Http;
    using Hangfire;
    using Hangfire.Server;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Application.Services.Imports;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Commands;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Loggers;
    using UBind.Domain.ReadModel;

    /// <inheritdoc/>
    public class PolicyDataPatchService : IPatchService
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IInternalUrlConfiguration internalUrlConfig;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly IBackgroundJobClient jobClient;
        private readonly IProgressLoggerFactory progressLoggerFactory;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly ILogger<PolicyDataPatchService> logger;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyDataPatchService"/> class.
        /// </summary>
        /// <param name="internalUrlConfig">The internal URL configuration.</param>
        /// <param name="quoteAggregateRepository">Repository for quote aggregates.</param>
        /// <param name="policyReadModelRepository">Repository for policy read models.</param>
        /// <param name="jobClient">The hangfire background job client.</param>
        /// <param name="progressLoggerFactory">The progress logger factory.</param>
        /// <param name="httpContextPropertiesResolver">The current user Identification.</param>
        /// <param name="logger">The logging service.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="clock">Represents a clock that can return the current time as instant.</param>
        public PolicyDataPatchService(
            IInternalUrlConfiguration internalUrlConfig,
            IQuoteAggregateRepository quoteAggregateRepository,
            IPolicyReadModelRepository policyReadModelRepository,
            IBackgroundJobClient jobClient,
            IProgressLoggerFactory progressLoggerFactory,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ILogger<PolicyDataPatchService> logger,
            ICachingResolver cachingResolver,
            IClock clock)
        {
            Contract.Assert(internalUrlConfig != null);
            Contract.Assert(quoteAggregateRepository != null);
            Contract.Assert(policyReadModelRepository != null);
            Contract.Assert(jobClient != null);
            Contract.Assert(progressLoggerFactory != null);
            Contract.Assert(logger != null);
            Contract.Assert(clock != null);

            this.cachingResolver = cachingResolver;
            this.internalUrlConfig = internalUrlConfig;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.policyReadModelRepository = policyReadModelRepository;
            this.jobClient = jobClient;
            this.progressLoggerFactory = progressLoggerFactory;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.logger = logger;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Result> PatchPolicyDataAsync(Guid tenantId, Guid aggregateId, PolicyDataPatchCommand command)
        {
            async Task<Result> PatchQuoteAggregateAsync()
            {
                var quoteAggregate = this.quoteAggregateRepository.GetById(tenantId, aggregateId);
                if (quoteAggregate == null)
                {
                    throw new NotFoundException(
                        Errors.General.NotFound("quote", aggregateId));
                }

                var result = quoteAggregate.PatchFormData(command, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                if (result.IsSuccess)
                {
                    await this.quoteAggregateRepository.Save(quoteAggregate);
                    return Result.Success();
                }

                return Result.Failure(result.Error);
            }

            return await ConcurrencyPolicy.ExecuteWithRetriesAsync(PatchQuoteAggregateAsync);
        }

        /// <inheritdoc/>
        public void QueuePatchProductFormData(
            Guid tenantId,
            DeploymentEnvironment env,
            Guid organisationId,
            Guid productId,
            PolicyDataPatchCommandModel model)
        {
            this.jobClient.Enqueue<IPatchService>(s => s.PatchProductFormData(
                tenantId, env, organisationId, productId, model, null));
        }

        /// <inheritdoc/>
        public async Task PatchProductFormData(
            Guid tenantId,
            DeploymentEnvironment env,
            Guid organisationId,
            Guid productId,
            PolicyDataPatchCommandModel model,
            PerformContext context)
        {
            var tasks = new List<Task>();
            var filters = new PolicyReadModelFilters();
            filters.OrganisationIds = new Guid[] { organisationId };
            filters.ProductId = productId;
            filters.Environment = env;
            filters.Page = 1;
            filters.PageSize = 1000;
            var policyIds = this.policyReadModelRepository.ListPolicyIds(
                tenantId, filters);

            foreach (var policyId in policyIds)
            {
                tasks.Add(DoRequest(policyId));
            }

            await Task.WhenAll(tasks);

            async Task DoRequest(Guid policyId)
            {
                Contract.Assert(context != null);
                IProgressLogger logger = this.progressLoggerFactory.Invoke(context);

                var requestUrl = this.internalUrlConfig.BaseApi + $"/api/v1/policydata/{policyId}";

                try
                {
                    await requestUrl.PatchJsonAsync(model);
                }
                catch (FlurlHttpException ex)
                {
                    if (ex.Call.Completed)
                    {
                        var error = await ex.GetResponseStringAsync();
                        logger.Log(LogLevel.Error, $"Error: {error}");
                        this.logger.LogError($"Error: {error}");
                    }
                }
            }
        }
    }
}
