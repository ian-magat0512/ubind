// <copyright file="CreateClaimCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Claim
{
    using System;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Creates a claim that is not associated with an existing policy.
    /// </summary>
    public class CreateClaimCommand : ICommand<Guid>
    {
        public CreateClaimCommand(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment environment,
            bool isTestData,
            Guid? customerId,
            Guid? ownerUserId,
            DateTimeZone timeZone)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.ProductId = productId;
            this.Environment = environment;
            this.IsTestData = isTestData;
            this.CustomerId = customerId;
            this.OwnerUserId = ownerUserId;
            this.TimeZone = timeZone;
        }

        public Guid TenantId { get; }

        public Guid OrganisationId { get; }

        public Guid ProductId { get; }

        public bool IsTestData { get; }

        public DeploymentEnvironment Environment { get; }

        public Guid? CustomerId { get; }

        public Guid? OwnerUserId { get; }

        public DateTimeZone TimeZone { get; }
    }
}
