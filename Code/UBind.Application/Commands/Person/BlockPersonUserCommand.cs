// <copyright file="BlockPersonUserCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Person
{
    using System;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for blocking person's user account.
    /// </summary>
    [CreateTransactionThatSavesChangesIfNoneExists]
    public class BlockPersonUserCommand : ChangeBlockingOfPersonUserCommand, ICommand<Unit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockPersonUserCommand"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="personId">The ID of the person.</param>
        public BlockPersonUserCommand(Guid tenantId, Guid personId)
            : base(tenantId, personId)
        {
        }
    }
}
