// <copyright file="FakeEventFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Fakes
{
    using System;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Tests.Fakes;

    /// <summary>
    /// Factory for fake events for use in tests.
    /// </summary>
    public static class FakeEventFactory
    {
        /// <summary>
        /// Create a fake event for a test.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the event belongs to (defaults to "tenantX").</param>
        /// <param name="organisationId">The ID of the organisation the event belongs to (defaults to "tenantX").</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>A custom event.</returns>
        public static SystemEvent Create(
            Guid tenantId = default,
            Guid organisationId = default,
            DeploymentEnvironment environment = DeploymentEnvironment.Staging)
            => SystemEvent.CreateCustom(
                tenantId == default ? TenantFactory.DefaultId : tenantId,
                organisationId,
                ProductFactory.DefaultId,
                environment,
                "aliasX",
                "{}",
                SystemClock.Instance.GetCurrentInstant());
    }
}
