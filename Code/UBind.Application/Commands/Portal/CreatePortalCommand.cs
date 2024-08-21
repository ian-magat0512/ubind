// <copyright file="CreatePortalCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Portal
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Portal;

    [CreateTransactionThatSavesChangesIfNoneExists]
    public class CreatePortalCommand : ICommand<PortalReadModel>
    {
        public CreatePortalCommand(
            Guid tenantId,
            string name,
            string alias,
            string title,
            PortalUserType userType,
            Guid organisationId)
        {
            this.TenantId = tenantId;
            this.Name = name;
            this.Alias = alias;
            this.Title = title;
            this.UserType = userType;
            this.OrganisationId = organisationId;
        }

        public Guid TenantId { get; }

        public string Name { get; }

        public string Alias { get; }

        public string Title { get; }

        public PortalUserType UserType { get; }

        public Guid OrganisationId { get; }
    }
}
