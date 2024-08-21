// <copyright file="CreateClaimVersionCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Claim
{
    using System;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// Represents the command for creating new quote version.
    /// </summary>
    [RetryOnDbException(5)]
    public class CreateClaimVersionCommand : ICommand<ClaimReadModel>
    {
        public CreateClaimVersionCommand(
            Guid tenantId,
            Guid claimId,
            string? formData = null)
        {
            this.TenantId = tenantId;
            this.ClaimId = claimId;
            this.FormData = formData;
        }

        /// <summary>
        /// Gets the tenant Id.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the quote Id.
        /// </summary>
        public Guid ClaimId { get; private set; }

        public string? FormData { get; }
    }
}
