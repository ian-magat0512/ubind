// <copyright file="PolicySearchIndexWriteModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Search
{
    using System;
    using System.Collections.Generic;

    public class PolicySearchIndexWriteModel : EntitySearchIndexWriteModel, IPolicySearchIndexWriteModel
    {
        public Guid QuoteId { get; set; }

        public Guid OrganisationId { get; set; }

        public Guid TenantId { get; set; }

        public Guid ProductId { get; set; }

        public Guid? OwnerUserId { get; set; }

        public Guid? OwnerPersonId { get; set; }

        public string OwnerFullName { get; set; }

        public Guid? CustomerId { get; set; }

        public Guid? CustomerPersonId { get; set; }

        public string CustomerFullName { get; set; }

        public string CustomerPreferredName { get; set; }

        public string CustomerEmail { get; set; }

        public string CustomerAlternativeEmail { get; set; }

        public string CustomerHomePhone { get; set; }

        public string CustomerWorkPhone { get; set; }

        public string CustomerMobilePhone { get; set; }

        public long IssuedTicksSinceEpoch { get; set; }

        public string PolicyNumber { get; set; }

        public string PolicyState { get; set; }

        public string PolicyTitle { get; set; }

        public long InceptionTicksSinceEpoch { get; set; }

        public long ExpiryTicksSinceEpoch { get; set; }

        public long CancellationEffectiveTicksSinceEpoch { get; set; }

        public long? LatestRenewalEffectiveTicksSinceEpoch { get; set; }

        public long? RetroactiveTicksSinceEpoch { get; set; }

        public string SerializedCalculationResult { get; set; }

        public bool IsDiscarded { get; set; }

        public bool IsTestData { get; set; }

        public List<IPolicyTransactionSearchIndexWriteModel> PolicyTransactionModel { get; set; }

        public string ProductName { get; set; }
    }
}
