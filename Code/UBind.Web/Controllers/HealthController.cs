// <copyright file="HealthController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using UBind.Application.Authorisation;
    using UBind.Application.Queries.LuceneIndex;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Search;
    using UBind.Web.Filters;

    /// <summary>
    /// Provides for health check of the application e.g lucene index.
    /// </summary>
    [Route("api/v1/health")]
    public class HealthController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;
        private readonly IClock clock;
        private readonly IAuthorisationService authorisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthController"/> class.
        /// </summary>
        /// <param name="cachingResolver">The tenant, product, organisation and portal resolver.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="authorisationService">The authorization service.</param>
        /// <param name="clock">The clock.</param>
        public HealthController(
            ICachingResolver cachingResolver,
            ICqrsMediator mediator,
            IAuthorisationService authorisationService,
            IClock clock)
        {
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
            this.authorisationService = authorisationService;
            this.clock = clock;
        }

        /// <summary>
        /// This endpoint for policy sanity check between dates so we can determine if there a problem on lucene index that has missing records.
        /// </summary>
        /// <param name="fromDateTime">The datetime From used for filtering, DateTime ISO-8601 format. Optional. Leave this empty to use the last 24 hours.</param>
        /// <param name="toDateTime">The datetime To used for filtering, DateTime ISO-8601 format. Optional. Leave this empty to use the last 24 hours.</param>
        /// <param name="tenant">The Id or Alias of the tenant. Omit this to sanity check all tenants.</param>
        /// <param name="environment">The environment. Omit this it will check on production environment.</param>
        /// <returns>Response the list of tenant repository and lucene index counts.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Route("policy-search-index")]
        public async Task<IActionResult> PolicySearchIndex(string fromDateTime, string toDateTime, string tenant, string environment)
        {
            this.authorisationService.ThrowIfNotValidSecretKeyAndNotMasterUser(this.User);

            DeploymentEnvironment env;
            IEnumerable<IEntityRepositoryAndLuceneIndexCountModel> policyRepositoryAndLuceneIndexResults;
            if (string.IsNullOrEmpty(environment))
            {
                env = DeploymentEnvironment.Production;
            }
            else
            {
                var isSuccess = Enum.TryParse(environment, true, out env);
                if (!isSuccess)
                {
                    return this.NotFound($"Environment \"{environment}\"not found");
                }
            }

            var now = this.clock.GetCurrentInstant();
            Instant fromDateTimeInstant = now.Minus(Duration.FromHours(24));
            Instant toDateTimeInstant = now.Minus(Duration.FromMinutes(5));

            if (!fromDateTime.IsNullOrEmpty())
            {
                fromDateTimeInstant = fromDateTime.ToInstantFromDateTimeIso8601();
            }

            if (!toDateTime.IsNullOrEmpty())
            {
                toDateTimeInstant = toDateTime.ToInstantFromDateTimeIso8601();
            }

            if (string.IsNullOrEmpty(tenant))
            {
                policyRepositoryAndLuceneIndexResults = await this.mediator.Send(new GetPolicyRepositoryAndLuceneIndexCountQuery(fromDateTimeInstant, toDateTimeInstant, env, null));
            }
            else
            {
                var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
                policyRepositoryAndLuceneIndexResults = await this.mediator.Send(new GetPolicyRepositoryAndLuceneIndexCountQuery(fromDateTimeInstant, toDateTimeInstant, env, tenantModel.Id));
            }

            return this.Ok(this.GenerateSearchIndexHealthResultMessage(policyRepositoryAndLuceneIndexResults, "policies"));
        }

        /// <summary>
        /// This endpoint for quote sanity check between dates so we can determine if there a problem on lucene index that has missing records.
        /// </summary>
        /// <param name="fromDateTime">The datetime From used for filtering,  DateTime ISO-8601 format. Optional. Leave this empty to use the last 24 hours.</param>
        /// <param name="toDateTime">The datetime To used for filtering,  DateTime ISO-8601 format. Optional. Leave this empty to use the last 24 hours.</param>
        /// <param name="tenant">The Id or Alias of the tenant. Omit this to sanity check all tenants.</param>
        /// <param name="environment">The environment. Omit this it will check on production environment.</param>
        /// <returns>Response the list of tenant repository and lucene index counts.
        /// Note: The max of lucene index count was only 5000. This was set initialy.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Route("quote-search-index")]
        public async Task<IActionResult> QuoteSearchIndex(string fromDateTime, string toDateTime, string tenant, string environment)
        {
            this.authorisationService.ThrowIfNotValidSecretKeyAndNotMasterUser(this.User);

            DeploymentEnvironment env;
            IEnumerable<IEntityRepositoryAndLuceneIndexCountModel> quoteRepositoryAndLuceneIndexResults;
            if (string.IsNullOrEmpty(environment))
            {
                env = DeploymentEnvironment.Production;
            }
            else
            {
                var isSuccess = Enum.TryParse(environment, true, out env);
                if (!isSuccess)
                {
                    return this.NotFound($"Environment \"{environment}\"not found");
                }
            }

            var now = this.clock.GetCurrentInstant();
            Instant fromDateTimeInstant = now.Minus(Duration.FromHours(24));
            Instant toDateTimeInstant = now.Minus(Duration.FromMinutes(5));

            if (!fromDateTime.IsNullOrEmpty())
            {
                fromDateTimeInstant = fromDateTime.ToInstantFromDateTimeIso8601();
            }

            if (!toDateTime.IsNullOrEmpty())
            {
                toDateTimeInstant = toDateTime.ToInstantFromDateTimeIso8601();
            }

            if (string.IsNullOrEmpty(tenant))
            {
                quoteRepositoryAndLuceneIndexResults = await this.mediator.Send(new GetQuoteRepositoryAndLuceneIndexCountQuery(fromDateTimeInstant, toDateTimeInstant, env, null));
            }
            else
            {
                var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
                quoteRepositoryAndLuceneIndexResults = await this.mediator.Send(new GetQuoteRepositoryAndLuceneIndexCountQuery(fromDateTimeInstant, toDateTimeInstant, env, tenantModel.Id));
            }

            return this.Ok(this.GenerateSearchIndexHealthResultMessage(quoteRepositoryAndLuceneIndexResults, "quotes"));
        }

        private string GenerateSearchIndexHealthResultMessage(IEnumerable<IEntityRepositoryAndLuceneIndexCountModel> entityRepositoryAndLuceneIndexResults, string entity)
        {
            if (entityRepositoryAndLuceneIndexResults != null && !entityRepositoryAndLuceneIndexResults.Where(p => p.RepositoryCount != p.LuceneIndexCount).Any())
            {
                return $"All tenants have the same number of {entity} in the database as in the index";
            }

            var message = new StringBuilder();
            foreach (var entityResult in entityRepositoryAndLuceneIndexResults.Where(p => p.RepositoryCount != p.LuceneIndexCount))
            {
                if (entityResult.RepositoryCount > entityResult.LuceneIndexCount)
                {
                    message.AppendLine($"The tenant {entityResult.Tenant} has {entityResult.RepositoryCount - entityResult.LuceneIndexCount} less {entity} in its index.");
                }
                else
                {
                    message.AppendLine($"The tenant {entityResult.Tenant} has {entityResult.LuceneIndexCount - entityResult.RepositoryCount} more {entity} in its index.");
                }
            }

            return message.ToString();
        }
    }
}
