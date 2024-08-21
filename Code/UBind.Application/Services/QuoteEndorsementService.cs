// <copyright file="QuoteEndorsementService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Linq;
    using System.Security.Authentication;
    using System.Threading.Tasks;
    using Humanizer;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.Services;
    using UBind.Domain.ValueTypes;

    /// <inheritdoc/>
    public class QuoteEndorsementService : IQuoteEndorsementService
    {
        private readonly IRoleService roleService;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly IProductFeatureSettingService productFeatureSettingService;
        private readonly IQuoteWorkflowProvider quoteWorkflowProvider;
        private readonly IPolicyService policyService;

        public QuoteEndorsementService(
            IRoleService roleService,
            IUserAggregateRepository userAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            IProductFeatureSettingService productFeatureSettingService,
            IQuoteWorkflowProvider quoteWorkflowProvider,
            ICachingResolver cachingResolver,
            IPolicyService policyService)
        {
            this.cachingResolver = cachingResolver;
            this.clock = clock;
            this.userAggregateRepository = userAggregateRepository;
            this.roleService = roleService;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.productFeatureSettingService = productFeatureSettingService;
            this.quoteWorkflowProvider = quoteWorkflowProvider;
            this.policyService = policyService;
        }

        /// <inheritdoc/>
        public async Task AutoApproveQuote(ReleaseContext releaseContext, Quote quote, FormData? formData)
        {
            await this.AssignQuoteNumberIfNoneHasBeenAssigned(releaseContext, quote);
            IQuoteWorkflow quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
            if (quote.Type == QuoteType.Cancellation || quote.Type == QuoteType.Adjustment)
            {
                this.ThrowIfManualApprovalRequired(releaseContext.TenantId, releaseContext.ProductId, quote);
            }

            if (formData != null)
            {
                quote.UpdateFormData(formData, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            }

            quote.AutoApproveQuote(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now(), quoteWorkflow);
        }

        /// <inheritdoc/>
        public async Task ApproveReviewedQuote(ReleaseContext releaseContext, Quote quote, FormData? formData)
        {
            IQuoteWorkflow quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
            if (formData != null)
            {
                quote.UpdateFormData(formData, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            }

            quote.ApproveReviewedQuote(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now(), quoteWorkflow);
        }

        /// <inheritdoc/>
        public async Task DeclineQuote(ReleaseContext releaseContext, Quote quote, FormData? formData)
        {
            await this.AssignQuoteNumberIfNoneHasBeenAssigned(releaseContext, quote);
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(releaseContext.TenantId);
            var isMutual = TenantHelper.IsMutual(tenantAlias);
            IQuoteWorkflow quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
            if (formData != null)
            {
                quote.UpdateFormData(formData, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            }

            quote.DeclineQuote(
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.Now(),
                quoteWorkflow,
                isMutual);
        }

        /// <inheritdoc/>
        public async Task ReferQuoteForEndorsement(ReleaseContext releaseContext, Quote quote, FormData? formData)
        {
            await this.AssignQuoteNumberIfNoneHasBeenAssigned(releaseContext, quote);
            bool hasPermission = false;
            UserAggregate user = null;
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            if (performingUserId.HasValue)
            {
                user = this.userAggregateRepository.GetById(releaseContext.TenantId, performingUserId.Value);
            }

            if (user != null)
            {
                foreach (var role in user.RoleIds)
                {
                    var permissions = this.roleService.GetRole(releaseContext.TenantId, role);
                    if (permissions.Permissions.Any(x => x.Equals(Permission.ReviewQuotes)))
                    {
                        hasPermission = true;
                        break;
                    }
                }
            }

            var calculationResult = quote.LatestCalculationResult.Data;
            if (calculationResult.HasReviewCalculationTriggers && !hasPermission)
            {
                throw new AuthenticationException($"Aggregate has a review trigger active and user does not have the correct permissions to refer quote");
            }

            if (formData != null)
            {
                quote.UpdateFormData(formData, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            }

            IQuoteWorkflow quoteWorkflow =
                await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
            quote.ReferEndorsementQuote(
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.Now(),
                quoteWorkflow);
        }

        /// <inheritdoc/>
        public async Task ApproveEndorsedQuote(ReleaseContext releaseContext, Quote quote, FormData? formData)
        {
            await this.AssignQuoteNumberIfNoneHasBeenAssigned(releaseContext, quote);
            if (formData != null)
            {
                quote.UpdateFormData(formData, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            }

            IQuoteWorkflow quoteWorkflow =
                await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
            quote.ApproveEndorsementQuote(
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.Now(),
                quoteWorkflow);
        }

        /// <inheritdoc/>
        public async Task ReturnQuote(ReleaseContext releaseContext, Quote quote, FormData? formData)
        {
            var quoteState = quote.QuoteStatus.Humanize();
            if (quoteState == StandardQuoteStates.Review.Humanize()
                || quoteState == StandardQuoteStates.Endorsement.Humanize()
                || quoteState == StandardQuoteStates.Approved.Humanize())
            {
                if (formData != null)
                {
                    quote.UpdateFormData(formData, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                }

                IQuoteWorkflow quoteWorkflow =
                    await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
                quote.ReturnQuote(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now(), quoteWorkflow);
            }
            else
            {
                var error = Domain.Errors.Quote.ReturnQuoteInvalidQuoteState(quote.Id, quoteState);
                throw new ErrorException(error);
            }
        }

        /// <inheritdoc/>
        public async Task ReferQuoteForReview(ReleaseContext releaseContext, Quote quote, FormData? formData)
        {
            await this.AssignQuoteNumberIfNoneHasBeenAssigned(releaseContext, quote);
            if (formData != null)
            {
                quote.UpdateFormData(formData, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            }

            IQuoteWorkflow quoteWorkflow =
                await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
            quote.ReferQuoteForReview(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now(), quoteWorkflow);
        }

        private void ThrowIfManualApprovalRequired(Guid tenantId, Guid productId, Quote quote)
        {
            if (quote.LatestCalculationResult == null)
            {
                throw new ErrorException(Errors.Quote.CannotAutoApproveQuoteWithoutCalculation(quote.Id));
            }

            var hasRefund = quote.LatestCalculationResult.Data.RefundBreakdown.TotalPayable > 0;
            if (hasRefund)
            {
                var productFeature = this.productFeatureSettingService.GetProductFeature(tenantId, productId);
                if (productFeature.RefundRule == RefundRule.RefundsCanOptionallyBeProvided)
                {
                    if (quote.Type == QuoteType.Cancellation)
                    {
                        throw new ErrorException(Errors.Policy.Cancellation.ManualApprovalRequired());
                    }
                    else if (quote.Type == QuoteType.Adjustment)
                    {
                        throw new ErrorException(Errors.Policy.Adjustment.ManualApprovalRequired());
                    }
                }
            }
        }

        private async Task AssignQuoteNumberIfNoneHasBeenAssigned(ReleaseContext releaseContext, Quote quote)
        {
            if (quote.QuoteNumber == null)
            {
                string newQuoteNumber = await this.policyService.GenerateQuoteNumber(releaseContext);
                quote.AssignQuoteNumber(
                    newQuoteNumber, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            }
        }
    }
}
