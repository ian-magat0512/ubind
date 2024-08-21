// <copyright file="UpdateCustomerDetailsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Patterns.Cqrs;

    [CreateTransactionThatSavesChangesIfNoneExists]
    public class UpdateCustomerDetailsCommand : ICommand
    {
        public UpdateCustomerDetailsCommand(
            Guid tenantId,
            Guid customerId,
            IPersonalDetails details,
            Guid? portalId,
            List<AdditionalPropertyValueUpsertModel> additionalPropertyValueUpsertModels = null)
        {
            this.TenantId = tenantId;
            this.CustomerId = customerId;
            this.Details = details;
            this.PortalId = portalId;
            this.AdditionalPropertyValueUpsertModels = additionalPropertyValueUpsertModels;
        }

        public Guid TenantId { get; private set; }

        public Guid CustomerId { get; private set; }

        public Guid? PortalId { get; private set; }

        public IPersonalDetails Details { get; private set; }

        public List<AdditionalPropertyValueUpsertModel> AdditionalPropertyValueUpsertModels { get; private set; }
    }
}
