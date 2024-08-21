// <copyright file="ChangeBlockingOfPersonUserCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Person
{
    using System;

    /// <summary>
    /// The base class as command for Blocking or unblocking the person's user account.
    /// </summary>
    public abstract class ChangeBlockingOfPersonUserCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeBlockingOfPersonUserCommand"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="personId">The ID of the person.</param>
        public ChangeBlockingOfPersonUserCommand(Guid tenantId, Guid personId)
        {
            this.PersonId = personId;
            this.TenantId = tenantId;
        }

        /// <summary>
        /// Gets the person id.
        /// </summary>
        public Guid PersonId { get; private set; }

        /// <summary>
        /// Gets the tenant id.
        /// </summary>
        public Guid TenantId { get; private set; }
    }
}
