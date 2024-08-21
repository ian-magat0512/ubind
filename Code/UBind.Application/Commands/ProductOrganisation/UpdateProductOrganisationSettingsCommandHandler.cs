// <copyright file="UpdateProductOrganisationSettingsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.ProductOrganisation
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Command handler to enable/disable allow new quotes for products.
    /// </summary>
    public class UpdateProductOrganisationSettingsCommandHandler : ICommandHandler<UpdateProductOrganisationSettingsCommand, Unit>
    {
        private readonly IProductOrganisationSettingRepository productOrganisationSettingRepository;

        public UpdateProductOrganisationSettingsCommandHandler(
            IProductOrganisationSettingRepository productOrganisationSettingRepository)
        {
            this.productOrganisationSettingRepository = productOrganisationSettingRepository;
        }

        public async Task<Unit> Handle(UpdateProductOrganisationSettingsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.productOrganisationSettingRepository.UpdateProductSetting(
                request.TenantId, request.OrganisationId, request.ProductId, request.IsNewQuotesAllowed);
            return Unit.Value;
        }
    }
}
