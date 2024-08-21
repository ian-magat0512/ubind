// <copyright file="AssociateEmailRelationshipWithCustomerCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Messages
{
    using System;
    using System.Collections.Generic;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command object for assigning new customer Id from existing emails (relationships).
    /// </summary>
    public class AssociateEmailRelationshipWithCustomerCommand : ICommand<Unit>
    {
        public AssociateEmailRelationshipWithCustomerCommand(
            Guid tenantId, IReadOnlyList<Guid> emailIds, Guid oldCustomerId, Guid newCustomerId)
        {
            this.TenantId = tenantId;
            this.EmailIds = emailIds;
            this.OldCustomerId = oldCustomerId;
            this.NewCustomerId = newCustomerId;
        }

        public Guid TenantId { get; }

        public IReadOnlyList<Guid> EmailIds { get; }

        public Guid OldCustomerId { get; }

        public Guid NewCustomerId { get; }
    }
}
