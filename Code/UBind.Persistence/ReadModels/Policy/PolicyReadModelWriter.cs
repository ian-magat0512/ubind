// <copyright file="PolicyReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using NodaTime;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Repositories;
    using UBind.Domain.Search;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;

    /// <summary>
    /// For updating the quote read model in response to events from the write model.
    /// </summary>
    public class PolicyReadModelWriter : IPolicyReadModelWriter
    {
        private readonly IWritableReadModelRepository<NewQuoteReadModel> quoteReadModelRepository;
        private readonly IWritableReadModelRepository<PolicyReadModel> policyReadModelRepository;
        private readonly IWritableReadModelRepository<PolicyTransaction> policyTransactionRepository;
        private readonly ILuceneRepository<IPolicySearchIndexWriteModel, IPolicySearchResultItemReadModel, PolicyReadModelFilters> lucenePolicyRepository;
        private readonly ITenantRepository tenantRepository;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyReadModelWriter"/> class.
        /// </summary>
        /// <param name="quoteReadModelRepository">The repository for quote read models.</param>
        /// <param name="policyReadModelRepository">The repository for policy read models.</param>
        /// <param name="policyTransactionRepository">A repository for policy transactions.</param>
        /// <param name="tenantRepository">A repository for tenants.</param>
        public PolicyReadModelWriter(
            IWritableReadModelRepository<NewQuoteReadModel> quoteReadModelRepository,
            IWritableReadModelRepository<PolicyReadModel> policyReadModelRepository,
            IWritableReadModelRepository<PolicyTransaction> policyTransactionRepository,
            ITenantRepository tenantRepository,
            ILuceneRepository<IPolicySearchIndexWriteModel, IPolicySearchResultItemReadModel, PolicyReadModelFilters> lucenePolicyRepository,
            IClock clock)
        {
            this.lucenePolicyRepository = lucenePolicyRepository;
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.policyReadModelRepository = policyReadModelRepository;
            this.policyTransactionRepository = policyTransactionRepository;
            this.tenantRepository = tenantRepository;
            this.clock = clock;
        }

        public void Dispatch(
            QuoteAggregate aggregate,
            IEvent<QuoteAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Dispatch(
            PersonAggregate aggregate,
            IEvent<PersonAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.CustomerAssignedEvent @event, int sequenceNumber)
        {
            var policy = this.GetPolicyById(@event.TenantId, @event.AggregateId);
            if (policy != null)
            {
                policy.CustomerId = @event.CustomerId;
                policy.CustomerPersonId = @event.PersonId;
                policy.CustomerFullName = @event.CustomerDetails.FullName;
                policy.CustomerPreferredName = @event.CustomerDetails.PreferredName;
                policy.CustomerEmail = @event.CustomerDetails.Email;
                policy.CustomerAlternativeEmail = @event.CustomerDetails.AlternativeEmail;
                policy.CustomerMobilePhone = @event.CustomerDetails.MobilePhone;
                policy.CustomerHomePhone = @event.CustomerDetails.HomePhone;
                policy.CustomerWorkPhone = @event.CustomerDetails.WorkPhone;
                policy.LastModifiedTimestamp = @event.Timestamp;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
#pragma warning disable CS8602 // We know that aggregate.Policy is not null here.
                aggregate.Policy.ReadModel = policy;
#pragma warning restore CS8602
            }

            var transactions = this.GetUpsertTransactionsForAggregate(@event.TenantId, @event.AggregateId);
            foreach (var transaction in transactions)
            {
                transaction.CustomerId = @event.CustomerId;
                transaction.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.CustomerDetailsUpdatedEvent @event, int sequenceNumber)
        {
            var policy = this.GetPolicyById(@event.TenantId, @event.AggregateId);

            // Only update model if there is no customer assigned, otherwise customer events will take care of it.
            if (policy != null && policy.CustomerId == default)
            {
                if (@event.CustomerDetails.FullName != null)
                {
                    policy.CustomerFullName = @event.CustomerDetails.FullName;
                }

                if (@event.CustomerDetails.PreferredName != null)
                {
                    policy.CustomerPreferredName = @event.CustomerDetails.PreferredName;
                }

                if (@event.CustomerDetails.Email != null)
                {
                    policy.CustomerEmail = @event.CustomerDetails.Email;
                }

                if (@event.CustomerDetails.AlternativeEmail != null)
                {
                    policy.CustomerAlternativeEmail = @event.CustomerDetails.AlternativeEmail;
                }

                if (@event.CustomerDetails.MobilePhone != null)
                {
                    policy.CustomerMobilePhone = @event.CustomerDetails.MobilePhone;
                }

                if (@event.CustomerDetails.HomePhone != null)
                {
                    policy.CustomerHomePhone = @event.CustomerDetails.HomePhone;
                }

                if (@event.CustomerDetails.WorkPhone != null)
                {
                    policy.CustomerWorkPhone = @event.CustomerDetails.WorkPhone;
                }

                policy.LastModifiedTimestamp = @event.Timestamp;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
#pragma warning disable CS8602 // We know that aggregate.Policy is not null here.
                aggregate.Policy.ReadModel = policy;
#pragma warning restore CS8602
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.OwnershipAssignedEvent @event, int sequenceNumber)
        {
            var policy = this.GetPolicyById(@event.TenantId, @event.AggregateId);
            if (policy != null)
            {
                policy.OwnerUserId = @event.UserId;
                policy.OwnerPersonId = @event.PersonId;
                policy.OwnerFullName = @event.FullName;
                policy.LastModifiedTimestamp = @event.Timestamp;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
                aggregate.Policy!.ReadModel = policy;
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.OwnershipUnassignedEvent @event, int sequenceNumber)
        {
            var policy = this.GetPolicyById(@event.TenantId, @event.AggregateId);
            if (policy != null)
            {
                policy.OwnerUserId = null;
                policy.OwnerPersonId = null;
                policy.OwnerFullName = string.Empty;
                policy.LastModifiedTimestamp = @event.Timestamp;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
#pragma warning disable CS8602 // We know that aggregate.Policy is not null here.
                aggregate.Policy.ReadModel = policy;
#pragma warning restore CS8602
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyIssuedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read models
                this.policyReadModelRepository.DeleteById(@event.TenantId, @event.AggregateId);
                this.policyTransactionRepository.DeleteById(@event.TenantId, @event.NewBusinessTransactionId);
            }

            var quoteId = @event.QuoteId.Value;
            var quote = this.GetQuoteById(@event.TenantId, quoteId);
            var policy = new PolicyReadModel(@event, quote);
            policy.PolicyTitle = quote?.QuoteTitle;
            this.policyReadModelRepository.Add(policy);

            policy.OnPolicyIssued(@event);
            policy.UpdatePolicyState(this.clock.Now());
            quote?.RecordPolicyIssued(policy, @event.Timestamp);

            // Write policy transactions
            var transactionData = new PolicyTransactionData(@event.DataSnapshot);
            var transaction = new NewBusinessTransaction(
                aggregate.TenantId,
                @event.NewBusinessTransactionId,
                @event.AggregateId,
                quoteId,
                quote?.QuoteNumber,
                sequenceNumber,
                @event.InceptionDateTime,
                @event.InceptionTimestamp,
                @event.ExpiryDateTime,
                @event.ExpiryTimestamp,
                @event.Timestamp,
                transactionData,
                @event.ProductReleaseId);
            transaction.Environment = quote.Environment;
            transaction.TenantId = quote.TenantId;
            transaction.ProductId = quote.ProductId;
            transaction.CustomerId = quote.CustomerId;
            transaction.OwnerUserId = quote.OwnerUserId;
            transaction.OrganisationId = quote.OrganisationId;
            transaction.IsTestData = policy.IsTestData;
            if (transactionData.CalculationResult != null && transactionData.CalculationResult.PayablePrice != null)
            {
                transaction.TotalPayable = transactionData.CalculationResult.PayablePrice.TotalPayable;
            }

            quote.PolicyTransactionId = transaction.Id;
            this.policyTransactionRepository.Add(transaction);
#pragma warning disable CS8602 // We know that aggregate.Policy is not null here.
            aggregate.Policy.ReadModel = policy;
#pragma warning restore CS8602
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyImportedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read models
                this.policyReadModelRepository.DeleteById(@event.TenantId, @event.AggregateId);
                this.policyTransactionRepository.DeleteById(@event.TenantId, @event.NewBusinessTransactionId);
            }

            var policy = new PolicyReadModel(@event);
            policy.UpdatePolicyState(this.clock.Now());
            this.policyReadModelRepository.Add(policy);

            policy.CustomerId = @event.CustomerId;
            policy.CustomerPersonId = @event.PersonId;
            policy.CustomerFullName = @event.DataSnapshot.CustomerDetails.Data.FullName;
            policy.CustomerPreferredName = @event.DataSnapshot.CustomerDetails.Data.PreferredName;
            policy.CustomerEmail = @event.DataSnapshot.CustomerDetails.Data.Email;
            policy.CustomerAlternativeEmail = @event.DataSnapshot.CustomerDetails.Data.AlternativeEmail;
            policy.CustomerMobilePhone = @event.DataSnapshot.CustomerDetails.Data.MobilePhone;
            policy.CustomerHomePhone = @event.DataSnapshot.CustomerDetails.Data.HomePhone;
            policy.CustomerWorkPhone = @event.DataSnapshot.CustomerDetails.Data.WorkPhone;
            policy.LastModifiedTimestamp = @event.Timestamp;
            policy.LastModifiedByUserTimestamp = @event.Timestamp;

            var tenant = this.tenantRepository.GetTenantById(policy.TenantId);
            if (tenant == null)
            {
                throw new InvalidOperationException($"Policy {policy.Id} has a tenant ID {policy.TenantId} that does not exist.");
            }

            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            var organisationId = @event.OrganisationId == default ? defaultOrganisationId : @event.OrganisationId;

            policy.OrganisationId = organisationId;

            // Write policy transactions
            var transactionData = new PolicyTransactionData(@event.DataSnapshot);
            var transaction = new NewBusinessTransaction(
                aggregate.TenantId,
                @event.NewBusinessTransactionId,
                @event.AggregateId,
                @event.QuoteId.Value,
                null,
                sequenceNumber,
                @event.InceptionDateTime,
                @event.InceptionTimestamp,
                @event.ExpiryDateTime,
                @event.ExpiryTimestamp,
                @event.Timestamp,
                transactionData,
                @event.ProductReleaseId)
            {
                Environment = @event.Environment,
                TenantId = @event.TenantId,
                ProductId = @event.ProductId,
                CustomerId = @event.CustomerId,
                OrganisationId = organisationId,
                IsTestData = @event.IsTestData,
            };
            if (transactionData.CalculationResult != null && transactionData.CalculationResult.PayablePrice != null)
            {
                transaction.TotalPayable = transactionData.CalculationResult.PayablePrice.TotalPayable;
            }

            this.policyTransactionRepository.Add(transaction);
#pragma warning disable CS8602 // We know that aggregate.Policy is not null here.
            aggregate.Policy.ReadModel = policy;
#pragma warning restore CS8602
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyIssuedWithoutQuoteEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read models
                this.policyReadModelRepository.DeleteById(@event.TenantId, @event.AggregateId);
                this.policyTransactionRepository.DeleteById(@event.TenantId, @event.NewBusinessTransactionId);
            }

            var policy = new PolicyReadModel(@event);
            policy.UpdatePolicyState(this.clock.Now());
            this.policyReadModelRepository.Add(policy);

            policy.PolicyTitle = @event.PolicyTitle;
            policy.CustomerId = @event.CustomerId;
            policy.CustomerPersonId = @event.PersonId;
            policy.CustomerFullName = @event.DataSnapshot?.CustomerDetails?.Data?.FullName;
            policy.CustomerPreferredName = @event.DataSnapshot?.CustomerDetails?.Data?.PreferredName;
            policy.CustomerEmail = @event.DataSnapshot?.CustomerDetails?.Data?.Email;
            policy.CustomerAlternativeEmail = @event.DataSnapshot?.CustomerDetails?.Data?.AlternativeEmail;
            policy.CustomerMobilePhone = @event.DataSnapshot?.CustomerDetails?.Data?.MobilePhone;
            policy.CustomerHomePhone = @event.DataSnapshot?.CustomerDetails?.Data?.HomePhone;
            policy.CustomerWorkPhone = @event.DataSnapshot?.CustomerDetails?.Data?.WorkPhone;
            policy.LastModifiedTimestamp = @event.Timestamp;
            policy.LastModifiedByUserTimestamp = @event.Timestamp;

            if (@event.OrganisationId == default)
            {
                var tenant = this.tenantRepository.GetTenantById(policy.TenantId);
                if (tenant == null)
                {
                    throw new InvalidOperationException($"Policy {policy.Id} has a tenant ID {policy.TenantId} that does not exist.");
                }

                policy.OrganisationId = tenant.Details.DefaultOrganisationId;
            }
            else
            {
                policy.OrganisationId = @event.OrganisationId;
            }

            // Write policy transactions
            var transactionData = new PolicyTransactionData(@event.DataSnapshot);
            var transaction = new NewBusinessTransaction(
                aggregate.TenantId,
                @event.NewBusinessTransactionId,
                @event.AggregateId,
                @event.QuoteId,
                null,
                sequenceNumber,
                @event.InceptionDateTime,
                @event.InceptionTimestamp,
                @event.ExpiryDateTime,
                @event.ExpiryTimestamp,
                @event.Timestamp,
                transactionData,
                @event.ProductReleaseId)
            {
                Environment = @event.Environment,
                TenantId = @event.TenantId,
                ProductId = @event.ProductId,
                CustomerId = @event.CustomerId,
                OrganisationId = policy.OrganisationId,
                IsTestData = @event.IsTestData,
            };
            if (transactionData.CalculationResult != null && transactionData.CalculationResult.PayablePrice != null)
            {
                transaction.TotalPayable = transactionData.CalculationResult.PayablePrice.TotalPayable;
            }

            this.policyTransactionRepository.Add(transaction);
#pragma warning disable CS8602 // We know that aggregate.Policy is not null here.
            aggregate.Policy.ReadModel = policy;
#pragma warning restore CS8602
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.SetPolicyTransactionsEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyAdjustedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read models
                this.policyTransactionRepository.DeleteById(@event.TenantId, @event.AdjustmentTransactionId);
            }

            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId.Value);
            var policy = this.GetPolicyById(@event.TenantId, @event.AggregateId);
            if (policy == null)
            {
                throw new InvalidOperationException(
                    $"When trying to adjust a policy, the policy with ID {@event.AggregateId} did not exist.");
            }

            policy.OnPolicyAdjusted(@event);
            policy.UpdatePolicyState(this.clock.Now());
            if (quote != null)
            {
                quote.PolicyId = policy.Id;
                quote.PolicyNumber = policy.PolicyNumber;
                quote.LastModifiedTimestamp = @event.Timestamp;
            }

            // Write policy transactions
            var transactionData = new PolicyTransactionData(@event.DataSnapshot);
            var transaction = new AdjustmentTransaction(
                aggregate.TenantId,
                @event.AdjustmentTransactionId,
                @event.AggregateId,
                @event.QuoteId.Value,
                quote?.QuoteNumber,
                sequenceNumber,
                @event.EffectiveDateTime,
                @event.EffectiveTimestamp,
                @event.ExpiryDateTime,
                @event.ExpiryTimestamp,
                @event.Timestamp,
                transactionData,
                @event.ProductReleaseId);
            transaction.Environment = quote.Environment;
            transaction.TenantId = quote.TenantId;
            transaction.ProductId = quote.ProductId;
            transaction.CustomerId = quote.CustomerId;
            transaction.OwnerUserId = quote.OwnerUserId;
            transaction.OrganisationId = quote.OrganisationId;
            transaction.IsTestData = policy.IsTestData;
            if (transactionData.CalculationResult != null && transactionData.CalculationResult.PayablePrice != null)
            {
                transaction.TotalPayable = transactionData.CalculationResult.PayablePrice.TotalPayable;
            }

            quote.PolicyTransactionId = transaction.Id;
            this.policyTransactionRepository.Add(transaction);
#pragma warning disable CS8602 // We know that aggregate.Policy is not null here.
            aggregate.Policy.ReadModel = policy;
#pragma warning restore CS8602
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyRenewedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read models
                this.policyTransactionRepository.DeleteById(@event.TenantId, @event.RenewalTransactionId);
            }

            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            var policy = this.GetPolicyById(@event.TenantId, @event.AggregateId);
            if (policy == null)
            {
                throw new InvalidOperationException(
                    $"When trying to renew a policy, the policy with ID {@event.AggregateId} did not exist.");
            }
            this.UpdatePolicy(policy, quote, @event);
            this.UpdateQuote(quote, policy, @event);

            // Write policy transactions
            var transaction = CreateRenewalTransaction(aggregate, sequenceNumber, quote, policy, @event);

            if (quote != null)
            {
                quote.PolicyTransactionId = transaction.Id;
            }
            this.policyTransactionRepository.Add(transaction);
#pragma warning disable CS8602 // We know that aggregate.Policy is not null here.
            aggregate.Policy.ReadModel = policy;
#pragma warning restore CS8602
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyDataPatchedEvent @event, int sequenceNumber)
        {
            var transactions = this.GetUpsertTransactionsForAggregate(@event.TenantId, @event.AggregateId).GetEnumerator();
            PolicyTransaction transaction;
            var lastTransaction = !transactions.MoveNext();
            while (!lastTransaction)
            {
                transaction = transactions.Current;
                transaction.ApplyPatch(@event.PolicyDatPatch);

                lastTransaction = !transactions.MoveNext();
                if (lastTransaction)
                {
                    if (@event.PolicyDatPatch.IsApplicable(transaction))
                    {
                        var policy = this.GetPolicyById(transaction.TenantId, transaction.PolicyId);
                        if (policy != null)
                        {
                            policy.ApplyPatch(@event.PolicyDatPatch);
                        }
                    }
                }
            }

            var quotes = this.GetByAggregateId(@event.TenantId, @event.AggregateId);
            foreach (var quote in quotes)
            {
                var matchesTarget = @event.PolicyDatPatch.Targets
                    .OfType<QuotDataPatchTarget>()
                    .Where(t => t.GetType() == typeof(QuotDataPatchTarget)) // Ignore quote version targets (derived type).
                    .Any(t => t.QuoteId == quote.Id);
                if (matchesTarget)
                {
                    quote.ApplyPatch(@event.PolicyDatPatch);
                }
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyCancelledEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read models
                this.policyTransactionRepository.DeleteById(@event.TenantId, @event.CancellationTransactionId);
            }

            var transactionData = new PolicyTransactionData(@event.DataSnapshot);
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            var policy = this.GetPolicyById(@event.TenantId, quote?.PolicyId);
            if (policy == null)
            {
                throw new ArgumentException("When trying to cancel a policy, the policy read model was not found.");
            }

            policy.RecordCancellation(@event);
            policy.UpdatePolicyState(this.clock.Now());

            // Write policy transactions
            var transaction = new CancellationTransaction(
                aggregate.TenantId,
                @event.CancellationTransactionId,
                @event.AggregateId,
                @event.QuoteId.Value,
                @event.QuoteNumber,
                sequenceNumber,
                @event.EffectiveDateTime,
                @event.EffectiveTimestamp,
                @event.Timestamp,
                transactionData,
                @event.ProductReleaseId);
            if (quote != null)
            {
                quote.PolicyId = policy.Id;
                quote.PolicyNumber = policy.PolicyNumber;
                quote.LastModifiedTimestamp = @event.Timestamp;
                quote.PolicyTransactionId = transaction.Id;
                transaction.Environment = quote.Environment;
                transaction.TenantId = quote.TenantId;
                transaction.ProductId = quote.ProductId;
                transaction.CustomerId = quote.CustomerId;
                transaction.OwnerUserId = quote.OwnerUserId;
                transaction.OrganisationId = quote.OrganisationId;
                transaction.IsTestData = quote.IsTestData;
            }
            if (transactionData.CalculationResult != null && transactionData.CalculationResult.PayablePrice != null)
            {
                transaction.TotalPayable = transactionData.CalculationResult.PayablePrice.TotalPayable;
            }

            this.policyTransactionRepository.Add(transaction);
#pragma warning disable CS8602 // We know that aggregate.Policy is not null here.
            aggregate.Policy.ReadModel = policy;
#pragma warning restore CS8602
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyNumberUpdatedEvent @event, int sequenceNumber)
        {
            var policy = this.GetPolicyById(@event.TenantId, @event.AggregateId);
            if (policy == null)
            {
                throw new InvalidOperationException(
                    $"When trying to update a policy number, the policy with ID {@event.AggregateId} did not exist.");
            }
            policy.PolicyNumber = @event.PolicyNumber;
            policy.LastModifiedTimestamp = @event.Timestamp;
            policy.LastModifiedByUserTimestamp = @event.Timestamp;
            policy.OnPolicyNumberUpdated(@event);
            aggregate.Policy!.ReadModel = policy;
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.FullNameUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var policy in this.GetPoliciesForPerson(@event.TenantId, @event.AggregateId))
            {
                policy.CustomerFullName = @event.Value;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
            }

            foreach (var policy in this.GetPoliciesByOwnerPersonId(@event.TenantId, @event.AggregateId))
            {
                policy.OwnerFullName = @event.Value;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PreferredNameUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var policy in this.GetPoliciesForPerson(@event.TenantId, @event.AggregateId))
            {
                policy.CustomerPreferredName = @event.Value;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.EmailAddressUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var policy in this.GetPoliciesForPerson(@event.TenantId, @event.AggregateId))
            {
                policy.CustomerEmail = @event.Value;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.AlternativeEmailAddressUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var policy in this.GetPoliciesForPerson(@event.TenantId, @event.AggregateId))
            {
                policy.CustomerAlternativeEmail = @event.Value;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.MobilePhoneUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var policy in this.GetPoliciesForPerson(@event.TenantId, @event.AggregateId))
            {
                policy.CustomerMobilePhone = @event.Value;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.HomePhoneUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var policy in this.GetPoliciesForPerson(@event.TenantId, @event.AggregateId))
            {
                policy.CustomerHomePhone = @event.Value;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.WorkPhoneUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var policy in this.GetPoliciesForPerson(@event.TenantId, @event.AggregateId))
            {
                policy.CustomerWorkPhone = @event.Value;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var policy in this.GetPoliciesForPerson(@event.TenantId, @event.AggregateId))
            {
                policy.CustomerFullName = @event.PersonData.FullName;
                policy.CustomerPreferredName = @event.PersonData.PreferredName;
                policy.CustomerEmail = @event.PersonData.Email;
                policy.CustomerAlternativeEmail = @event.PersonData.AlternativeEmail;
                policy.CustomerMobilePhone = @event.PersonData.MobilePhone;
                policy.CustomerHomePhone = @event.PersonData.HomePhone;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
            }

            foreach (var policy in this.GetPoliciesByOwnerPersonId(@event.TenantId, @event.AggregateId))
            {
                policy.OwnerFullName = @event.PersonData.FullName;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteRollbackEvent @event, int sequenceNumber)
        {
            this.ReplayEvents(aggregate, @event);
            this.RollbackStrippedEvents(aggregate, @event);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            var transactions = this.GetUpsertTransactionsForAggregate(@event.TenantId, @event.AggregateId);

            foreach (var transaction in transactions)
            {
                transaction.TenantId = @event.TenantId;
                transaction.ProductId = @event.ProductId;
            }

            var policy = this.GetPolicyById(@event.TenantId, @event.AggregateId);
            if (policy != null)
            {
                policy.TenantId = @event.TenantId;
                policy.ProductId = @event.ProductId;
                policy.LastModifiedTimestamp = @event.Timestamp;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
            }
        }

        public void Handle(
            QuoteAggregate aggregate,
            AdditionalPropertyValueUpdatedEvent<QuoteAggregate, IQuoteEventObserver> @event,
            int sequenceNumber)
        {
            var policy = this.GetPolicyById(@event.TenantId, @event.EntityId);
            if (policy != null)
            {
                policy.LastModifiedTimestamp = @event.Timestamp;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
            }

            var policyTransaction = this.policyTransactionRepository
                .GetById(@event.TenantId, @event.EntityId);

            if (policyTransaction != null)
            {
                policyTransaction.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(
            QuoteAggregate aggregate,
            QuoteAggregate.QuoteTransferredToAnotherOrganisationEvent @event,
            int sequenceNumber)
        {
            var transactions = this.GetUpsertTransactionsForAggregate(@event.TenantId, @event.AggregateId);
            foreach (var transaction in transactions)
            {
                transaction.OrganisationId = @event.OrganisationId;
                transaction.LastModifiedTimestamp = @event.Timestamp;
            }

            var policy = this.GetPolicyById(@event.TenantId, @event.AggregateId);
            if (policy != null)
            {
                policy.OrganisationId = @event.OrganisationId;
                policy.LastModifiedTimestamp = @event.Timestamp;
                if (@event.PerformingUserId != null)
                {
                    policy.LastModifiedByUserTimestamp = @event.Timestamp;
                }
            }
        }

        public void Handle(
            PersonAggregate aggregate,
            PersonAggregate.PersonTransferredToAnotherOrganisationEvent @event,
            int sequenceNumber)
        {
            foreach (var policy in this.GetPoliciesForPerson(@event.TenantId, @event.AggregateId))
            {
                policy.OrganisationId = @event.OrganisationId;
                policy.LastModifiedTimestamp = @event.Timestamp;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
            }

            foreach (var policy in this.GetPoliciesByOwnerPersonId(@event.TenantId, @event.AggregateId))
            {
                policy.OrganisationId = @event.OrganisationId;
                policy.LastModifiedTimestamp = @event.Timestamp;
                policy.LastModifiedByUserTimestamp = @event.Timestamp;
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.MigrateQuotesAndPolicyTransactionsToNewProductReleaseEvent @event, int sequenceNumber)
        {
            var policyTransactions = this.GetPolicyTransactionsByProductReleaseId(@event.TenantId, @event.AggregateId, @event.OrginalProductReleaseId);

            foreach (var policyTransaction in policyTransactions)
            {
                policyTransaction.ProductReleaseId = @event.NewProductReleaseId;
                policyTransaction.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.MigrateUnassociatedEntitiesToProductReleaseEvent @event, int sequenceNumber)
        {
            var policyTransactions = this.GetPolicyTransactionsWithoutAssociation(@event.TenantId, @event.AggregateId);

            foreach (var policyTransaction in policyTransactions)
            {
                policyTransaction.ProductReleaseId = @event.NewProductReleaseId;
                policyTransaction.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        private static RenewalTransaction CreateRenewalTransaction(
            QuoteAggregate aggregate, int sequenceNumber, NewQuoteReadModel? quote, PolicyReadModel policy, PolicyRenewedEvent @event)
        {
            var transactionData = new PolicyTransactionData(@event.DataSnapshot);
            var transaction = new RenewalTransaction(
                aggregate.TenantId,
                @event.RenewalTransactionId,
                @event.PolicyId,
                @event.QuoteId,
                quote?.QuoteNumber,
                sequenceNumber,
                @event.EffectiveDateTime,
                @event.EffectiveTimestamp,
                @event.ExpiryDateTime,
                @event.ExpiryTimestamp,
                @event.Timestamp,
                transactionData,
                @event.ProductReleaseId)
            {
                Environment = quote?.Environment ?? policy.Environment,
                TenantId = quote?.TenantId ?? policy.TenantId,
                ProductId = quote?.ProductId ?? policy.ProductId,
                CustomerId = quote?.CustomerId ?? policy.CustomerId,
                OwnerUserId = quote?.OwnerUserId ?? policy.OwnerUserId,
                OrganisationId = quote?.OrganisationId ?? policy.OrganisationId,
                IsTestData = policy.IsTestData,
                TotalPayable = transactionData.CalculationResult?.PayablePrice?.TotalPayable,
            };
            return transaction;
        }

        private NewQuoteReadModel? GetQuoteById(Guid tenantId, Guid? quoteId)
        {
            return quoteId != null ? this.quoteReadModelRepository.GetById(tenantId, quoteId.Value) : null;
        }

        private IEnumerable<NewQuoteReadModel> GetByAggregateId(Guid tenantId, Guid aggregateId)
        {
            return this.quoteReadModelRepository
                .Where(tenantId, q => q.AggregateId == aggregateId)
                .ToList();
        }

        private IEnumerable<PolicyTransaction> GetUpsertTransactionsForAggregate(Guid tenantId, Guid aggregateId)
        {
            return this.policyTransactionRepository
                .Where(tenantId, x => x.PolicyId == aggregateId);
        }

        private IEnumerable<PolicyTransaction> GetPolicyTransactionsByProductReleaseId(Guid tenantId, Guid aggregateId, Guid productReleaseId)
        {
            return this.policyTransactionRepository
                .Where(tenantId, x => x.PolicyId == aggregateId && x.ProductReleaseId == productReleaseId);
        }

        private IEnumerable<PolicyTransaction> GetPolicyTransactionsWithoutAssociation(Guid tenantId, Guid aggregateId)
        {
            return this.policyTransactionRepository
                .Where(tenantId, x => x.PolicyId == aggregateId && x.ProductReleaseId == null);
        }

        private PolicyReadModel? GetPolicyById(Guid? tenantId, Guid? policyId)
        {
            if (!policyId.HasValue || !tenantId.HasValue)
            {
                return null;
            }

            Maybe<PolicyReadModel> maybe = this.policyReadModelRepository
                .GetByIdMaybe(tenantId.Value, policyId.Value);

            return maybe.HasValue
                ? maybe.Value
                : null;
        }

        private IEnumerable<PolicyReadModel> GetPoliciesForPerson(Guid tenantId, Guid personId)
        {
            return this.policyReadModelRepository
                .Where(tenantId, p => p.CustomerPersonId == personId);
        }

        private IEnumerable<PolicyReadModel> GetPoliciesByOwnerPersonId(Guid tenantId, Guid personId)
        {
            return this.policyReadModelRepository
                .Where(tenantId, p => p.OwnerPersonId == personId);
        }

        private void UpdateQuote(NewQuoteReadModel? quote, PolicyReadModel policy, PolicyRenewedEvent @event)
        {
            if (quote != null)
            {
                quote.PolicyId = policy.Id;
                quote.PolicyNumber = policy.PolicyNumber;
                quote.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        private void UpdatePolicy(PolicyReadModel policy, NewQuoteReadModel? quote, PolicyRenewedEvent @event)
        {
            policy.PolicyUpsert(@event);
            policy.LatestPolicyPeriodStartDateTime = @event.EffectiveDateTime;
            policy.LatestPolicyPeriodStartTimestamp = @event.EffectiveTimestamp;
            policy.PolicyTitle = quote?.QuoteTitle ?? policy.PolicyTitle;
            policy.RecordRenewal(@event);
            policy.UpdatePolicyState(this.clock.Now());
        }

        private void ReplayEvents(QuoteAggregate aggregate, QuoteRollbackEvent @event)
        {
            // Replay events to update policy details
            int sequenceNumber = 0;
            foreach (IEvent<QuoteAggregate, Guid> replayEvent in @event.ReplayEvents)
            {
                if (replayEvent is IPolicyUpsertEvent upsertEvent)
                {
                    var policy = this.GetPolicyById(@event.TenantId, @event.AggregateId);

                    // If policy doesn't exist due to previous rollback
                    if (policy == null)
                    {
                        if (replayEvent is PolicyIssuedEvent policyIssuedEvent)
                        {
                            var quote = this.quoteReadModelRepository.GetById(policyIssuedEvent.TenantId, policyIssuedEvent.QuoteId.Value);
                            policy = new PolicyReadModel(policyIssuedEvent, quote);
                        }
                        else if (replayEvent is PolicyImportedEvent policyImportedEvent)
                        {
                            policy = new PolicyReadModel(policyImportedEvent);
                        }
                        else if (replayEvent is PolicyIssuedWithoutQuoteEvent policyCreatededEvent)
                        {
                            policy = new PolicyReadModel(policyCreatededEvent);
                        }

                        this.policyReadModelRepository.Add(policy);
                    }

                    policy.PolicyUpsert(upsertEvent);
                    policy.UpdatePolicyState(this.clock.Now());
                }

                sequenceNumber++;
            }
        }

        private void RollbackStrippedEvents(QuoteAggregate aggregate, QuoteRollbackEvent @event)
        {
            // Rollback should delete policies & transactions created during PolicyIssued & PolicyImported events.
            var strippedEvents = @event.StrippedEvents;
            foreach (var strippedEvent in strippedEvents)
            {
                if (strippedEvent is IPolicyCreatedEvent ||
                    strippedEvent is AggregateCreationFromPolicyEvent)
                {
                    var tenant = this.tenantRepository.GetTenantById(strippedEvent.TenantId);
                    this.policyReadModelRepository.DeleteById(strippedEvent.TenantId, strippedEvent.AggregateId);
                    this.lucenePolicyRepository.DeleteItemsFromIndex(
                    tenant,
                    aggregate.Environment,
                    new List<Guid>
                    {
                        strippedEvent.AggregateId,
                    });
                }

                if (strippedEvent is PolicyIssuedEvent policyIssuedEvent)
                {
                    this.policyTransactionRepository.Delete(
                        strippedEvent.TenantId,
                        transaction =>
                            transaction.Id == policyIssuedEvent.NewBusinessTransactionId);
                }
                else if (strippedEvent is PolicyAdjustedEvent policyAdjustedEvent)
                {
                    this.policyTransactionRepository.Delete(
                        strippedEvent.TenantId,
                        transaction =>
                            transaction.Id == policyAdjustedEvent.AdjustmentTransactionId);
                }
                else if (strippedEvent is PolicyRenewedEvent policyRenewedEvent)
                {
                    this.policyTransactionRepository.Delete(
                        strippedEvent.TenantId,
                        transaction =>
                            transaction.Id == policyRenewedEvent.RenewalTransactionId);
                }
                else if (strippedEvent is PolicyCancelledEvent policyCancelledEvent)
                {
                    this.policyTransactionRepository.Delete(
                        strippedEvent.TenantId,
                        transaction =>
                            transaction.Id == policyCancelledEvent.CancellationTransactionId);

                    var policy = this.GetPolicyById(@event.TenantId, @event.AggregateId);
                    if (policy != null)
                    {
                        policy.Uncancel();
                        policy.UpdatePolicyState(this.clock.Now());
                    }
                }
                else if (strippedEvent is IPolicyUpsertEvent upsertEvent)
                {
                    // Delete transactions inside stripped events
                    var createdTicksSinceEpoch = upsertEvent.Timestamp.ToUnixTimeTicks();
                    this.policyTransactionRepository.Delete(
                        strippedEvent.TenantId,
                        transaction =>
                            transaction.PolicyId == strippedEvent.AggregateId &&
                            transaction.CreatedTicksSinceEpoch == createdTicksSinceEpoch);
                }
            }
        }
    }
}
