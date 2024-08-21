// <copyright file="FixAssociationOfPersonToCustomerCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Person
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for fixing association of existing person aggregates to customer counterparts.
    /// </summary>
    public class FixAssociationOfPersonToCustomerCommand : ICommand
    {
        public FixAssociationOfPersonToCustomerCommand(Guid tenantId, List<Guid> personIds)
        {
            this.TenantId = tenantId;
            this.PersonIds = personIds;
        }

        /// <summary>
        /// Gets the ID of the tenant the fix is running against.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the list of person IDs that need to be fixed.
        /// </summary>
        public List<Guid> PersonIds { get; private set; }
    }
}
