// <copyright file="CreateCustomerCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Creates a customer.
    /// </summary>
    [CreateTransactionThatSavesChangesIfNoneExists]
    public class CreateCustomerCommand : ICommand<Guid>
    {
        public CreateCustomerCommand(
            Guid tenantId,
            DeploymentEnvironment environment,
            IPersonalDetails personDetails,
            Guid? ownerId,
            Guid? portalId,
            bool isTestData = false,
            List<AdditionalPropertyValueUpsertModel>? additionalProperties = null)
        {
            this.TenantId = tenantId;
            this.PersonDetails = personDetails;
            this.Environment = environment;
            this.OwnerId = ownerId;
            this.PortalId = portalId;
            this.IsTestData = isTestData;
            this.AdditionalProperties = additionalProperties;
        }

        public Guid TenantId { get; }

        public IPersonalDetails PersonDetails { get; }

        public DeploymentEnvironment Environment { get; }

        public Guid? OwnerId { get; }

        public Guid? PortalId { get; }

        public bool IsTestData { get; }

        public List<AdditionalPropertyValueUpsertModel>? AdditionalProperties { get; }
    }
}
