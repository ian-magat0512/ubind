// <copyright file="UpdateRefundSettingsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.Releases;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Services;

    /// <summary>
    /// Command handler for updating refund policy.
    /// </summary>
    public class UpdateRefundSettingsCommandHandler : ICommandHandler<UpdateRefundSettingsCommand, Unit>
    {
        private readonly IProductFeatureSettingRepository productFeatureRepository;
        private readonly IGlobalReleaseCache globalReleaseCache;
        private readonly IProductReleaseService productReleaseService;

        public UpdateRefundSettingsCommandHandler(
            IProductFeatureSettingRepository productFeatureRepository,
            IGlobalReleaseCache globalReleaseCache,
            IProductReleaseService productReleaseService)
        {
            this.productFeatureRepository = productFeatureRepository;
            this.globalReleaseCache = globalReleaseCache;
            this.productReleaseService = productReleaseService;
        }

        /// <inheritdoc/>
        public Task<Unit> Handle(UpdateRefundSettingsCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.productFeatureRepository.UpdateRefundSettings(
                command.TenantId,
                command.ProductId,
                command.RefundPolicy,
                command.PolicyPeriodWithNoClaimsRaised,
                command.LastNumberOfYears);
            this.globalReleaseCache.InvalidateCache(this.productReleaseService, command.TenantId, command.ProductId);
            return Task.FromResult(Unit.Value);
        }
    }
}
