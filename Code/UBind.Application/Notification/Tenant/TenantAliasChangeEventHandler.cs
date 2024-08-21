// <copyright file="TenantAliasChangeEventHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Notification.Tenant
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.Services;
    using UBind.Application.Services.Search;
    using UBind.Domain;
    using UBind.Domain.Notification;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Search;

    /// <summary>
    /// Handlers event <see cref="TenantAliasChangeEventHandler"/>.
    /// </summary>
    public class TenantAliasChangeEventHandler : INotificationHandler<TenantAliasChangeDomainEvent>
    {
        private readonly ISearchableEntityService<IQuoteSearchResultItemReadModel, QuoteReadModelFilters> quoteSearchResultEntityService;
        private readonly ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters> policySearchResultEntityService;
        private readonly ICqrsMediator mediator;
        private readonly IUserSessionDeletionService userSessionDeletionService;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantAliasChangeEventHandler"/> class.
        /// </summary>
        /// <param name="mediator"><see cref="ICqrsMediator"/>.</param>
        public TenantAliasChangeEventHandler(
            ICqrsMediator mediator,
            ISearchableEntityService<IQuoteSearchResultItemReadModel, QuoteReadModelFilters> quoteSearchResultEntityService,
            ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters> policySearchResultEntityService,
            IUserSessionDeletionService userSessionDeletionService,
            ICachingResolver cachingResolver)
        {
            this.quoteSearchResultEntityService = quoteSearchResultEntityService;
            this.policySearchResultEntityService = policySearchResultEntityService;
            this.mediator = mediator;
            this.userSessionDeletionService = userSessionDeletionService;
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public async Task Handle(TenantAliasChangeDomainEvent notification, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.userSessionDeletionService.DeleteByTenant(notification.TenantId, cancellationToken);

            var renameCommand =
                new Commands.Tenant.RenameProductDirectoryCommand(
                    notification.TenantId,
                    notification.OldTenantAlias,
                    notification.NewTenantAlias,
                    notification.PerformingUserId,
                    notification.Timestamp);
            await this.mediator.Send(renameCommand);
            var activeProducts = await this.cachingResolver.GetActiveProducts();
            var tenant = await this.cachingResolver.GetTenantOrThrow(notification.TenantId);
            this.quoteSearchResultEntityService.RegenerateLuceneIndexesForTenant(
                tenant,
                activeProducts,
                cancellationToken);
            this.policySearchResultEntityService.RegenerateLuceneIndexesForTenant(
                tenant,
                activeProducts,
                cancellationToken);
        }
    }
}
