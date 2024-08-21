// <copyright file="ProductCreatedEventHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Product
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.Commands.ProductOrganisation;
    using UBind.Domain.Notifications;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
    {
        private readonly ICqrsMediator mediator;
        private readonly ITenantRepository tenantRepository;

        public ProductCreatedEventHandler(
            ICqrsMediator mediator,
            ITenantRepository tenantRepository)
        {
            this.mediator = mediator;
            this.tenantRepository = tenantRepository;
        }

        public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Enable new quotes for this product on the default organisation
            var tenant = this.tenantRepository.GetTenantById(notification.Product.TenantId);
            var command = new UpdateProductOrganisationSettingsCommand(tenant.Id, tenant.Details.DefaultOrganisationId, notification.Product.Id, true);
            await this.mediator.Send(command);
        }
    }
}
