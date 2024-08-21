// <copyright file="MergeCustomerIntoExistingInvitedOrActivatedCustomerCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer.Merge
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for merging a new customer's record into an existing one.
    /// </summary>
    public class MergeCustomerIntoExistingInvitedOrActivatedCustomerCommand : ICommand
    {
        public MergeCustomerIntoExistingInvitedOrActivatedCustomerCommand(
            Guid tenantId, DeploymentEnvironment environment, Guid destinationPersonId)
        {
            this.DestinationPersonId = destinationPersonId;
            this.TenantId = tenantId;
            this.Environment = environment;
        }

        public Guid DestinationPersonId { get; }

        public Guid TenantId { get; }

        public DeploymentEnvironment Environment { get; }
    }
}
