// <copyright file="IPolicyTransactionSearchIndexWriteModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Search
{
    using System;

    public interface IPolicyTransactionSearchIndexWriteModel : IEntitySearchIndexWriteModel
    {
        Guid PolicyTransactionId { get; }

        Guid PolicyId { get; }

        Guid QuoteId { get; }

        Guid TenantId { get; }

        Guid ProductId { get; }

        string QuoteNumber { get; }

        string Discriminator { get; }

        string PolicyData_FormData { get; }

        string PolicyData_SerializedCalculationResult { get; }

        long ExpiryTicksSinceEpoch { get; }

        long EffectiveTicksSinceEpoch { get; }

        Guid? CustomerId { get; }

        Guid? OwnerUserId { get; }

        Guid OrganisationId { get; }

        bool IsTestData { get; }
    }
}
