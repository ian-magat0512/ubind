// <copyright file="EnquireClaimCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Claim
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents the command for lodging a claim enquiry.
    /// </summary>
    public class EnquireClaimCommand : ICommand
    {
        public EnquireClaimCommand(
            Guid tenantId,
            Guid productId,
            Guid quoteId,
            string formDataJson)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.ClaimId = quoteId;
            this.FormDataJson = formDataJson;
        }

        public Guid TenantId { get; private set; }

        public Guid ProductId { get; private set; }

        public Guid ClaimId { get; private set; }

        public string FormDataJson { get; private set; }
    }
}
