// <copyright file="PolicyTransactionSearchIndexWriteModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Search
{
    using System;

    public class PolicyTransactionSearchIndexWriteModel : EntitySearchIndexWriteModel, IPolicyTransactionSearchIndexWriteModel
    {
        public Guid PolicyTransactionId
        {
            get
            {
                return this.Id;
            }

            set
            {
                this.Id = value;
            }
        }

        public Guid PolicyId { get; set; }

        public Guid QuoteId { get; set; }

        public Guid TenantId { get; set; }

        public Guid ProductId { get; set; }

        public string QuoteNumber { get; set; }

        public string Discriminator { get; set; }

        public string PolicyData_FormData { get; set; }

        public string PolicyData_SerializedCalculationResult { get; set; }

        public long ExpiryTicksSinceEpoch { get; set; }

        public long EffectiveTicksSinceEpoch { get; set; }

        public Guid? CustomerId { get; set; }

        public Guid? OwnerUserId { get; set; }

        public Guid OrganisationId { get; set; }

        public bool IsTestData { get; set; }
    }
}
