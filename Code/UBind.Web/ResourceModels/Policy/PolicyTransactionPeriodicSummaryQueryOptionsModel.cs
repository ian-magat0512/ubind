// <copyright file="PolicyTransactionPeriodicSummaryQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Application.Dashboard;
    using UBind.Application.Dashboard.Model;
    using UBind.Application.Helpers;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Search;

    /// <summary>
    /// Model used for the parameters of the GET request to the PolicyTransaction Periodic Summary endpoint.
    /// </summary>
    public class PolicyTransactionPeriodicSummaryQueryOptionsModel : BasePeriodicSummaryQueryOptionsModel, IPeriodicSummaryQueryOptionsModel
    {
        [FromQuery(Name = "policyTransactionType")]
        public IEnumerable<string>? PolicyTransactionTypes { get; set; }

        protected override HashSet<string> ValidIncludeProperties => PolicyTransactionPeriodicSummaryModel.IncludeProperties;

        /// <summary>
        /// Create filters from these options.
        /// </summary>
        /// <param name="contextTenantId">The context tenant Id.</param>
        /// <param name="cachingResolver">The caching resolver to resolve tenant/product/organisation.</param>
        /// <returns>Read model filters.</returns>
        public async Task<PolicyReadModelFilters> ToFilters(Guid contextTenantId, ICachingResolver cachingResolver)
        {
            await this.ConvertContextToGuid(contextTenantId, cachingResolver);
            var tenantId = this.TenantId ?? contextTenantId;
            var productIds = await this.GetValidProducts(contextTenantId, cachingResolver);
            this.AddAllPolicyTransactionTypesIfEmpty();

            var filter = new PolicyReadModelFilters
            {
                SearchTerms = this.SearchTerms ?? Enumerable.Empty<string>(),
                Sources = this.Sources,
                Statuses = this.Statuses ?? Enumerable.Empty<string>(),
                QuoteTypes = this.PolicyTransactionTypes ?? Enumerable.Empty<string>(),
                ProductIds = productIds.Any() ? productIds : Enumerable.Empty<Guid>(),
                IncludeTestData = this.IncludeTestData,
                PolicyNumber = this.PolicyNumber,
                ProductId = this.ProductId,
                TenantId = this.TenantId,
                OrganisationIds = this.OrganisationId.HasValue ? new List<Guid> { this.OrganisationId.Value } : Enumerable.Empty<Guid>(),
                Page = this.Page,
                PageSize = this.PageSize,
                Environment = EnvironmentHelper.ParseOptionalEnvironmentOrThrow(this.Environment),
                OwnerUserId = this.OwnerUserId,
                IsDiscarded = false,
                CustomerId = this.CustomerId,
                SortBy = !string.IsNullOrEmpty(this.SortBy) ? this.SortBy : nameof(IPolicySearchResultItemReadModel.LastModifiedTicksSinceEpoch),
                SortOrder = !string.IsNullOrEmpty(this.SortOrder.ToString())
                            ? (SortDirection)Enum.Parse(typeof(SortDirection), this.SortOrder.ToString())
                            : SortDirection.Descending,
            };

            filter.DateFilteringPropertyName = nameof(PolicyTransaction.CreatedTicksSinceEpoch);
            filter.DateIsAfterTicks = this.FromDateTime?.ToTicksFromExtendedISO8601InZone(this.Timezone);
            filter.DateIsBeforeTicks = this.ToDateTime?.ToTicksFromExtendedISO8601InZone(this.Timezone);
            return filter;
        }

        /// <summary>
        /// Check and validates the options included in the query.
        /// </summary>
        /// <exception cref="ErrorException">Throws an exception if any of the options are invalid.</exception>
        public override void ValidateQueryOptions()
        {
            base.ValidateQueryOptions();

            this.PolicyTransactionTypes = this.PolicyTransactionTypes ?? Enumerable.Empty<string>();
            var invalidPolicyTransactionTypes = new List<string>();
            foreach (var q in this.PolicyTransactionTypes)
            {
                var quoteType = QuoteType.NewBusiness;
                if (!Enum.TryParse<QuoteType>(q, true, out quoteType))
                {
                    invalidPolicyTransactionTypes.Add(q);
                }
            }

            var validPolicyTransactionTypes = Enum.GetValues(typeof(QuoteType))
                .Cast<QuoteType>().Select(x => x.ToString());
            if (invalidPolicyTransactionTypes.Any())
            {
                throw new ErrorException(
                    this.GetErrorMultipleValueParameterInvalid(
                        "policyTransactionType",
                        invalidPolicyTransactionTypes,
                        $"Each value for the \"policyTransactionType\" parameter must be one" +
                        $" of {this.FormatMultipleValuesString(validPolicyTransactionTypes)}."));
            }
        }

        protected override long GetNumberOfExpectedPeriods()
        {
            return new PolicyTransactionSummaryGeneratorFactory().GetNumberOfExpectedPeriods(
                this.SamplePeriodLength.Value,
                this.FromDateTime,
                this.ToDateTime,
                this.TimeZoneId,
                this.CustomSamplePeriodMinutes);
        }

        private void AddAllPolicyTransactionTypesIfEmpty()
        {
            if (this.PolicyTransactionTypes == null || !this.PolicyTransactionTypes.Any())
            {
                var listQuoteTypes = new List<string>();
                foreach (var e in Enum.GetValues(typeof(QuoteType)))
                {
                    listQuoteTypes.Add(((QuoteType)e).ToString());
                }

                this.PolicyTransactionTypes = listQuoteTypes;
            }
        }
    }
}