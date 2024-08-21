// <copyright file="AssignPortalToUserCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.User
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    public class AssignPortalToUserCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssignPortalToUserCommand"/> class.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="UserId">The Id of the user.</param>
        /// <param name="portalId">The Id of the portal ("null" value for unassigned).</param>
        public AssignPortalToUserCommand(Guid tenantId, Guid userId, Guid portalId)
        {
            this.TenantId = tenantId;
            this.UserId = userId;
            this.PortalId = portalId;
        }

        public Guid TenantId { get; }

        /// <summary>
        /// Gets the Id of the user where to assign the portal to.
        /// </summary>
        public Guid UserId { get; }

        /// <summary>
        /// Gets the Id of the portal.
        /// </summary>
        public Guid PortalId { get; }
    }
}
