// <copyright file="UpdateProductOrganisationSettingsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.ProductOrganisation
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command to enable/disable allow new quotes for products.
    /// </summary>
    public class UpdateProductOrganisationSettingsCommand : ICommand
    {
        public UpdateProductOrganisationSettingsCommand(
            Guid tenantId, Guid organisationId, Guid productId, bool isNewQuotesAllowed)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.ProductId = productId;
            this.IsNewQuotesAllowed = isNewQuotesAllowed;
        }

        public Guid TenantId { get; }

        public Guid OrganisationId { get; }

        public Guid ProductId { get; }

        public bool IsNewQuotesAllowed { get; }
    }
}
