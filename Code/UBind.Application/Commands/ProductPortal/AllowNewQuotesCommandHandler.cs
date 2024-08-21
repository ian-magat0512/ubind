// <copyright file="AllowNewQuotesCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.ProductPortal
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Command handler to enable/disable allow new quotes for products.
    /// </summary>
    public class AllowNewQuotesCommandHandler : ICommandHandler<AllowNewQuotesCommand, Unit>
    {
        private readonly IProductPortalSettingRepository productPortalSettingRepository;

        public AllowNewQuotesCommandHandler(
            IProductPortalSettingRepository productPortalSettingRepository)
        {
            this.productPortalSettingRepository = productPortalSettingRepository;
        }

        public Task<Unit> Handle(AllowNewQuotesCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.productPortalSettingRepository.AddOrUpdateProductSetting(
                request.TenantId, request.PortalId, request.ProductId, request.IsNewQuotesAllowed);

            return Task.FromResult(Unit.Value);
        }
    }
}
