// <copyright file="FakeNewQuoteReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Fakes
{
    using System;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests.Fakes;

    /// <summary>
    /// Fake class for NewQuoteReadModel.
    /// </summary>
    public class FakeNewQuoteReadModel : NewQuoteReadModel
    {
        public FakeNewQuoteReadModel(Guid quoteId)
        {
            this.Id = quoteId;
            this.CreatedTimestamp = new TestClock().GetCurrentInstant();
            this.LastModifiedTimestamp = new TestClock().GetCurrentInstant();
        }

        public FakeNewQuoteReadModel(
            Guid tenantId,
            Guid quoteId,
            Instant createdTimestamp,
            DeploymentEnvironment environment = DeploymentEnvironment.Development)
        {
            this.Id = quoteId;
            this.CreatedTimestamp = createdTimestamp;
            this.LastModifiedTimestamp = createdTimestamp;
            this.TenantId = tenantId;
            this.Environment = environment;
        }
    }
}
