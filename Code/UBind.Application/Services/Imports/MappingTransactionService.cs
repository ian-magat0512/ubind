// <copyright file="MappingTransactionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Import
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Application.Queries.ProductRelease;
    using UBind.Application.Services.Imports;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Imports;
    using UBind.Domain.Loggers;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Services;
    using UBind.Domain.Services.QuoteExpiry;
    using UBind.Domain.ValueTypes;
    using static UBind.Domain.Errors;

    /// <summary>
    /// The mapping transaction service that handles anything related to mappers including the command.
    /// </summary>
    public class MappingTransactionService : IMappingTransactionService
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly ICustomerAggregateRepository customerRepository;

        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly IPolicyTransactionTimeOfDayScheme timeOfDayScheme;

        private readonly IClaimAggregateRepository claimRepository;
        private readonly IClaimReadModelRepository claimReadModelRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IClaimReferenceNumberGenerator claimReferenceNumberGenerator;
        private readonly IQuoteExpirySettingsProvider quoteExpirySettingsProvider;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly ICqrsMediator mediator;
        private readonly IClock clock;

        public MappingTransactionService(
            IPersonReadModelRepository personReadModelRepository,
            IPersonAggregateRepository personAggregateRepository,
            ICustomerAggregateRepository customerRepository,
            IPolicyReadModelRepository policyReadModelRepository,
            IQuoteAggregateRepository quoteRepository,
            IQuoteReadModelRepository quoteReadModelRepository,
            IPolicyTransactionTimeOfDayScheme limitTimesPolicy,
            IClaimAggregateRepository claimRepository,
            IClaimReadModelRepository claimReadModelRepository,
            IUserReadModelRepository userReadModelRepository,
            IClaimReferenceNumberGenerator referenceNumberGenerator,
            IQuoteExpirySettingsProvider quoteExpirySettingsProvider,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ICachingResolver cachingResolver,
            IClock clock,
            ICqrsMediator mediator)
        {
            this.cachingResolver = cachingResolver;
            this.personReadModelRepository = personReadModelRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.customerRepository = customerRepository;

            this.policyReadModelRepository = policyReadModelRepository;
            this.quoteAggregateRepository = quoteRepository;
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.timeOfDayScheme = limitTimesPolicy;

            this.claimRepository = claimRepository;
            this.claimReadModelRepository = claimReadModelRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.claimReferenceNumberGenerator = referenceNumberGenerator;
            this.quoteExpirySettingsProvider = quoteExpirySettingsProvider;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.mediator = mediator;
        }

        /// <inheritdoc/>
        public async Task<CustomerAggregate> HandleCustomers(
            IProgressLogger logger, ImportBaseParam baseParam, CustomerImportData data, bool updateEnabled = false)
        {
            var timestamp = this.clock.GetCurrentInstant();

            async Task<CustomerAggregate> CreateCustomer()
            {
                logger.Log(LogLevel.Information, $"Creating a new customer user account for {data.Email}.");
                var personAggregate
                    = await this.CreatePerson(baseParam.TenantId, baseParam.OrganisationId, data, timestamp);
                return await this.CreateCustomer(personAggregate, baseParam.Environment, timestamp);
            }

            CustomerAggregate customer;
            var person = !string.IsNullOrEmpty(data.Email)
                ? this.personReadModelRepository.GetPersonAssociatedWithCustomerByEmail(
                    baseParam.TenantId,
                    baseParam.OrganisationId,
                    data.Email)
                : null;

            // no match.
            if (person == null || !person.CustomerId.HasValue)
            {
                customer = await CreateCustomer();
            }
            else
            {
                customer = this.customerRepository.GetById(person.TenantId, person.CustomerId.Value);
                if (updateEnabled)
                {
                    logger.Log(LogLevel.Information, $"Starting to update user with the username of {data.Email}.");

                    var personAggregate = this.personAggregateRepository.GetById(baseParam.TenantId, person.Id);
                    personAggregate.UpdateWithImportedData(
                        data, this.httpContextPropertiesResolver.PerformingUserId, timestamp);
                    await this.personAggregateRepository.Save(personAggregate);
                }
                else
                {
                    logger.Log(LogLevel.Information, $"Customer with email {data.Email} already exists. System will use this record and note that the customer update feature is disabled at this moment.");
                }
            }

            return customer;
        }

        /// <inheritdoc/>
        public async Task HandlePolicies(
            IProgressLogger logger, ImportBaseParam baseParam, PolicyImportData data, bool updateEnabled)
        {
            if (baseParam.ProductId == default)
            {
                var message = $"Importing a policy requires Product Id.";
                logger.Log(LogLevel.Error, message);
                throw new NotSupportedException(message);
            }

            var policyExists = this.policyReadModelRepository.GetPolicyByNumber(
                    baseParam.TenantId, baseParam.ProductId, baseParam.Environment, data.PolicyNumber) != null;
            if (policyExists)
            {
                var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(baseParam.TenantId);
                var message = $"Policy with policy number {data.PolicyNumber} already exists. Policy update feature is not yet supported, thus aborting import of this policy.";
                message = TenantHelper.CheckAndChangeTextToMutual(message, tenantAlias);
                logger.Log(LogLevel.Error, message);
                throw new NotSupportedException(message);
            }

            var agentEmail = data.AgentEmail;
            UserReadModel? owner = null;
            if (!string.IsNullOrWhiteSpace(agentEmail))
            {
                owner = this.userReadModelRepository.GetUsersMatchingEmailAddressIncludingPlusAddressing(baseParam.TenantId, agentEmail)
                    .FirstOrDefault(u => u.UserType == UserType.Client.ToString());
                if (owner == null)
                {
                    throw new ErrorException(Errors.User.AgentOrEmailNotFound(agentEmail));
                }
            }

            var timestamp = this.clock.GetCurrentInstant();
            var customerData = new CustomerImportData(baseParam.TenantId, baseParam.OrganisationId, data.CustomerEmail, data.CustomerName);
            var customer = await this.HandleCustomers(logger, baseParam, customerData, updateEnabled);
            var person = this.personAggregateRepository.GetById(customer.TenantId, customer.PrimaryPersonId);
            var personDetails = new PersonalDetails(person);

            logger.Log(LogLevel.Information, $"Creating a new policy from import under policy number {data.PolicyNumber}.");

            Guid? productReleaseId = await this.mediator.Send(new GetProductReleaseIdQuery(
                    baseParam.TenantId,
                    baseParam.ProductId,
                    baseParam.Environment,
                    QuoteType.NewBusiness));

            DateTimeZone timeZone = string.IsNullOrEmpty(data.TimeZoneId)
                ? Timezones.AET
                : Timezones.GetTimeZoneByIdOrThrow(data.TimeZoneId);
            var quoteAggregate = QuoteAggregate.CreateImportedPolicy(
                baseParam.TenantId,
                baseParam.OrganisationId,
                baseParam.ProductId,
                baseParam.Environment,
                customer.PrimaryPersonId,
                customer.Id,
                personDetails,
                data,
                timeZone,
                this.timeOfDayScheme,
                this.httpContextPropertiesResolver.PerformingUserId,
                timestamp,
                productReleaseId,
                owner);
            await this.quoteAggregateRepository.Save(quoteAggregate);
        }

        /// <inheritdoc/>
        public async Task HandleClaims(
            IProgressLogger logger, ImportBaseParam baseParam, ClaimImportData data, bool updateEnabled)
        {
            if (baseParam.ProductId == default)
            {
                var message = $"Create/Update Claims requires Product Id.";
                var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(baseParam.TenantId, baseParam.ProductId);
                message = TenantHelper.CheckAndChangeTextToMutual(message, productAlias);
                logger.Log(LogLevel.Error, message);
                throw new NotSupportedException(message);
            }

            var timestamp = data.NotifiedDate.ToLocalDateFromMdyy().AtStartOfDayInZone(Timezones.AET).ToInstant();
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(baseParam.TenantId);
            var isMutual = TenantHelper.IsMutual(tenantAlias);
            var claimReadModel = this.claimReadModelRepository.GetByClaimNumber(
                baseParam.TenantId, baseParam.ProductId, baseParam.Environment, data.ClaimNumber);
            if (claimReadModel != null)
            {
                var message = $"Updating claim with a reference number of {data.ClaimNumber} and policy number of {data.PolicyNumber}.";
                message = TenantHelper.CheckAndChangeTextToMutual(message, isMutual);
                logger.Log(LogLevel.Information, message);

                var claim = this.claimRepository.GetById(claimReadModel.TenantId, claimReadModel.Id);
                claim.UpdateClaimFromImport(data, this.httpContextPropertiesResolver.PerformingUserId, timestamp);
                await this.claimRepository.Save(claim);
            }
            else
            {
                var policyReadModel = this.policyReadModelRepository.GetPolicyByNumber(
                    baseParam.TenantId, baseParam.ProductId, baseParam.Environment, data.PolicyNumber);
                if (policyReadModel != null)
                {
                    var message = $"Creating a new claim with a reference number of {data.ClaimNumber} and policy number of {data.PolicyNumber}.";
                    message = TenantHelper.CheckAndChangeTextToMutual(message, isMutual);
                    logger.Log(LogLevel.Information, message);

                    PersonAggregate person = null;
                    if (policyReadModel.CustomerPersonId.HasValue)
                    {
                        person = this.personAggregateRepository.GetById(policyReadModel.TenantId, policyReadModel.CustomerPersonId.Value);
                    }

                    this.claimReferenceNumberGenerator.SetProperties(
                        baseParam.TenantId,
                        baseParam.ProductId,
                        baseParam.Environment);
                    var claimReference = this.claimReferenceNumberGenerator.Generate();

                    var newClaim = ClaimAggregate.CreateImportedClaim(
                        claimReference,
                        policyReadModel,
                        person,
                        data,
                        this.httpContextPropertiesResolver.PerformingUserId,
                        timestamp);
                    await this.claimRepository.Save(newClaim);
                }
                else
                {
                    var message = $"The policy with the policy number {data.PolicyNumber} doesn't exist. Fix me.";
                    message = TenantHelper.CheckAndChangeTextToMutual(message, isMutual);
                    logger.Log(LogLevel.Error, message);

                    throw new NotSupportedException(message);
                }
            }
        }

        public async Task HandleQuotes(IProgressLogger logger, ImportBaseParam baseParam, QuoteImportData data, bool updateEnabled = false)
        {
            var timestamp = this.clock.GetCurrentInstant();
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(baseParam.TenantId);
            if (baseParam.ProductId == default)
            {
                var message = "Importing a quote requires the product Id";
                logger.Log(LogLevel.Error, message);
                throw new ErrorException(General.ModelValidationFailed(message));
            }

            if (data.QuoteState.IsNotNullOrEmpty() && data.QuoteState.EqualsIgnoreCase(StandardQuoteStates.Nascent))
            {
                var error = Domain.Errors.Quote.CannotImportNascentQuote(data.QuoteNumber);
                logger.Log(LogLevel.Error, error.Message);
                throw new ErrorException(error);
            }

            if (this.quoteReadModelRepository.GetQuoteIdByReferenceNumber(baseParam.TenantId, baseParam.Environment, data.QuoteNumber).HasValue)
            {
                var message = $"Quote with quote number {data.QuoteNumber} already exists. Quote update feature is not yet supported, thus aborting import of this quote.";
                message = TenantHelper.CheckAndChangeTextToMutual(message, tenantAlias);
                logger.Log(LogLevel.Error, message);
                throw new NotSupportedException(message);
            }

            var customerData = new CustomerImportData(baseParam.TenantId, baseParam.OrganisationId, data.CustomerEmail, data.CustomerName);
            var customer = await this.HandleCustomers(logger, baseParam, customerData, updateEnabled);
            var details = new PersonalDetails(this.personAggregateRepository.GetById(customer.TenantId, customer.PrimaryPersonId));

            logger.Log(LogLevel.Information, $"Creating a new quote from import under quote number {data.QuoteNumber}.");

            Guid? productReleaseId = await this.mediator.Send(new GetProductReleaseIdQuery(
                    baseParam.TenantId,
                    baseParam.ProductId,
                    baseParam.Environment,
                    QuoteType.NewBusiness));
            DateTimeZone timeZone = string.IsNullOrEmpty(data.TimeZoneId)
                ? Timezones.AET
                : Timezones.GetTimeZoneByIdOrThrow(data.TimeZoneId);
            var quoteAggregate = QuoteAggregate.CreateImportedQuote(
                baseParam.TenantId,
                baseParam.OrganisationId,
                baseParam.ProductId,
                baseParam.Environment,
                customer.Id,
                details,
                data,
                timeZone,
                await this.quoteExpirySettingsProvider.Retrieve(baseParam.TenantId, baseParam.ProductId),
                this.httpContextPropertiesResolver.PerformingUserId,
                timestamp,
                productReleaseId);
            await this.quoteAggregateRepository.Save(quoteAggregate);
        }

        private async Task<PersonAggregate> CreatePerson(
            Guid tenantId, Guid organisationId, CustomerImportData data, Instant timestamp)
        {
            var personAggregate = PersonAggregate.CreateImportedPerson(tenantId, organisationId, data, this.httpContextPropertiesResolver.PerformingUserId, timestamp);
            await this.personAggregateRepository.Save(personAggregate);
            return personAggregate;
        }

        private async Task<CustomerAggregate> CreateCustomer(
            PersonAggregate person, DeploymentEnvironment environment, Instant timestamp)
        {
            var customerAggregate = CustomerAggregate.CreateImportedCustomer(
                person.TenantId, person, environment, this.httpContextPropertiesResolver.PerformingUserId, null, timestamp);
            person.AssociateWithCustomer(customerAggregate.Id, this.httpContextPropertiesResolver.PerformingUserId, timestamp);
            await this.customerRepository.Save(customerAggregate);
            await this.personAggregateRepository.Save(person);
            return customerAggregate;
        }
    }
}
