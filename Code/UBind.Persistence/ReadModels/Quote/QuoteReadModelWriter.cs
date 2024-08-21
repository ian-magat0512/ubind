// <copyright file="QuoteReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using LinqKit;
    using Microsoft.AspNetCore.JsonPatch;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.ValueTypes;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;

    /// <summary>
    /// For updating the quote read model in response to events from the write model.
    /// </summary>
    public class QuoteReadModelWriter : IQuoteReadModelWriter
    {
        private readonly IWritableReadModelRepository<NewQuoteReadModel> quoteReadModelRepository;
        private readonly IWritableReadModelRepository<PolicyReadModel> policyReadModelRepository;
        private readonly IWritableReadModelRepository<PolicyTransaction> writablePolicyTransactionRepository;
        private readonly IWritableReadModelRepository<CustomerReadModel> customerReadModelRepository;
        private readonly ITenantRepository tenantRepository;
        private readonly IQuoteFileAttachmentRepository quoteFileAttachmentRepository;
        private readonly IProductRepository productRepository;
        private readonly IQuoteReadModelRepository quoteRepository;
        private readonly PropertyTypeEvaluatorService propertyTypeEvaluatorService;
        private readonly IWritableReadModelRepository<QuoteFileAttachmentReadModel> fileAttachmentRepository;
        private readonly IWritableReadModelRepository<PersonReadModel> personReadModelRepository;
        private readonly IClock clock;
        private readonly IPolicyTransactionTimeOfDayScheme timeOfDayScheme;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly IProductReleaseService productReleaseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteReadModelWriter"/> class.
        /// </summary>
        /// <param name="quoteReadModelRepository">The repository for quote read models.</param>
        /// <param name="policyReadModelRepository">The repository for policy read models.</param>
        /// <param name="fileAttachmentRepository">The repository for quote file attachments.</param>
        /// <param name="customerReadModelRepository">The repository for customer read models.</param>
        /// <param name="personReadModelRepository">The repository for person read models.</param>
        /// <param name="productRepository">The repository for products.</param>
        /// <param name="tenantRepository">The repository for tenants.</param>
        /// <param name="timeOfDayScheme">The transaction time of day scheme.</param>
        /// <param name="productConfigurationProvider">The product configuration provider .</param>
        public QuoteReadModelWriter(
            IWritableReadModelRepository<NewQuoteReadModel> quoteReadModelRepository,
            IWritableReadModelRepository<PolicyReadModel> policyReadModelRepository,
            IWritableReadModelRepository<PolicyTransaction> writablePolicyTransactionRepository,
            IWritableReadModelRepository<QuoteFileAttachmentReadModel> fileAttachmentRepository,
            IWritableReadModelRepository<CustomerReadModel> customerReadModelRepository,
            IWritableReadModelRepository<PersonReadModel> personReadModelRepository,
            IProductRepository productRepository,
            IQuoteReadModelRepository quoteRepository,
            PropertyTypeEvaluatorService propertyTypeEvaluatorService,
            IQuoteFileAttachmentRepository quoteFileAttachmentRepository,
            ITenantRepository tenantRepository,
            IClock clock,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme,
            IProductConfigurationProvider productConfigurationProvider,
            IProductReleaseService productReleaseService)
        {
            this.tenantRepository = tenantRepository;
            this.quoteFileAttachmentRepository = quoteFileAttachmentRepository;
            this.productRepository = productRepository;
            this.quoteRepository = quoteRepository;
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.policyReadModelRepository = policyReadModelRepository;
            this.writablePolicyTransactionRepository = writablePolicyTransactionRepository;
            this.customerReadModelRepository = customerReadModelRepository;
            this.propertyTypeEvaluatorService = propertyTypeEvaluatorService;
            this.fileAttachmentRepository = fileAttachmentRepository;
            this.personReadModelRepository = personReadModelRepository;
            this.clock = clock;
            this.timeOfDayScheme = timeOfDayScheme;
            this.productConfigurationProvider = productConfigurationProvider;
            this.productReleaseService = productReleaseService;
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

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteInitializedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read model
                this.quoteReadModelRepository.DeleteById(@event.TenantId, @event.QuoteId);
            }

            var quote = new NewQuoteReadModel(@event)
            {
                CustomerId = @event.CustomerId,
                OrganisationId = aggregate.OrganisationId,
                Environment = @event.Environment,
                TenantId = @event.TenantId,
                ProductId = @event.ProductId,
            };

            if (quote.ProductId == default)
            {
                Product product =
                   this.productRepository.GetProductByStringId(@event.TenantId, @event.ProductStringId);

                quote.ProductId = product.Id;
            }

            if (@event.AdditionalProperties != null)
            {
                foreach (var additionalProperty in @event.AdditionalProperties)
                {
                    this.propertyTypeEvaluatorService.CreateNewAdditionalPropertyValueByPropertyType(
                       (TGuid<Tenant>)@event.TenantId,
                       quote.Id,
                       additionalProperty.Type,
                       (TGuid<AdditionalPropertyDefinition>)additionalProperty.DefinitionId,
                       (TGuid<AdditionalPropertyValue>)Guid.NewGuid(), // the additional property value id is generated here
                       additionalProperty.Value);
                }
            }

            quote.IsTestData = @event.IsTestData;
            this.quoteReadModelRepository.Add(quote);
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteImportedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                this.quoteReadModelRepository.DeleteById(@event.TenantId, @event.QuoteId);
            }

            var quote = new NewQuoteReadModel(@event);
            quote.QuoteState = @event.QuoteState;
            quote.QuoteNumber = @event.QuoteNumber;
            quote.LatestCalculationResultId = @event.CalculationResultId;
            quote.LatestCalculationResult = @event.CalculationResult;
            quote.LatestCalculationResultFormDataId = @event.CalculationResult.FormDataId.Value;

            quote.CustomerPersonId = @event.PersonId;
            quote.CustomerFullName = @event.CustomerDetails.FullName;
            quote.CustomerPreferredName = @event.CustomerDetails.PreferredName;
            quote.CustomerEmail = @event.CustomerDetails.Email;
            quote.CustomerAlternativeEmail = @event.CustomerDetails.AlternativeEmail;
            quote.CustomerMobilePhone = @event.CustomerDetails.MobilePhone;
            quote.CustomerHomePhone = @event.CustomerDetails.HomePhone;
            quote.CustomerWorkPhone = @event.CustomerDetails.WorkPhone;
            this.quoteReadModelRepository.Add(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.AdjustmentQuoteCreatedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read model
                this.quoteReadModelRepository.DeleteById(@event.TenantId, @event.QuoteId);
            }

            var quote = new NewQuoteReadModel(aggregate, @event);
            if (@event.AdditionalProperties != null)
            {
                foreach (var additionalProperty in @event.AdditionalProperties)
                {
                    this.propertyTypeEvaluatorService.CreateNewAdditionalPropertyValueByPropertyType(
                       (TGuid<Tenant>)@event.TenantId,
                       quote.Id,
                       additionalProperty.Type,
                       (TGuid<AdditionalPropertyDefinition>)additionalProperty.DefinitionId,
                       (TGuid<AdditionalPropertyValue>)Guid.NewGuid(), // the additional property value id is generated here
                       additionalProperty.Value);
                }
            }

            this.UpdateQuoteWithPolicyDetails(@event.TenantId, quote, @event.AggregateId);
            this.quoteReadModelRepository.Add(quote);
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.CancellationQuoteCreatedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read model
                this.quoteReadModelRepository.DeleteById(@event.TenantId, @event.QuoteId);
            }

            var quote = new NewQuoteReadModel(aggregate, @event);
            if (@event.AdditionalProperties != null)
            {
                foreach (var additionalProperty in @event.AdditionalProperties)
                {
                    this.propertyTypeEvaluatorService.CreateNewAdditionalPropertyValueByPropertyType(
                       (TGuid<Tenant>)@event.TenantId,
                       quote.Id,
                       additionalProperty.Type,
                       (TGuid<AdditionalPropertyDefinition>)additionalProperty.DefinitionId,
                       (TGuid<AdditionalPropertyValue>)Guid.NewGuid(), // the additional property value id is generated here
                       additionalProperty.Value);
                }
            }

            this.UpdateQuoteWithPolicyDetails(@event.TenantId, quote, @event.AggregateId);
            this.quoteReadModelRepository.Add(quote);
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.CalculationResultCreatedEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            quote.LatestCalculationResultId = @event.CalculationResultId;
            quote.LatestCalculationResult = @event.CalculationResult;
            quote.LatestCalculationResultFormDataId = @event.CalculationResult.FormDataId.Value;
            if (@event.CalculationResult != null && @event.CalculationResult.PayablePrice != null)
            {
                quote.TotalPayable = @event.CalculationResult.PayablePrice.TotalPayable;
            }

            quote.LastModifiedTimestamp = @event.Timestamp;
            quote.LastModifiedByUserTimestamp = @event.Timestamp;
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.CustomerAssignedEvent @event, int sequenceNumber)
        {
            var affectedQuotes = this.GetByAggregateId(@event.TenantId, @event.AggregateId);
            foreach (var quote in affectedQuotes)
            {
                quote.CustomerId = @event.CustomerId;
                quote.CustomerPersonId = @event.PersonId;
                quote.CustomerFullName = @event.CustomerDetails.FullName;
                quote.CustomerPreferredName = @event.CustomerDetails.PreferredName;
                quote.CustomerEmail = @event.CustomerDetails.Email;
                quote.CustomerAlternativeEmail = @event.CustomerDetails.AlternativeEmail;
                quote.CustomerMobilePhone = @event.CustomerDetails.MobilePhone;
                quote.CustomerHomePhone = @event.CustomerDetails.HomePhone;
                quote.CustomerWorkPhone = @event.CustomerDetails.WorkPhone;

                // as the customerId was changed, change the content of the formdata as well.
                if (quote.LatestFormData != null)
                {
                    var formData = new UBind.Domain.Aggregates.Quote.FormData(quote.LatestFormData);
                    formData.PatchFormModelProperty(new JsonPath("contactEmail"), @event.CustomerDetails.Email);
                    formData.PatchFormModelProperty(new JsonPath("contactName"), @event.CustomerDetails.FullName);
                    formData.PatchFormModelProperty(new JsonPath("contactMobile"), @event.CustomerDetails.MobilePhoneNumber);
                    formData.PatchFormModelProperty(new JsonPath("contactPhone"), @event.CustomerDetails.HomePhoneNumber);
                    quote.LatestFormData = formData.Json;
                }

                if (quote.LatestCalculationResult != null)
                {
                    quote.LatestCalculationResult.PatchProperty(new JsonPath("contactEmail"), @event.CustomerDetails.Email);
                    quote.LatestCalculationResult.PatchProperty(new JsonPath("contactName"), @event.CustomerDetails.FullName);
                    quote.LatestCalculationResult.PatchProperty(new JsonPath("contactMobile"), @event.CustomerDetails.MobilePhoneNumber);
                    quote.LatestCalculationResult.PatchProperty(new JsonPath("contactPhone"), @event.CustomerDetails.HomePhoneNumber);
                }

                aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteExpiryTimestampSetEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            quote.ExpiryTimestamp = @event.ExpiryTimestamp;
            var aggregateQuote = aggregate.GetQuoteOrThrow(@event.QuoteId);
            quote.QuoteState = aggregateQuote.QuoteStatus;
            if (aggregateQuote.IsExpired(this.clock.Now()))
            {
                quote.QuoteState = StandardQuoteStates.Expired;
            }

            quote.LastModifiedTimestamp = @event.Timestamp;
            quote.LastModifiedByUserTimestamp = @event.Timestamp;
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.CustomerDetailsUpdatedEvent @event, int sequenceNumber)
        {
            var affectedQuotes = this.GetByAggregateId(@event.TenantId, @event.AggregateId);
            foreach (var quote in affectedQuotes)
            {
                if (quote.CustomerId == null || quote.CustomerId.GetValueOrDefault() == default)
                {
                    continue;
                }

                var customer = this.GetCustomerById(quote.TenantId, quote.CustomerId.Value);
                if (customer == null)
                {
                    return;
                }

                var person = this.GetPersonById(customer.TenantId, customer.PrimaryPersonId);
                if (person == null)
                {
                    return;
                }

                if (@event.CustomerDetails.FullName != null)
                {
                    person.FullName = @event.CustomerDetails.FullName;
                    quote.CustomerFullName = @event.CustomerDetails.FullName;
                }

                if (@event.CustomerDetails.PreferredName != null)
                {
                    person.PreferredName = @event.CustomerDetails.PreferredName;
                    quote.CustomerPreferredName = @event.CustomerDetails.PreferredName;
                }

                if (@event.CustomerDetails.Email != null)
                {
                    person.Email = @event.CustomerDetails.Email;
                    quote.CustomerEmail = @event.CustomerDetails.Email;
                }

                if (@event.CustomerDetails.AlternativeEmail != null)
                {
                    person.AlternativeEmail = @event.CustomerDetails.AlternativeEmail;
                    quote.CustomerAlternativeEmail = @event.CustomerDetails.AlternativeEmail;
                }

                if (@event.CustomerDetails.MobilePhone != null)
                {
                    person.MobilePhoneNumber = @event.CustomerDetails.MobilePhone;
                    quote.CustomerMobilePhone = @event.CustomerDetails.MobilePhone;
                }

                if (@event.CustomerDetails.HomePhone != null)
                {
                    person.HomePhoneNumber = @event.CustomerDetails.HomePhone;
                    quote.CustomerHomePhone = @event.CustomerDetails.HomePhone;
                }

                if (@event.CustomerDetails.WorkPhone != null)
                {
                    person.WorkPhoneNumber = @event.CustomerDetails.WorkPhone;
                    quote.CustomerWorkPhone = @event.CustomerDetails.WorkPhone;
                }

                person.LastModifiedTimestamp = @event.Timestamp;
                customer.LastModifiedTimestamp = @event.Timestamp;
                aggregate.GetQuote(quote.Id)?.SetReadModel(quote);
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.FileAttachedEvent @event, int sequenceNumber)
        {
            var attachment = this.quoteFileAttachmentRepository.GetById(@event.AttachmentId);

            // already exists.
            if (attachment != null)
            {
                // if exist but outdated tenant id, update.
                if (attachment.TenantId == Guid.Empty)
                {
                    this.quoteFileAttachmentRepository.UpdateTenantId(@event.TenantId, @event.AttachmentId);
                }

                return;
            }

            var fileAttachment = new QuoteFileAttachment(
                @event.TenantId,
                @event.AttachmentId,
                @event.QuoteId,
                @event.FileContentId,
                @event.Name,
                @event.Type,
                @event.FileSize,
                @event.Timestamp);
            this.fileAttachmentRepository.Add(QuoteFileAttachmentReadModel.Create(fileAttachment));
        }

        public async Task Handle(QuoteAggregate aggregate, FormDataUpdatedEvent @event, int sequenceNumber)
        {
            var quoteReadModel = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            quoteReadModel.LatestFormData = @event.FormData;
            quoteReadModel.LastModifiedTimestamp = @event.Timestamp;
            quoteReadModel.LastModifiedByUserTimestamp = @event.Timestamp;
            var quote = aggregate.GetQuoteOrThrow(@event.QuoteId);
            var productReleaseId = quote.ProductReleaseId
                ?? this.productReleaseService.GetDefaultProductReleaseId(aggregate.TenantId, aggregate.ProductId, aggregate.Environment);
            if (productReleaseId == null)
            {
                throw new InvalidOperationException("Could not find a default product release for the quote. This is unexpected.");
            }

            var releaseContext = new ReleaseContext(
                aggregate.TenantId,
                aggregate.ProductId,
                aggregate.Environment,
                productReleaseId.Value);
            var configuration = await this.productConfigurationProvider.GetProductConfiguration(
                releaseContext,
                WebFormAppType.Quote);
            var quoteDataRetriever = new StandardQuoteDataRetriever(
                configuration, new FormData(@event.FormData), quote.LatestCalculationResult?.Data);
            LocalDate? effectiveDateMaybe;
            if (quote.Type == QuoteType.NewBusiness)
            {
                effectiveDateMaybe = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.InceptionDate);
                LocalDate? expiryDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.ExpiryDate);

                if (expiryDate.HasValue)
                {
                    LocalDateTime expiryDateTime = expiryDate.Value.At(this.timeOfDayScheme.GetEndTime());
                    Instant expiryTimestamp = expiryDateTime.InZoneLeniently(quote.TimeZone).ToInstant();
                    quoteReadModel.PolicyExpiryTimestamp = expiryTimestamp;
                }
            }
            else
            {
                effectiveDateMaybe = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.EffectiveDate);
                if (!effectiveDateMaybe.HasValue && quote.Type == QuoteType.Renewal)
                {
                    effectiveDateMaybe = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.InceptionDate);
                }
            }

            if (effectiveDateMaybe.HasValue)
            {
                var effectiveDate = effectiveDateMaybe.Value;
                var effectiveDateTime = this.GetEffectiveTimestampForEffectiveDateUsingTimeOfDaySchemeBasedOnQuoteType(
                    effectiveDate, @event.Timestamp, quote.TimeZone, quote.Type);
                var effectiveTimestamp = effectiveDateTime.InZoneLeniently(quote.TimeZone).ToInstant();
                quoteReadModel.PolicyTransactionEffectiveTimestamp = effectiveTimestamp;
            }
            quote.SetReadModel(quoteReadModel);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.FormDataPatchedEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);

            JsonPatchDocument jsonPatchDocument = @event.FormDataPatch;
            ExpandoObject formDataExpandoObject =
                JsonConvert.DeserializeObject<ExpandoObject>(quote.LatestFormData, new ExpandoObjectConverter());
            jsonPatchDocument.ApplyTo(formDataExpandoObject);
            var formData = JsonConvert.SerializeObject(formDataExpandoObject);

            quote.LatestFormData = formData;
            quote.LastModifiedTimestamp = @event.Timestamp;
            quote.LastModifiedByUserTimestamp = @event.Timestamp;
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.FundingProposalCreatedEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            quote.RecordFunding(@event.FundingProposal, @event.Timestamp);
            quote.LastModifiedTimestamp = @event.Timestamp;
            quote.LastModifiedByUserTimestamp = @event.Timestamp;
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.FundingProposalAcceptedEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            quote.RecordFunding(@event.FundingProposal, @event.Timestamp);
            quote.IsFunded = true;
            quote.LastModifiedTimestamp = @event.Timestamp;
            quote.LastModifiedByUserTimestamp = @event.Timestamp;
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.InvoiceIssuedEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            quote.RecordInvoiceIssued(@event.InvoiceNumber, @event.Timestamp);
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.CreditNoteIssuedEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            quote.RecordCreditNoteIssued(@event.CreditNoteNumber, @event.Timestamp);
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.OwnershipAssignedEvent @event, int sequenceNumber)
        {
            var affectedQuotes = this.GetByAggregateId(@event.TenantId, @event.AggregateId);
            foreach (var quote in affectedQuotes)
            {
                quote.OwnerUserId = @event.UserId;
                quote.OwnerPersonId = @event.PersonId;
                quote.OwnerFullName = @event.FullName;
                quote.LastModifiedTimestamp = @event.Timestamp;
                quote.LastModifiedByUserTimestamp = @event.Timestamp;
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.OwnershipUnassignedEvent @event, int sequenceNumber)
        {
            var policy = this.GetPolicyById(@event.TenantId, @event.AggregateId);
            if (policy != null)
            {
                policy.OwnerUserId = default;
                policy.OwnerPersonId = default;
                policy.OwnerFullName = string.Empty;
                policy.LastModifiedTimestamp = @event.Timestamp;
            }

            var affectedQuotes = this.GetByAggregateId(@event.TenantId, @event.AggregateId);
            foreach (var quote in affectedQuotes)
            {
                quote.OwnerUserId = default;
                quote.LastModifiedTimestamp = @event.Timestamp;
                quote.LastModifiedByUserTimestamp = @event.Timestamp;
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PaymentMadeEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            quote.PolicyId = @event.AggregateId;
            quote.IsPaidFor = true;
            quote.PaymentTimestamp = @event.Timestamp;
            quote.PaymentReference = @event.PaymentDetails.Reference;
            quote.PaymentGateway = Enum.GetName(typeof(PaymentGatewayName), @event.PaymentDetails.PaymentGateway);
            quote.PaymentResponseJson = @event.PaymentDetails.Response;
            quote.LastModifiedTimestamp = @event.Timestamp;
            quote.LastModifiedByUserTimestamp = @event.Timestamp;
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.AggregateCreationFromPolicyEvent @event, int sequenceNumber)
        {
            if (!@event.QuoteId.HasValue)
            {
                return;
            }
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId.Value);
            if (quote == null)
            {
                quote = new NewQuoteReadModel(@event)
                {
                    PolicyId = @event.AggregateId,
                    PolicyNumber = @event.PolicyNumber,
                    CustomerId = @event.CustomerId,
                    CustomerPersonId = @event.PersonId,
                    CustomerFullName = @event.DataSnapshot.CustomerDetails.Data.FullName,
                    CustomerPreferredName = @event.DataSnapshot.CustomerDetails.Data.PreferredName,
                    CustomerEmail = @event.DataSnapshot.CustomerDetails.Data.Email,
                    CustomerAlternativeEmail = @event.DataSnapshot.CustomerDetails.Data.AlternativeEmail,
                    CustomerMobilePhone = @event.DataSnapshot.CustomerDetails.Data.MobilePhone,
                    CustomerHomePhone = @event.DataSnapshot.CustomerDetails.Data.HomePhone,
                    CustomerWorkPhone = @event.DataSnapshot.CustomerDetails.Data.WorkPhone,
                    PolicyTransactionId = @event.AggregateId,
                };

                if (@event.OrganisationId == default)
                {
                    var tenant = this.tenantRepository.GetTenantById(quote.TenantId);
                    if (tenant == null)
                    {
                        throw new ErrorException(Errors.General.NotFound("tenant", quote.TenantId));
                    }

                    quote.OrganisationId = tenant.Details.DefaultOrganisationId;
                }
                else
                {
                    quote.OrganisationId = @event.OrganisationId;
                }

                if (@event.DataSnapshot.FormData != null)
                {
                    quote.LatestFormData = @event.DataSnapshot.FormData.Data.Json;
                    if (@event.DataSnapshot.FormData.CreatedTimestamp > quote.LastModifiedTimestamp)
                    {
                        quote.LastModifiedTimestamp = @event.DataSnapshot.FormData.CreatedTimestamp;
                        quote.LastModifiedByUserTimestamp = @event.DataSnapshot.FormData.CreatedTimestamp;
                    }
                }

                if (@event.DataSnapshot.CalculationResult != null)
                {
                    quote.LatestCalculationResultId = @event.DataSnapshot.CalculationResult.Id;
                    quote.LatestCalculationResultFormDataId = @event.DataSnapshot.CalculationResult.Data.FormDataId.Value;
                    quote.LatestCalculationResultJson = @event.DataSnapshot.CalculationResult.Data.Json;
                    if (@event.DataSnapshot.CalculationResult.CreatedTimestamp > quote.LastModifiedTimestamp)
                    {
                        quote.LastModifiedTimestamp = @event.DataSnapshot.CalculationResult.CreatedTimestamp;
                        quote.LastModifiedByUserTimestamp = @event.DataSnapshot.FormData.CreatedTimestamp;
                    }
                }

                this.quoteReadModelRepository.Add(quote);
                aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteBoundEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            if (quote != null)
            {
                quote.LastModifiedTimestamp = @event.Timestamp;
                quote.LastModifiedByUserTimestamp = @event.Timestamp;
                aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteDiscardEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            if (quote.Type == QuoteType.NewBusiness && quote.PolicyId != null)
            {
                throw new InvalidOperationException(
                    "An attempt has been made to discard a quote which has been bound into a policy. "
                    + "This is forbidden.");
            }

            quote.RecordDiscarding(@event.Timestamp);
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, PolicyDeletedEvent @event, int sequenceNumber)
        {
            if (aggregate.Policy != null)
            {
                var policyId = aggregate.Policy.PolicyId;
                var quoteIds = this.quoteRepository.ListQuoteIdsFromPolicy(@event.TenantId, policyId, aggregate.Environment);
                quoteIds.ToList().ForEach(q =>
                {
                    this.quoteReadModelRepository.DeleteById(@event.TenantId, q);
                    var attachmentIds = this.quoteFileAttachmentRepository.GetAttachmentIdsForQuote(@event.TenantId, q);
                    attachmentIds.ForEach(a => this.fileAttachmentRepository.DeleteById(@event.TenantId, a));
                });

                this.policyReadModelRepository.DeleteById(@event.TenantId, policyId);
                this.writablePolicyTransactionRepository.Delete(@event.TenantId, p => p.PolicyId == policyId);
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteNumberAssignedEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            quote.QuoteNumber = @event.QuoteNumber;
            quote.LastModifiedTimestamp = @event.Timestamp;
            quote.LastModifiedByUserTimestamp = @event.Timestamp;
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteTitleAssignedEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            quote.QuoteTitle = @event.QuoteTitle;
            quote.LastModifiedTimestamp = @event.Timestamp;
            quote.LastModifiedByUserTimestamp = @event.Timestamp;
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteSubmittedEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            quote.IsSubmitted = true;
            quote.SubmissionTimestamp = @event.Timestamp;
            quote.LastModifiedTimestamp = @event.Timestamp;
            quote.LastModifiedByUserTimestamp = @event.Timestamp;
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyIssuedEvent @event, int sequenceNumber)
        {
            if (!@event.QuoteId.HasValue)
            {
                return;
            }
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId.Value);
            if (quote != null)
            {
                quote.PolicyId = @event.AggregateId;
                quote.PolicyNumber = @event.PolicyNumber;
                quote.LatestFormData = @event.DataSnapshot.FormData.Data.Json;
                quote.LatestCalculationResultId = @event.DataSnapshot.CalculationResult.Id;
                quote.LatestCalculationResult = @event.CalculationResult;
                quote.LatestCalculationResultFormDataId = @event.CalculationResult.FormDataId.Value;
                aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyNumberUpdatedEvent @event, int sequenceNumber)
        {
            if (!@event.QuoteId.HasValue)
            {
                return;
            }
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId.Value);
            if (quote != null)
            {
                quote.PolicyNumber = @event.PolicyNumber;
                quote.LastModifiedTimestamp = @event.Timestamp;
                quote.LastModifiedByUserTimestamp = @event.Timestamp;
                aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyAdjustedEvent @event, int sequenceNumber)
        {
            if (!@event.QuoteId.HasValue)
            {
                return;
            }
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId.Value);
            if (quote != null)
            {
                quote.LatestFormData = @event.DataSnapshot.FormData.Data.Json;
                quote.LatestCalculationResultId = @event.DataSnapshot.CalculationResult.Id;
                quote.LatestCalculationResult = @event.DataSnapshot.CalculationResult.Data;
                quote.LatestCalculationResultFormDataId = @event.DataSnapshot.CalculationResult.Data.FormDataId.Value;
                aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyCancelledEvent @event, int sequenceNumber)
        {
            if (!@event.QuoteId.HasValue)
            {
                return;
            }
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId.Value);
            if (quote != null)
            {
                quote.LatestFormData = @event.DataSnapshot.FormData.Data.Json;
                quote.LatestCalculationResultId = @event.DataSnapshot.CalculationResult.Id;
                quote.LatestCalculationResult = @event.DataSnapshot.CalculationResult.Data;
                quote.LatestCalculationResultFormDataId = @event.DataSnapshot.CalculationResult.Data.FormDataId.Value;
                aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyRenewedEvent @event, int sequenceNumber)
        {
            if (!@event.QuoteId.HasValue)
            {
                return;
            }
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId.Value);
            if (quote != null)
            {
                quote.LatestFormData = @event.DataSnapshot.FormData.Data.Json;
                quote.LatestCalculationResultId = @event.DataSnapshot.CalculationResult.Id;
                quote.LatestCalculationResult = @event.DataSnapshot.CalculationResult.Data;
                quote.LatestCalculationResultFormDataId = @event.DataSnapshot.CalculationResult.Data.FormDataId.Value;
                aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.FullNameUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var quote in this.GetQuotesForPerson(@event.TenantId, @event.AggregateId))
            {
                quote.CustomerFullName = @event.Value;
            }

            foreach (var quote in this.GetQuotesByOwnerPersonId(@event.TenantId, @event.AggregateId))
            {
                quote.OwnerFullName = @event.Value;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PreferredNameUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var quote in this.GetQuotesForPerson(@event.TenantId, @event.AggregateId))
            {
                quote.CustomerPreferredName = @event.Value;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.EmailAddressUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var quote in this.GetQuotesForPerson(@event.TenantId, @event.AggregateId))
            {
                quote.CustomerEmail = @event.Value;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.AlternativeEmailAddressUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var quote in this.GetQuotesForPerson(@event.TenantId, @event.AggregateId))
            {
                quote.CustomerAlternativeEmail = @event.Value;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.MobilePhoneUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var quote in this.GetQuotesForPerson(@event.TenantId, @event.AggregateId))
            {
                quote.CustomerMobilePhone = @event.Value;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.HomePhoneUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var quote in this.GetQuotesForPerson(@event.TenantId, @event.AggregateId))
            {
                quote.CustomerHomePhone = @event.Value;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.WorkPhoneUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var quote in this.GetQuotesForPerson(@event.TenantId, @event.AggregateId))
            {
                quote.CustomerWorkPhone = @event.Value;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var quote in this.GetQuotesForPerson(@event.TenantId, @event.AggregateId))
            {
                quote.CustomerFullName = @event.PersonData.FullName;
                quote.CustomerPreferredName = @event.PersonData.PreferredName;
                quote.CustomerEmail = @event.PersonData.Email;
                quote.CustomerAlternativeEmail = @event.PersonData.AlternativeEmail;
                quote.CustomerMobilePhone = @event.PersonData.MobilePhone;
                quote.CustomerHomePhone = @event.PersonData.HomePhone;
            }

            foreach (var quote in this.GetQuotesByOwnerPersonId(@event.TenantId, @event.AggregateId))
            {
                quote.OwnerFullName = @event.PersonData.FullName;
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.RenewalQuoteCreatedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read model
                this.quoteReadModelRepository.DeleteById(@event.TenantId, @event.QuoteId);
            }

            var quote = new NewQuoteReadModel(aggregate, @event);
            if (@event.AdditionalProperties != null)
            {
                foreach (var additionalProperty in @event.AdditionalProperties)
                {
                    this.propertyTypeEvaluatorService.CreateNewAdditionalPropertyValueByPropertyType(
                       (TGuid<Tenant>)@event.TenantId,
                       quote.Id,
                       additionalProperty.Type,
                       (TGuid<AdditionalPropertyDefinition>)additionalProperty.DefinitionId,
                       (TGuid<AdditionalPropertyValue>)Guid.NewGuid(), // the additional property value id is generated here
                       additionalProperty.Value);
                }
            }

            this.UpdateQuoteWithPolicyDetails(@event.TenantId, quote, @event.AggregateId);
            this.quoteReadModelRepository.Add(quote);
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteRollbackEvent @event, int sequenceNumber)
        {
            this.DeleteQuotes(@event);
            aggregate.IsBeingReplayed = true;
            sequenceNumber = 0;
            foreach (IEvent<QuoteAggregate, Guid> replayEvent in @event.ReplayEvents)
            {
                if (replayEvent is QuoteAggregate.QuoteRollbackEvent)
                {
                    throw new InvalidOperationException("ReplayEvents should not contain a QuoteRollbackEvent.");
                }
                else
                {
                    this.DispatchIfHandlerExists(aggregate, replayEvent, sequenceNumber);
                }

                sequenceNumber++;
            }

            // update the last modified timestamp
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            quote.LastModifiedTimestamp = @event.Timestamp;
            var aggregateQuote = aggregate.GetQuoteOrThrow(@event.QuoteId);
            if (aggregateQuote.IsExpired(this.clock.Now()))
            {
                quote.QuoteState = StandardQuoteStates.Expired;
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.WorkflowStepAssignedEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            if (quote != null)
            {
                quote.WorkflowStep = @event.WorkflowStep;
                quote.LastModifiedTimestamp = @event.Timestamp;
                quote.LastModifiedByUserTimestamp = @event.Timestamp;
                aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
            }

            // this is used for backward compatibility.
            else
            {
                var quotes = this.GetByAggregateId(@event.TenantId, @event.AggregateId);
                foreach (var tmpQuote in quotes)
                {
                    tmpQuote.WorkflowStep = @event.WorkflowStep;
                    tmpQuote.LastModifiedTimestamp = @event.Timestamp;
                    tmpQuote.LastModifiedByUserTimestamp = @event.Timestamp;
                }
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteStateChangedEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            quote.QuoteState = @event.ResultingState;
            quote.LastModifiedTimestamp = @event.Timestamp;
            quote.LastModifiedByUserTimestamp = @event.Timestamp;
            var aggregateQuote = aggregate.GetQuoteOrThrow(@event.QuoteId);
            if (aggregateQuote.IsExpired(this.clock.Now()))
            {
                quote.QuoteState = StandardQuoteStates.Expired;
            }

            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteActualisedEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            quote.IsActualised = true;
            aggregate.GetQuoteOrThrow(quote.Id).SetReadModel(quote);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            var quotes = this.GetByAggregateId(@event.TenantId, @event.AggregateId);

            foreach (var quote in quotes)
            {
                quote.TenantId = @event.TenantId;
                quote.ProductId = @event.ProductId;
                quote.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            var quotes = this.GetQuotesByAggregateId(@event.TenantId, @event.AggregateId);
            foreach (var quote in quotes)
            {
                quote.OrganisationId = @event.OrganisationId;
                quote.LastModifiedTimestamp = @event.Timestamp;
                if (@event.PerformingUserId.HasValue)
                {
                    quote.LastModifiedByUserTimestamp = @event.Timestamp;
                }
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.MigrateQuotesAndPolicyTransactionsToNewProductReleaseEvent @event, int sequenceNumber)
        {
            var quotes = this.GetQuotesByProductReleaseId(@event.TenantId, @event.AggregateId, @event.OrginalProductReleaseId);

            foreach (var quote in quotes)
            {
                quote.ProductReleaseId = @event.NewProductReleaseId;
                quote.LastModifiedTimestamp = @event.Timestamp;
                if (@event.PerformingUserId.HasValue)
                {
                    quote.LastModifiedByUserTimestamp = @event.Timestamp;
                }
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.MigrateUnassociatedEntitiesToProductReleaseEvent @event, int sequenceNumber)
        {
            var quotes = this.GetQuotesWithoutAssociation(@event.TenantId, @event.AggregateId);

            foreach (var quote in quotes)
            {
                quote.ProductReleaseId = @event.NewProductReleaseId;
                quote.LastModifiedTimestamp = @event.Timestamp;
                if (@event.PerformingUserId.HasValue)
                {
                    quote.LastModifiedByUserTimestamp = @event.Timestamp;
                }
            }
        }

        public void Handle(
            QuoteAggregate aggregate,
            AdditionalPropertyValueInitializedEvent<QuoteAggregate, IQuoteEventObserver> @event,
            int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                this.propertyTypeEvaluatorService.DeleteAdditionalPropertyValue(
                    (TGuid<Tenant>)@event.TenantId,
                    @event.AdditionalPropertyDefinitionType,
                    (TGuid<AdditionalPropertyValue>)@event.AdditionalPropertyValueId);
            }

            this.propertyTypeEvaluatorService.CreateNewAdditionalPropertyValueByPropertyType(
                (TGuid<Tenant>)@event.TenantId,
                @event.EntityId,
                @event.AdditionalPropertyDefinitionType,
                (TGuid<AdditionalPropertyDefinition>)@event.AdditionalPropertyDefinitionId,
                (TGuid<AdditionalPropertyValue>)@event.AdditionalPropertyValueId,
                @event.Value);
        }

        public void Handle(
            QuoteAggregate aggregate,
            AdditionalPropertyValueUpdatedEvent<QuoteAggregate, IQuoteEventObserver> @event,
            int sequenceNumber)
        {
            this.propertyTypeEvaluatorService.UpdateAdditionalPropertyValue(
                (TGuid<Tenant>)@event.TenantId,
                @event.EntityId,
                @event.AdditionalPropertyDefinitionType,
                (TGuid<AdditionalPropertyDefinition>)@event.AdditionalPropertyDefinitionId,
                (TGuid<AdditionalPropertyValue>)@event.AdditionalPropertyValueId,
                @event.Value);

            var quote = this.GetQuoteById(@event.TenantId, @event.EntityId);
            if (quote != null)
            {
                quote.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        private void DeleteQuotes(QuoteAggregate.QuoteRollbackEvent @event)
        {
            foreach (IEvent<QuoteAggregate, Guid> replayEvent in @event.ReplayEvents)
            {
                if (replayEvent is QuoteInitializedEvent quoteInitialized)
                {
                    this.quoteReadModelRepository.DeleteById(quoteInitialized.TenantId, quoteInitialized.QuoteId);
                }
            }

            foreach (IEvent<QuoteAggregate, Guid> strippedEvent in @event.StrippedEvents)
            {
                if (strippedEvent is IQuoteCreatedEvent quoteCreatedEvent)
                {
                    this.quoteReadModelRepository.DeleteById(@event.TenantId, quoteCreatedEvent.QuoteId);
                }
            }
        }

        private NewQuoteReadModel GetQuoteById(Guid tenantId, Guid quoteId)
        {
            return this.quoteReadModelRepository.GetById(tenantId, quoteId);
        }

        /// <summary>
        /// Gets all the quote read models for an aggregate.
        /// Warning: Do not use this if you only need to update specific quotes. A Quote read model contains a lot of data
        /// and fetching them unnecessarily from the database is costly.
        /// </summary>
        private IEnumerable<NewQuoteReadModel> GetQuotesByAggregateId(Guid tenantId, Guid aggregateId)
        {
            return this.quoteReadModelRepository.Where(tenantId, q => q.AggregateId == aggregateId);
        }

        private IEnumerable<NewQuoteReadModel> GetByAggregateId(Guid tenantId, Guid aggregateId)
        {
            return this.quoteReadModelRepository.Where(tenantId, q => q.AggregateId == aggregateId).ToList();
        }

        private PolicyReadModel GetPolicyById(Guid tenantId, Guid aggregateId)
        {
            Maybe<PolicyReadModel> maybe = this.policyReadModelRepository.GetByIdMaybe(tenantId, aggregateId);

            return maybe.HasValue
                ? maybe.Value
                : null;
        }

        private IEnumerable<NewQuoteReadModel> GetQuotesForPerson(Guid tenantId, Guid personId)
        {
            return this.quoteReadModelRepository.Where(tenantId, q => q.CustomerPersonId == personId);
        }

        private IEnumerable<NewQuoteReadModel> GetQuotesByOwnerPersonId(Guid tenantId, Guid personId)
        {
            return this.quoteReadModelRepository.Where(tenantId, q => q.OwnerPersonId == personId);
        }

        private IEnumerable<NewQuoteReadModel> GetQuotesByProductReleaseId(Guid tenantId, Guid aggregateId, Guid productReleaseId)
        {
            return this.quoteReadModelRepository.Where(tenantId, q => q.AggregateId == aggregateId && q.ProductReleaseId == productReleaseId);
        }

        private IEnumerable<NewQuoteReadModel> GetQuotesWithoutAssociation(Guid tenantId, Guid aggregateId)
        {
            return this.quoteReadModelRepository.Where(tenantId, q => q.AggregateId == aggregateId && q.ProductReleaseId == null);
        }

        private CustomerReadModel GetCustomerById(Guid tenantId, Guid customerId)
        {
            Maybe<CustomerReadModel> maybe = this.customerReadModelRepository.GetByIdMaybe(tenantId, customerId);

            return maybe.HasValue
                ? maybe.Value
                : null;
        }

        private PersonReadModel GetPersonById(Guid tenantId, Guid personId)
        {
            Maybe<PersonReadModel> maybe = this.personReadModelRepository.GetByIdMaybe(tenantId, personId);

            return maybe.HasValue
                ? maybe.Value
                : null;
        }

        private void UpdateQuoteWithPolicyDetails(Guid tenantId, NewQuoteReadModel quote, Guid aggregateId)
        {
            var policy = this.GetPolicyById(tenantId, aggregateId);
            quote.PolicyId = policy.Id;
            quote.PolicyNumber = policy.PolicyNumber;
            quote.Environment = policy.Environment;
            quote.CustomerId = policy.CustomerId;
            quote.CustomerPersonId = policy.CustomerPersonId.GetValueOrDefault();
            quote.CustomerFullName = policy.CustomerFullName;
            quote.CustomerPreferredName = policy.CustomerPreferredName;
            quote.CustomerEmail = policy.CustomerEmail;
            quote.CustomerAlternativeEmail = policy.CustomerAlternativeEmail;
            quote.CustomerMobilePhone = policy.CustomerMobilePhone;
            quote.CustomerHomePhone = policy.CustomerHomePhone;
            quote.CustomerWorkPhone = policy.CustomerWorkPhone;
            quote.OwnerUserId = policy.OwnerUserId;
            quote.OwnerPersonId = policy.OwnerPersonId;
            quote.OwnerFullName = policy.OwnerFullName;
            quote.TenantId = policy.TenantId;
            quote.ProductId = policy.ProductId;
            quote.IsTestData = policy.IsTestData;
            quote.OrganisationId = policy.OrganisationId;
        }

        /// <summary>
        /// Usually there is a time of day scheme in place, which has the following rule:
        /// If the effective date of the new business policy or adjustment transaction is
        /// today's date, then we can make that policy transaction active immediately, which means we set
        /// the effective time of day to the current time. This method will adjust the time of day
        /// according to those rules.
        /// </summary>
        private LocalDateTime GetEffectiveTimestampForEffectiveDateUsingTimeOfDaySchemeBasedOnQuoteType(
            LocalDate effectiveDate,
            Instant now,
            DateTimeZone timeZone,
            QuoteType quoteType)
        {
            LocalDateTime effectiveDateTime = effectiveDate.At(this.timeOfDayScheme.GetEndTime());
            if (quoteType == QuoteType.NewBusiness || quoteType == QuoteType.Adjustment)
            {
                ZonedDateTime nowZonedDateTime = now.InZone(timeZone);
                var todaysDate = nowZonedDateTime.Date;
                if (effectiveDate.Equals(todaysDate))
                {
                    var nowLocalDateTime = nowZonedDateTime.LocalDateTime;
                    if ((nowLocalDateTime > effectiveDateTime && !this.timeOfDayScheme.DoesAllowInceptionTimeInThePast)
                        || (effectiveDateTime > nowLocalDateTime && this.timeOfDayScheme.DoesAllowImmediateCoverage))
                    {
                        effectiveDateTime = nowLocalDateTime;
                    }
                }
            }

            return effectiveDateTime;
        }
    }
}
