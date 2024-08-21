// <copyright file="GetAutomationsForEnvironmentQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.PortalExtensions
{
    using System;
    using System.Collections.Generic;
    using UBind.Application.Automation;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    public class GetAutomationsForEnvironmentQuery : IQuery<List<ProductAutomation>>
    {
        public GetAutomationsForEnvironmentQuery(Guid tenantId, DeploymentEnvironment environment)
        {
            this.TenantId = tenantId;
            this.Environment = environment;
        }

        public Guid TenantId { get; }

        public DeploymentEnvironment Environment { get; }
    }
}
