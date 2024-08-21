// <copyright file="UpdateOrganisationCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Organisation
{
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Patterns.Cqrs;

    [CreateTransactionThatSavesChangesIfNoneExists]
    public class UpdateOrganisationCommand : ICommand
    {
        public UpdateOrganisationCommand(
            Guid tenantId,
            Guid organisationId,
            string alias,
            string name,
            List<AdditionalPropertyValueUpsertModel>? properties = null,
            List<Domain.Aggregates.Organisation.LinkedIdentity>? linkedIdentities = null)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.Alias = alias;
            this.Name = name;
            this.Properties = properties;
            this.LinkedIdentities = linkedIdentities;
        }

        public Guid TenantId { get; }

        public Guid OrganisationId { get; }

        public string Alias { get; }

        public string Name { get; }

        public List<AdditionalPropertyValueUpsertModel>? Properties { get; }

        public List<UBind.Domain.Aggregates.Organisation.LinkedIdentity>? LinkedIdentities { get; }
    }
}
