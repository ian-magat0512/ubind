// <copyright file="CreateOrganisationCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Organisation
{
    using System;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    [CreateTransactionThatSavesChangesIfNoneExists]
    public class CreateOrganisationCommand : ICommand<OrganisationReadModel>
    {
        public CreateOrganisationCommand(
            Guid tenantId,
            string? alias,
            string name,
            Guid? managingOrganisationId,
            List<AdditionalPropertyValueUpsertModel>? properties = null,
            List<Domain.Aggregates.Organisation.LinkedIdentity>? linkedIdentities = null)
        {
            this.TenantId = tenantId;
            this.Alias = alias;
            this.Name = name;
            this.ManagingOrganisationId = managingOrganisationId;
            this.Properties = properties;
            this.LinkedIdentities = linkedIdentities;
        }

        public Guid TenantId { get; }

        public string? Alias { get; set; }

        public string Name { get; }

        public Guid? ManagingOrganisationId { get; }

        public List<AdditionalPropertyValueUpsertModel>? Properties { get; }

        /// <summary>
        /// Gets a list of authentication methods that this organisation should be linked to.
        /// This is used typically during auto provisioning of organisations for SAML identity providers, where one
        /// linked identity will be set.
        /// It may also be used during manual creation of organisations, where multiple linked identities may be set.
        /// </summary>
        public List<UBind.Domain.Aggregates.Organisation.LinkedIdentity>? LinkedIdentities { get; }
    }
}
