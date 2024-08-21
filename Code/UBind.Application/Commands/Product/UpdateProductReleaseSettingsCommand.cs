// <copyright file="UpdateProductReleaseSettingsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Product
{
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    public class UpdateProductReleaseSettingsCommand : ICommand<Unit>
    {
        public UpdateProductReleaseSettingsCommand(
            Guid tenantId,
            Guid productId,
            bool doesAdjustmentUseDefaultProductRelease,
            bool doesCancellationUseDefaultProductRelease)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.DoesAdjustmentUseDefaultProductRelease = doesAdjustmentUseDefaultProductRelease;
            this.DoesCancellationUseDefaultProductRelease = doesCancellationUseDefaultProductRelease;
        }

        public Guid TenantId { get; private set; }

        public Guid ProductId { get; private set; }

        public bool DoesAdjustmentUseDefaultProductRelease { get; private set; }

        public bool DoesCancellationUseDefaultProductRelease { get; private set; }
    }
}
