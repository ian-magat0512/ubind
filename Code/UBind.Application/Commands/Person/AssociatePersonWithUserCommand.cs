// <copyright file="AssociatePersonWithUserCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Person
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    [RetryOnDbException(5)]
    public class AssociatePersonWithUserCommand : ICommand
    {
        public AssociatePersonWithUserCommand(Guid tenantId, Guid personId, Guid userId)
        {
            this.TenantId = tenantId;
            this.PersonId = personId;
            this.UserId = userId;
        }

        public Guid TenantId { get; private set; }

        public Guid PersonId { get; private set; }

        public Guid UserId { get; private set; }
    }
}
