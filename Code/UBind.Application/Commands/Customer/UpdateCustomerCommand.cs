// <copyright file="UpdateCustomerCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Patterns.Cqrs;

    public class UpdateCustomerCommand : ICommand
    {
        public UpdateCustomerCommand(
            Guid tenantId,
            Guid customerId,
            IPersonalDetails personDetails)
        {
            this.TenantId = tenantId;
            this.CustomerId = customerId;
            this.PersonDetails = personDetails;
        }

        public Guid TenantId { get; }

        public Guid CustomerId { get; }

        public IPersonalDetails PersonDetails { get; }
    }
}
