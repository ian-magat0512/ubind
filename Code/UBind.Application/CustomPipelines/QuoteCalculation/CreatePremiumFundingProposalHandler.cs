// <copyright file="CreatePremiumFundingProposalHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.CustomPipelines.QuoteCalculation
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using StackExchange.Profiling;
    using UBind.Application.Commands.QuoteCalculation;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// This class is needed to calculate the premium funding if necessary.
    /// </summary>
    /// <typeparam name="TRequest">The command request.</typeparam>
    /// <typeparam name="TResponse">The command response.</typeparam>
    public class CreatePremiumFundingProposalHandler<TRequest, TResponse>
        : IPipelineBehavior<QuoteCalculationCommand, CalculationResponseModel>
        where TRequest : QuoteCalculationCommand
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ILogger<QuoteCalculationCommand> logger;
        private readonly IApplicationFundingService applicationFundingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePremiumFundingProposalHandler{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="logger">The file logger.</param>
        /// <param name="applicationFundingService">Service for accessing premium funding functionality.</param>
        public CreatePremiumFundingProposalHandler(
            ILogger<QuoteCalculationCommand> logger,
            IApplicationFundingService applicationFundingService,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.logger = logger;
            this.applicationFundingService = applicationFundingService;
        }

        /// <inheritdoc/>
        public async Task<CalculationResponseModel> Handle(
            QuoteCalculationCommand request,
            RequestHandlerDelegate<CalculationResponseModel> next,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Premium funding is only supported for new business and renewal quotes
            var requestNeedsFundingProposal =
                (request.QuoteType == QuoteType.NewBusiness || request.QuoteType == QuoteType.Renewal)
                && request.HasFundingProposal;
            if (requestNeedsFundingProposal)
            {
                request.FundingProposal = await this.TryGetFundingProposal(request, cancellationToken);
            }

            return await next();
        }

        private async Task<FundingProposal?> TryGetFundingProposal(QuoteCalculationCommand request, CancellationToken cancellationToken)
        {
            using (MiniProfiler.Current.Step("CreatePremiumFundingProposalHandler." + nameof(this.TryGetFundingProposal)))
            {
                var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(request.ProductContext.TenantId);
                var fundingService = await this.applicationFundingService.GetFundingService(request.ReleaseContext);
                if (fundingService == null
                    || !fundingService.PricingSupported
                    || request.FinalFormData == null
                    || request.CalculationResultData == null
                    || request.PriceBreakdown == null)
                {
                    return null;
                }

                if (request.Quote?.LatestFundingProposalCreationResult?.IsSuccess == true)
                {
                    return await fundingService.UpdateFundingProposal(
                       request.ProductContext,
                       request.Quote?.LatestFundingProposalCreationResult.Proposal.ExternalId,
                       request.FinalFormData,
                       request.CalculationResultData,
                       request.PriceBreakdown,
                       request.Quote,
                       request.PaymentData,
                       request.IsTestData,
                       cancellationToken);
                }

                if (request.PriceBreakdown.TotalPayable <= 0 || cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                return await fundingService.CreateFundingProposal(
                        request.ProductContext,
                        request.FinalFormData,
                        request.CalculationResultData,
                        request.PriceBreakdown,
                        request.Quote,
                        request.PaymentData,
                        request.IsTestData,
                        cancellationToken);
            }
        }
    }
}
