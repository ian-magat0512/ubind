// <copyright file="UpdateDatabaseTableNameWhenProductAliasChangeDomainEventHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Notification.DataTable
{
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using UBind.Application.Services;
    using UBind.Domain.Notifications;

    /// <summary>
    /// When an product alias was changed, this handler will update the associated database table name.
    /// </summary>
    public class UpdateDatabaseTableNameWhenProductAliasChangeDomainEventHandler : IDomainEventHandler<ProductAliasChangeDomainEvent>
    {
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IDataTableDefinitionService dataTableDefinitionService;

        public UpdateDatabaseTableNameWhenProductAliasChangeDomainEventHandler(
            IBackgroundJobClient backgroundJobClient,
            IDataTableDefinitionService dataTableDefinitionService)
        {
            this.backgroundJobClient = backgroundJobClient;
            this.dataTableDefinitionService = dataTableDefinitionService;
        }

        public Task Handle(ProductAliasChangeDomainEvent notification, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (notification.OldProductAlias == notification.NewProductAlias)
            {
                return Task.CompletedTask;
            }
            this.backgroundJobClient.Enqueue(() =>
                 this.dataTableDefinitionService.RenameDataTableDefinitionDataTableName(
                    notification.TenantId, Domain.EntityType.Product, notification.ProductId, notification.NewProductAlias));
            return Task.CompletedTask;
        }
    }
}
