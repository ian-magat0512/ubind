// <copyright file="GetPolicyRepositoryAndLuceneIndexCountQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.LuceneIndex
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Search;

    public class GetPolicyRepositoryAndLuceneIndexCountQuery : IQuery<IEnumerable<IEntityRepositoryAndLuceneIndexCountModel>>
    {
        public GetPolicyRepositoryAndLuceneIndexCountQuery(
            Instant fromTimestamp,
            Instant toTimestamp,
            DeploymentEnvironment environment,
            Guid? tenantId = null)
        {
            this.FromTimestamp = fromTimestamp;
            this.ToTimestamp = toTimestamp;
            this.Environment = environment;
            this.TenantId = tenantId;
        }

        public Instant FromTimestamp { get; private set; }

        public Instant ToTimestamp { get; private set; }

        public DeploymentEnvironment Environment { get; private set; }

        public Guid? TenantId { get; private set; }
    }
}
