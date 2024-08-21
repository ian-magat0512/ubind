// <copyright file="CreateCustomerPersonUserAccountCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command model to create a customer user.
    /// </summary>
    [CreateTransactionThatSavesChangesIfNoneExists]
    public class CreateCustomerPersonUserAccountCommand : ICommand<Guid>
    {
        public CreateCustomerPersonUserAccountCommand(
            Guid tenantId,
            Guid? portalId,
            DeploymentEnvironment environment,
            IPersonalDetails personDetails,
            Guid organisationId,
            Guid? personId = null)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.PortalId = portalId;
            this.Environment = environment;
            this.PersonDetails = personDetails;
            this.PersonId = personId;
        }

        public Guid TenantId { get; private set; }

        public Guid OrganisationId { get; private set; }

        public Guid? PortalId { get; private set; }

        public DeploymentEnvironment Environment { get; private set; }

        public IPersonalDetails PersonDetails { get; private set; }

        public Guid? PersonId { get; private set; }
    }
}
