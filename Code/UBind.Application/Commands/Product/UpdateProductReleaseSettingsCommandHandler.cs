// <copyright file="UpdateProductReleaseSettingsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Product
{
    using MediatR;
    using UBind.Domain;
    using UBind.Domain.Models;
    using UBind.Domain.Patterns.Cqrs;

    public class UpdateProductReleaseSettingsCommandHandler
        : ICommandHandler<UpdateProductReleaseSettingsCommand, Unit>
    {
        private readonly IEntitySettingsRepository entitySettingRepository;

        public UpdateProductReleaseSettingsCommandHandler(
            IEntitySettingsRepository entitySettingRepository)
        {
            this.entitySettingRepository = entitySettingRepository;
        }

        public Task<Unit> Handle(UpdateProductReleaseSettingsCommand request, CancellationToken cancellationToken)
        {
            var productReleaseSettings = this.entitySettingRepository
                .GetEntitySettings<ProductReleaseEntitySettings>(request.TenantId, EntityType.Product, request.ProductId);
            productReleaseSettings.DoesAdjustmentUseDefaultProductRelease = request.DoesAdjustmentUseDefaultProductRelease;
            productReleaseSettings.DoesCancellationUseDefaultProductRelease = request.DoesCancellationUseDefaultProductRelease;
            this.entitySettingRepository.AddOrUpdateEntitySettings(
                request.TenantId, EntityType.Product, request.ProductId, productReleaseSettings);
            return Task.FromResult(Unit.Value);
        }
    }
}
