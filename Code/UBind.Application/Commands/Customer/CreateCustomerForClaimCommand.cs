// <copyright file="CreateCustomerForClaimCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Patterns.Cqrs;

    [CreateTransactionThatSavesChangesIfNoneExists]
    public class CreateCustomerForClaimCommand : CreateCustomerCommand
    {
        public CreateCustomerForClaimCommand(
            Guid tenantId,
            DeploymentEnvironment environment,
            ClaimAggregate claimAggregate,
            IPersonalDetails personDetails,
            Guid? ownerId,
            Guid? portalId,
            bool isTestData = false)
            : base(tenantId, environment, personDetails, ownerId, portalId, isTestData)
        {
            this.ClaimAggregate = claimAggregate;
        }

        public ClaimAggregate ClaimAggregate { get; }
    }
}
