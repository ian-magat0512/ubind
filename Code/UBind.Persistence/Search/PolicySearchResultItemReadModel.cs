// <copyright file="PolicySearchResultItemReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Search
{
    using System;
    using Lucene.Net.Documents;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Search;

    public class PolicySearchResultItemReadModel : EntitySearchResultItemReadModel, IPolicySearchResultItemReadModel
    {
        public PolicySearchResultItemReadModel(Document document, Guid id, long createdTimestamp, long lastUpdatedTimestamp)
            : base(id, createdTimestamp, lastUpdatedTimestamp)
        {
            this.TenantId = Guid.Parse(document.Get(PolicyLuceneFieldNames.FieldTenantId));
            this.ProductId = Guid.Parse(document.Get(PolicyLuceneFieldNames.FieldProductId));
            this.OrganisationId = Guid.Parse(document.Get(PolicyLuceneFieldNames.FieldOrganisationId));
            this.ProductName = document.Get(PolicyLuceneFieldNames.FieldProductName);
            this.PolicyNumber = document.Get(PolicyLuceneFieldNames.FieldPolicyNumber);
            this.PolicyState = document.Get(PolicyLuceneFieldNames.FieldPolicyState);
            this.PolicyTitle = document.Get(PolicyLuceneFieldNames.FieldPolicyTitle);
            this.IsTestData = bool.Parse(document.Get(PolicyLuceneFieldNames.IsTestData));
            this.IsDiscarded = bool.Parse(document.Get(PolicyLuceneFieldNames.IsDiscarded));
            this.QuoteId = Guid.Parse(document.Get(PolicyLuceneFieldNames.FieldQuoteId));
            if (Guid.TryParse(document.Get(PolicyLuceneFieldNames.FieldOwnerUserId), out Guid ownerUserId))
            {
                this.OwnerUserId = ownerUserId;
            }

            if (Guid.TryParse(document.Get(PolicyLuceneFieldNames.FieldOwnerPersonId), out Guid ownerPersonId))
            {
                this.OwnerPersonId = ownerPersonId;
            }

            if (Guid.TryParse(document.Get(PolicyLuceneFieldNames.FieldCustomerId), out Guid customerId))
            {
                this.CustomerId = customerId;
            }

            if (Guid.TryParse(document.Get(PolicyLuceneFieldNames.FieldCustomerPersonId), out Guid customerPersonId))
            {
                this.CustomerPersonId = customerPersonId;
            }

            this.OwnerFullName = document.Get(PolicyLuceneFieldNames.FieldOwnerFullName);
            this.CustomerFullName = document.Get(PolicyLuceneFieldNames.FieldCustomerFullName);
            this.CustomerPreferredName = document.Get(PolicyLuceneFieldNames.FieldCustomerPreferredName);
            this.CustomerEmail = document.Get(PolicyLuceneFieldNames.FieldCustomerEmail);
            this.CustomerAlternativeEmail = document.Get(PolicyLuceneFieldNames.FieldCustomerAlternativeEmail);
            this.CustomerHomePhone = document.Get(PolicyLuceneFieldNames.FieldCustomerHomePhone);
            this.CustomerWorkPhone = document.Get(PolicyLuceneFieldNames.FieldCustomerWorkPhone);
            this.CustomerMobilePhone = document.Get(PolicyLuceneFieldNames.FieldCustomerMobilePhone);
            this.IssuedTicksSinceEpoch = long.Parse(document.Get(PolicyLuceneFieldNames.FieldIssuedTicksSinceEpoch));
            this.InceptionTicksSinceEpoch = long.Parse(document.Get(PolicyLuceneFieldNames.FieldInceptionTicksSinceEpoch));
            this.ExpiryTicksSinceEpoch = long.Parse(document.Get(PolicyLuceneFieldNames.FieldExpiryTicksSinceEpoch));
            if (long.TryParse(document.Get(PolicyLuceneFieldNames.FieldCancellationEffectiveTicksSinceEpoch), out long cancellationEffectiveTicksSinceEpoch))
            {
                this.CancellationEffectiveTicksSinceEpoch = cancellationEffectiveTicksSinceEpoch;
            }

            if (long.TryParse(document.Get(PolicyLuceneFieldNames.FieldLatestRenewalEffectiveTicksSinceEpoch), out long latestRenewalEffectiveTicksSinceEpoch))
            {
                this.LatestRenewalEffectiveTicksSinceEpoch = latestRenewalEffectiveTicksSinceEpoch;
            }

            if (long.TryParse(document.Get(PolicyLuceneFieldNames.FieldRetroactiveTicksSinceEpoch), out long retroactiveTicksSinceEpoch))
            {
                this.RetroactiveTicksSinceEpoch = retroactiveTicksSinceEpoch;
            }
        }

        public Guid ProductId { get; private set; }

        public Guid OrganisationId { get; private set; }

        public string ProductName { get; private set; }

        public string PolicyNumber { get; private set; }

        public string PolicyState { get; private set; }

        public string PolicyTitle { get; private set; }

        public DeploymentEnvironment Environment { get; private set; }

        public bool IsTestData { get; private set; }

        public bool IsDiscarded { get; private set; }

        public Guid QuoteId { get; private set; }

        public Guid? OwnerUserId { get; private set; }

        public Guid? OwnerPersonId { get; private set; }

        public string OwnerFullName { get; private set; }

        public Guid? CustomerId { get; private set; }

        public Guid? CustomerPersonId { get; private set; }

        public string CustomerFullName { get; private set; }

        public string CustomerPreferredName { get; private set; }

        public string CustomerEmail { get; private set; }

        public string CustomerAlternativeEmail { get; private set; }

        public string CustomerMobilePhone { get; private set; }

        public string CustomerHomePhone { get; private set; }

        public string CustomerWorkPhone { get; private set; }

        public Instant InceptionTimestamp
        {
            get { return Instant.FromUnixTimeTicks(this.InceptionTicksSinceEpoch); }
            private set { this.InceptionTicksSinceEpoch = value.ToUnixTimeTicks(); }
        }

        public Instant IssuedTimestamp
        {
            get { return Instant.FromUnixTimeTicks(this.IssuedTicksSinceEpoch); }
            private set { this.IssuedTicksSinceEpoch = value.ToUnixTimeTicks(); }
        }

        public Instant? CancellationEffectiveTimestamp
        {
            get
            {
                return this.CancellationEffectiveTicksSinceEpoch.HasValue
                    ? Instant.FromUnixTimeTicks(this.CancellationEffectiveTicksSinceEpoch.Value)
                    : (Instant?)null;
            }

            private set
            {
                this.CancellationEffectiveTicksSinceEpoch = value.HasValue
                  ? value.Value.ToUnixTimeTicks()
                  : (long?)null;
            }
        }

        public Instant ExpiryTimestamp
        {
            get { return Instant.FromUnixTimeTicks(this.ExpiryTicksSinceEpoch); }
            private set { this.ExpiryTicksSinceEpoch = value.ToUnixTimeTicks(); }
        }

        public Instant? LatestRenewalEffectiveTimestamp
        {
            get
            {
                return this.LatestRenewalEffectiveTicksSinceEpoch.HasValue
                    ? Instant.FromUnixTimeTicks(this.LatestRenewalEffectiveTicksSinceEpoch.Value)
                    : (Instant?)null;
            }

            private set
            {
                this.LatestRenewalEffectiveTicksSinceEpoch = value.HasValue
                  ? value.Value.ToUnixTimeTicks()
                  : (long?)null;
            }
        }

        public Instant? RetroactiveTimestamp
        {
            get
            {
                return this.RetroactiveTicksSinceEpoch.HasValue
                    ? Instant.FromUnixTimeTicks(this.RetroactiveTicksSinceEpoch.Value)
                    : (Instant?)null;
            }

            private set
            {
                this.RetroactiveTicksSinceEpoch = value.HasValue
                  ? value.Value.ToUnixTimeTicks()
                  : (long?)null;
            }
        }

        public long IssuedTicksSinceEpoch { get; private set; }

        public long InceptionTicksSinceEpoch { get; private set; }

        public long ExpiryTicksSinceEpoch { get; private set; }

        public long? CancellationEffectiveTicksSinceEpoch { get; private set; }

        public long? LatestRenewalEffectiveTicksSinceEpoch { get; private set; }

        public long? RetroactiveTicksSinceEpoch { get; private set; }
    }
}
