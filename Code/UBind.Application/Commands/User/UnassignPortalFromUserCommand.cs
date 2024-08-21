// <copyright file="UnassignPortalFromUserCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.User
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for unassigning portal to a customer.
    /// </summary>
    [RetryOnDbException(5)]
    public class UnassignPortalFromUserCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnassignPortalFromUserCommand"/> class.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="userId">The Id of the customer.</param>
        public UnassignPortalFromUserCommand(Guid tenantId, Guid userId)
        {
            this.TenantId = tenantId;
            this.UserId = userId;
        }

        public Guid TenantId { get; }

        public Guid UserId { get; }
    }
}