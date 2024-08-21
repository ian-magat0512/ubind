// <copyright file="IPolicySearchResultItemReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Search
{
    using System;
    using NodaTime;

    public interface IPolicySearchResultItemReadModel : IEntitySearchResultItemReadModel
    {
        Guid ProductId { get; }

        Guid OrganisationId { get; }

        string ProductName { get; }

        string PolicyNumber { get; }

        string PolicyState { get; }

        string PolicyTitle { get; }

        DeploymentEnvironment Environment { get; }

        bool IsTestData { get; }

        bool IsDiscarded { get; }

        Guid QuoteId { get; }

        Guid? OwnerUserId { get; }

        Guid? OwnerPersonId { get; }

        string OwnerFullName { get; }

        Guid? CustomerId { get; }

        Guid? CustomerPersonId { get; }

        string CustomerFullName { get; }

        string CustomerPreferredName { get; }

        string CustomerEmail { get; }

        string CustomerAlternativeEmail { get; }

        string CustomerMobilePhone { get; }

        string CustomerHomePhone { get; }

        string CustomerWorkPhone { get; }

        Instant InceptionTimestamp { get; }

        Instant IssuedTimestamp { get; }

        Instant? CancellationEffectiveTimestamp { get; }

        Instant ExpiryTimestamp { get; }

        Instant? LatestRenewalEffectiveTimestamp { get; }

        Instant? RetroactiveTimestamp { get; }

        long IssuedTicksSinceEpoch { get; }

        long InceptionTicksSinceEpoch { get; }

        long ExpiryTicksSinceEpoch { get; }

        long? CancellationEffectiveTicksSinceEpoch { get; }

        long? LatestRenewalEffectiveTicksSinceEpoch { get; }

        long? RetroactiveTicksSinceEpoch { get; }
    }
}
