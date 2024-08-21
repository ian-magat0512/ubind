// <copyright file="ProductAliasChangeEventHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Notification.Product
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain.Notifications;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// An event handler which handles <see cref="ProductAliasChangeDomainEvent"/> event class.
    /// </summary>
    public class ProductAliasChangeEventHandler
        : INotificationHandler<ProductAliasChangeDomainEvent>
    {
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductAliasChangeEventHandler"/> class.
        /// </summary>
        /// <param name="mediator"><see cref="ICqrsMediator"/>.</param>
        public ProductAliasChangeEventHandler(ICqrsMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <inheritdoc/>
        public async Task Handle(ProductAliasChangeDomainEvent notification, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var renameCommand =
                new Commands.Product.RenameProductDirectoryCommand(
                       notification.TenantId,
                       notification.ProductId,
                       notification.OldProductAlias,
                       notification.NewProductAlias,
                       notification.PerformingUserId,
                       notification.Timestamp);
            await this.mediator.Send(renameCommand);
        }
    }
}
