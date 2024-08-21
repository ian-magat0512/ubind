// <copyright file="CloneQuoteFromExpiredQuoteCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Quote
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    [RetryOnDbException(5)]
    public class CloneQuoteFromExpiredQuoteCommand : ICommand<NewQuoteReadModel>
    {
        public CloneQuoteFromExpiredQuoteCommand(
            Guid tenantId,
            Guid quoteId,
            DeploymentEnvironment deploymentEnvironment)
        {
            this.TenantId = tenantId;
            this.QuoteId = quoteId;
            this.DeploymentEnvironment = deploymentEnvironment;
        }

        public Guid TenantId { get; }

        public Guid QuoteId { get; }

        public DeploymentEnvironment DeploymentEnvironment { get; }
    }
}
