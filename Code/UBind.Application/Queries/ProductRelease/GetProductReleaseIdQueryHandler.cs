// <copyright file="GetProductReleaseIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ProductRelease
{
    using UBind.Application.Queries.Product;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class GetProductReleaseIdQueryHandler : IQueryHandler<GetProductReleaseIdQuery, Guid>
    {
        private readonly IReleaseQueryService releaseQueryService;
        private readonly ICqrsMediator mediator;
        private readonly IPolicyReadModelRepository policyReadModelRepository;

        public GetProductReleaseIdQueryHandler(
            IReleaseQueryService releaseQueryService,
            ICqrsMediator mediator,
            IPolicyReadModelRepository policyReadModelRepository)
        {
            this.releaseQueryService = releaseQueryService;
            this.mediator = mediator;
            this.policyReadModelRepository = policyReadModelRepository;
        }

        public async Task<Guid> Handle(GetProductReleaseIdQuery request, CancellationToken cancellationToken)
        {
            var productReleaseSettings
                = await this.mediator.Send(new GetProductReleaseSettingsQuery(request.TenantId, request.ProductId));
            if (request.Environment != DeploymentEnvironment.Development
                && request.PolicyId != null
                && ((request.QuoteType == QuoteType.Adjustment
                && !productReleaseSettings.DoesAdjustmentUseDefaultProductRelease)
                    || (request.QuoteType == QuoteType.Cancellation
                       && !productReleaseSettings.DoesCancellationUseDefaultProductRelease)))
            {
                Guid? releaseId = this.policyReadModelRepository.GetProductReleaseIdForLatestPolicyPeriodTransaction(
                        request.TenantId,
                        request.ProductId,
                        request.PolicyId.Value,
                        request.Environment);
                if (releaseId != null && releaseId.HasValue)
                {
                    return releaseId.Value;
                }
            }

            return this.releaseQueryService.GetDefaultProductReleaseIdOrThrow(
                request.TenantId,
                request.ProductId,
                request.Environment);
        }
    }
}
