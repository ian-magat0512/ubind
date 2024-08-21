// <copyright file="FakePolicyTransaction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Fakes
{
    using System;
    using UBind.Domain;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Tests.Fakes;

    /// <summary>
    /// Fake class for Policy Transactionl.
    /// </summary>
    public class FakePolicyTransaction : PolicyTransaction
    {
        public FakePolicyTransaction(Guid tenantId, Guid quoteId)
                : base(tenantId, quoteId, new TestClock().Timestamp)
        {
        }

        public FakePolicyTransaction(Guid tenantId, Guid quoteId, PolicyTransactionData policyTransactionData, Guid? productReleaseId)
        : base(
              tenantId,
              quoteId,
              quoteId,
              1,
              new TestClock().Timestamp.InZone(Timezones.AET).LocalDateTime,
              new TestClock().Timestamp,
              new TestClock().Timestamp.InZone(Timezones.AET).LocalDateTime,
              new TestClock().Timestamp,
              new TestClock().Timestamp,
              quoteId,
              "Test",
              policyTransactionData,
              productReleaseId,
              TransactionType.NewBusiness)
        {
        }

        public override string GetEventTypeSummary()
        {
            return "Sample";
        }
    }
}
