// <copyright file="ApplicationQuoteService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Humanizer;
    using NodaTime;
    using UBind.Application.Queries.Portal;
    using UBind.Application.Services.SystemEmail;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.QuoteExpiry;
    using UBind.Domain.ValueTypes;

    /// <inheritdoc />
    public class ApplicationQuoteService : IApplicationQuoteService
    {
        private readonly IQuoteWorkflowProvider quoteWorkflowProvider;
        private readonly ICustomerService customerService;
        private readonly ITenantRepository tenantRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IPolicyService policyService;
        private readonly IQuoteExpirySettingsProvider quoteExpirySettingsProvider;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly IProductFeatureSettingService productFeatureSettingService;
        private readonly ISystemEmailService systemEmailService;
        private readonly ICqrsMediator mediator;
        private readonly IUserLoginEmailRepository userLoginEmailRepository;
        private readonly IQuoteSystemEventEmitter quoteSystemEventEmitter;

        public ApplicationQuoteService(
            IQuoteAggregateRepository quoteAggregateRepository,
            ITenantRepository tenantRepository,
            IUserAggregateRepository userAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            IQuoteWorkflowProvider quoteWorkflowProvider,
            ICustomerService customerService,
            IQuoteExpirySettingsProvider quoteExpirySettingsProvider,
            ICustomerAggregateRepository customerAggregateRepository,
            IUserLoginEmailRepository userLoginEmailRepository,
            IPolicyService policyService,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IOrganisationReadModelRepository organisationReadModelRepository,
            IProductFeatureSettingService productFeatureSettingService,
            ISystemEmailService systemEmailService,
            ICqrsMediator mediator,
            IQuoteSystemEventEmitter quoteSystemEventEmitter)
        {
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.clock = clock;
            this.quoteExpirySettingsProvider = quoteExpirySettingsProvider;
            this.quoteWorkflowProvider = quoteWorkflowProvider;
            this.tenantRepository = tenantRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.customerService = customerService;
            this.customerAggregateRepository = customerAggregateRepository;
            this.userLoginEmailRepository = userLoginEmailRepository;
            this.policyService = policyService;
            this.personAggregateRepository = personAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.productFeatureSettingService = productFeatureSettingService;
            this.systemEmailService = systemEmailService;
            this.mediator = mediator;
            this.quoteSystemEventEmitter = quoteSystemEventEmitter;
        }

        /// <inheritdoc/>
        public async Task<QuoteAggregate> CreateCustomerForApplication(
            Guid tenantId,
            Guid quoteId,
            Guid quoteAggregateId,
            IPersonalDetails customerDetails,
            Guid? portalId)
        {
            var quoteAggregate = this.quoteAggregateRepository.GetById(tenantId, quoteAggregateId);
            quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            CustomerAggregate? newCustomerAggregate = null;
            if (!quoteAggregate.HasCustomer)
            {
                newCustomerAggregate = await this.customerService.CreateCustomerForNewPerson(
                    quoteAggregate.TenantId,
                    quoteAggregate.Environment,
                    customerDetails,
                    quoteAggregate.OwnerUserId,
                    portalId,
                    quoteAggregate.IsTestData);
            }
            else if (quoteAggregate.CustomerId.HasValue)
            {
                if (!quoteAggregate.CustomerId.HasValue)
                {
                    throw new DomainRuleViolationException("Cannot update customer details for a quote unless a customer is already associated with the quote.");
                }

                await this.customerService.UpdateCustomerDetails(
                    tenantId,
                    quoteAggregate.CustomerId.Value,
                    customerDetails,
                    portalId);
            }

            var timestamp = this.clock.Now();
            quoteAggregate.UpdateCustomerDetails(customerDetails, this.httpContextPropertiesResolver.PerformingUserId, timestamp, quoteAggregate.Id);

            if (newCustomerAggregate != null)
            {
                quoteAggregate.RecordAssociationWithCustomer(
                    newCustomerAggregate,
                    customerDetails,
                    this.httpContextPropertiesResolver.PerformingUserId,
                    timestamp);
                quoteAggregate = await this.SendCustomerAssociationInvitationForExistingCustomerUser(
                    quoteAggregate,
                    quoteId,
                    customerDetails);
            }

            await this.quoteAggregateRepository.Save(quoteAggregate);
            return quoteAggregate;
        }

        /// <inheritdoc/>
        public async Task<QuoteAggregate> UpdateCustomerForApplication(
            Guid tenantId, Guid quoteId, Guid customerId, IPersonalDetails customerDetails, Guid? portalId)
        {
            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(tenantId, quoteId)
                ?? throw new ErrorException(Errors.General.NotFound("quote", quoteId));

            if (!quoteAggregate.HasCustomer || !quoteAggregate.CustomerId.HasValue)
            {
                throw new ErrorException(Errors.Quote.NoCustomerAssociated(quoteId, customerId));
            }

            if (quoteAggregate.CustomerId != null && quoteAggregate.CustomerId != customerId)
            {
                throw new ErrorException(Errors.Quote.MismatchedCustomerId(customerId, quoteAggregate.CustomerId.Value));
            }

            if (customerDetails != null)
            {
                var newCustomerDetails = this.GetPersonDetailsUpdateModel(customerId, customerDetails);
                await this.customerService.UpdateCustomerDetails(tenantId, customerId, newCustomerDetails, portalId);
            }

            return quoteAggregate;
        }

        /// <inheritdoc/>
        public async Task Actualise(
            ReleaseContext releaseContext,
            Quote quote,
            Domain.Aggregates.Quote.FormData? formData)
        {
            var timestamp = this.clock.Now();
            if (formData != null)
            {
                quote.UpdateFormData(formData, this.httpContextPropertiesResolver.PerformingUserId, timestamp);
            }

            if (quote.QuoteNumber == null)
            {
                var newQuoteNumber = await this.policyService.GenerateQuoteNumber(releaseContext);
                quote.AssignQuoteNumber(
                    newQuoteNumber, this.httpContextPropertiesResolver.PerformingUserId, timestamp);
            }

            if (!quote.IsActualised && quote.QuoteStatus == StandardQuoteStates.Nascent)
            {
                IQuoteWorkflow quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
                quote.Actualise(this.httpContextPropertiesResolver.PerformingUserId, timestamp, quoteWorkflow);
            }
        }

        /// <inheritdoc/>
        public async Task<QuoteAggregate> CreateVersion(
            Guid tenantId,
            Guid quoteId,
            Domain.Aggregates.Quote.FormData formData)
        {
            async Task<QuoteAggregate> CreateAndSaveVersion()
            {
                var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(tenantId, quoteId);
                var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
                var timestamp = this.clock.Now();
                quote.UpdateFormData(formData, this.httpContextPropertiesResolver.PerformingUserId, timestamp);
                quoteAggregate.CreateVersion(this.httpContextPropertiesResolver.PerformingUserId, timestamp, quote.Id);
                await this.quoteAggregateRepository.Save(quoteAggregate);
                return quoteAggregate;
            }

            return await ConcurrencyPolicy.ExecuteWithRetriesAsync(CreateAndSaveVersion);
        }

        /// <inheritdoc/>
        public async Task<QuoteAggregate> DiscardQuote(Guid tenantId, Guid quoteId, Guid userTenantId)
        {
            async Task<QuoteAggregate> DiscardAndSave()
            {
                var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(tenantId, quoteId);
                var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
                if (quoteAggregate?.TenantId != userTenantId)
                {
                    throw new UnauthorizedException($"Unauthorized cancellation of quote {quoteId}");
                }

                quoteAggregate.Discard(quoteId, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                await this.quoteAggregateRepository.Save(quoteAggregate);
                return quoteAggregate;
            }

            return await ConcurrencyPolicy.ExecuteWithRetriesAsync(DiscardAndSave);
        }

        /// <inheritdoc/>
        public async Task<QuoteAggregate> ExpireNow(Guid tenantId, Guid quoteId)
        {
            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(tenantId, quoteId);
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);

            // set expiry to now.
            var now = this.clock.Now();
            quoteAggregate.SetQuoteExpiryTime(quoteId, now, this.httpContextPropertiesResolver.PerformingUserId, now);
            await this.quoteAggregateRepository.Save(quoteAggregate);
            return quoteAggregate;
        }

        /// <inheritdoc/>
        public async Task<QuoteAggregate> SetExpiry(Guid tenantId, Guid quoteId, Instant dateTime)
        {
            if (dateTime == default)
            {
                throw new ArgumentNullException("DateTime is null.");
            }

            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(tenantId, quoteId);
            var quoteExpirySettings = await this.quoteExpirySettingsProvider.Retrieve(quoteAggregate.TenantId, quoteAggregate.ProductId);
            quoteAggregate.SetExpiryDate(quoteId, dateTime, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now(), quoteExpirySettings);
            await this.quoteAggregateRepository.Save(quoteAggregate);
            return quoteAggregate;
        }

        /// <inheritdoc/>
        public async Task TriggerEventWhenQuoteIsOpened(NewQuoteReadModel quote, bool performingUserIsCustomer)
        {
            var isCustomerAndQuoteExpired = performingUserIsCustomer
                && quote.QuoteState.EqualsIgnoreCase(StandardQuoteStates.Expired);

            if (!isCustomerAndQuoteExpired)
            {
                return;
            }

            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            await this.quoteSystemEventEmitter.CreateAndEmitSystemEvent(
                quote,
                Domain.Events.SystemEventType.CustomerExpiredQuoteOpened,
                performingUserId,
                this.clock.Now());
        }

        private async Task<QuoteAggregate> SendCustomerAssociationInvitationForExistingCustomerUser(
            QuoteAggregate quoteAggregate,
            Guid quoteId,
            IPersonalDetails customerDetails)
        {
            if (!quoteAggregate.HasCustomer)
            {
                throw new ErrorException(Errors.Quote.CustomerDetailsNotFound(quoteId));
            }

            if (customerDetails?.Email != null)
            {
                // Get the activated user aggregate of the customer email with role checking.
                var userLoginEmail = this.userLoginEmailRepository.GetUserLoginByEmail(
                    quoteAggregate.TenantId, quoteAggregate.OrganisationId, customerDetails.Email);
                if (userLoginEmail != null)
                {
                    var customerUserAggregate = this.userAggregateRepository.GetById(quoteAggregate.TenantId, userLoginEmail.Id);
                    if ((customerUserAggregate?.CanBeAssociatedWithQuotes() ?? false)
                        && quoteAggregate.CustomerId != customerUserAggregate.CustomerId)
                    {
                        // Process the creation and sending of association invitation
                        var associationResult = this.CreateCustomerAssociationInvitation(
                            customerUserAggregate, quoteAggregate, quoteId);
                        await this.SendCustomerAssociationInvitation(
                            quoteId,
                            associationResult.InvitationId,
                            quoteAggregate,
                            associationResult.UserAggregate,
                            associationResult.PersonAggregate);
                    }
                }
            }

            return quoteAggregate;
        }

        private QuoteCustomerAssociationInvitationResult CreateCustomerAssociationInvitation(
           UserAggregate customerUser, QuoteAggregate quoteAggregate, Guid quoteId)
        {
            if (customerUser.CustomerId == null)
            {
                throw new ErrorException(
                    Errors.General.Unexpected("When attempting to create a customer association invitation, the user does not have a customer ID."));
            }

            var customerReadModel = this.customerService.GetCustomerById(customerUser.TenantId, customerUser.CustomerId.Value);
            var personAggregate = this.personAggregateRepository.GetById(customerUser.TenantId, customerReadModel.PrimaryPersonId);
            personAggregate = EntityHelper.ThrowIfNotFound(personAggregate, customerReadModel.PrimaryPersonId, "person");
            var invitationId = quoteAggregate.RecordCustomerAssociationInvitationCreation(
                quoteId,
                customerUser.Id,
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.Now());

            return new QuoteCustomerAssociationInvitationResult(invitationId, customerUser, personAggregate);
        }

        private async Task SendCustomerAssociationInvitation(
            Guid quoteId,
            Guid associationInvitationId,
            QuoteAggregate quoteAggregate,
            UserAggregate customerUserAggregate,
            PersonAggregate personAggregate)
        {
            var tenant = this.tenantRepository.GetTenantById(quoteAggregate.TenantId);
            tenant = EntityHelper.ThrowIfNotFound(tenant, quoteAggregate.TenantId, "tenant");
            var customerOrganisation
                = this.organisationReadModelRepository.Get(tenant.Id, customerUserAggregate.OrganisationId);
            customerOrganisation = EntityHelper.ThrowIfNotFound(customerOrganisation, customerUserAggregate.OrganisationId, "organisation");
            string path = $"my-quotes/{quoteId}/associate/{associationInvitationId}";

            var portalId = customerUserAggregate.PortalId
                ?? await this.mediator.Send(new GetDefaultPortalIdQuery(
                    tenant.Id, customerOrganisation.Id, PortalUserType.Customer));
            string associationInvitationLink = await this.mediator.Send(new GetPortalUrlQuery(
                    tenant.Id,
                    customerOrganisation.Id,
                    portalId,
                    quoteAggregate.Environment,
                    path));

            var userDrop = new UserDrop(
                personAggregate.TenantId,
                customerUserAggregate.Environment.Humanize(),
                customerUserAggregate.Id,
                personAggregate.Email,
                personAggregate.AlternativeEmail,
                !string.IsNullOrEmpty(personAggregate.PreferredName) ? personAggregate.PreferredName : personAggregate.FullName,
                personAggregate.FullName,
                personAggregate.NamePrefix,
                personAggregate.FirstName,
                personAggregate.MiddleNames,
                personAggregate.LastName,
                personAggregate.NameSuffix,
                personAggregate.GreetingName,
                personAggregate.Company,
                personAggregate.Title,
                personAggregate.MobilePhone,
                personAggregate.WorkPhone,
                personAggregate.HomePhone,
                customerUserAggregate.Blocked,
                customerUserAggregate.CreatedTimestamp);
            var tenantDrop = new TenantDrop(tenant.Id, tenant.Details.Name, tenant.Details.Alias);
            var quoteCustomerAssociationDrop = new QuoteCustomerAssociationDrop(
                associationInvitationLink, associationInvitationId.ToString());

            var organisation = this.organisationReadModelRepository.Get(tenant.Id, personAggregate.OrganisationId);
            organisation = EntityHelper.ThrowIfNotFound(organisation, personAggregate.OrganisationId, "organisation");
            var organisationDrop = new OrganisationDrop(organisation.Id, organisation.Alias, organisation.Name);

            var emailDrop = EmailDrop.CreateQuoteAssociationInvitation(
                tenant.Id,
                quoteAggregate.ProductId,
                portalId,
                userDrop,
                tenantDrop,
                organisationDrop,
                quoteCustomerAssociationDrop);

            this.systemEmailService.SendAndPersistQuoteAssociationInvitationEmail(
                emailDrop, customerUserAggregate, quoteId);
        }

        private IPersonalDetails GetPersonDetailsUpdateModel(Guid customerId, IPersonalDetails personDetails)
        {
            var customerAggregate = this.customerAggregateRepository.GetById(personDetails.TenantId, customerId);
            customerAggregate = EntityHelper.ThrowIfNotFound(customerAggregate, customerId, "customer");
            var personAggregate = this.personAggregateRepository.GetById(personDetails.TenantId, customerAggregate.PrimaryPersonId);
            personAggregate = EntityHelper.ThrowIfNotFound(personAggregate, customerAggregate.PrimaryPersonId, "person");
            var currentPersonDetails = new PersonalDetails(personAggregate);

            if (personDetails.FirstName != null && currentPersonDetails.FirstName != personDetails.FirstName)
            {
                currentPersonDetails.FirstName = personDetails.FirstName;
            }

            if (personDetails.LastName != null && currentPersonDetails.LastName != personDetails.LastName)
            {
                currentPersonDetails.LastName = personDetails.LastName;
            }

            if (personDetails.MiddleNames != null && currentPersonDetails.MiddleNames != personDetails.MiddleNames)
            {
                currentPersonDetails.MiddleNames = personDetails.MiddleNames;
            }

            if (personDetails.FullName != null && currentPersonDetails.FullName != personDetails.FullName)
            {
                currentPersonDetails.FullName = personDetails.FullName;
            }

            if (personDetails.PreferredName != null && currentPersonDetails.PreferredName != personDetails.PreferredName)
            {
                currentPersonDetails.PreferredName = personDetails.PreferredName;
            }

            if (personDetails.NamePrefix != null && currentPersonDetails.NamePrefix != personDetails.NamePrefix)
            {
                currentPersonDetails.NamePrefix = personDetails.NamePrefix;
            }

            if (personDetails.NameSuffix != null && currentPersonDetails.NameSuffix != personDetails.NameSuffix)
            {
                currentPersonDetails.NameSuffix = personDetails.NameSuffix;
            }

            if (personDetails.Company != null && currentPersonDetails.Company != personDetails.Company)
            {
                currentPersonDetails.Company = personDetails.Company;
            }

            if (personDetails.Title != null && currentPersonDetails.Title != personDetails.Title)
            {
                currentPersonDetails.Title = personDetails.Title;
            }

            if (personDetails.Email != null && currentPersonDetails.Email != personDetails.Email)
            {
                currentPersonDetails.Email = personDetails.Email;
            }

            if (personDetails.PhoneNumbers?.Any() ?? false)
            {
                var phoneNumberList = currentPersonDetails.PhoneNumbers.ToList();

                // We assume that the application quote could only provide one of each phone types
                foreach (var phoneNumber in personDetails.PhoneNumbers)
                {
                    int index = phoneNumberList.FindIndex(phone => phone.Label == phoneNumber.Label);

                    if (index != -1)
                    {
                        phoneNumberList[index] = phoneNumber;
                    }
                    else
                    {
                        phoneNumberList.Add(phoneNumber);
                    }

                    if (phoneNumber.Label?.ToLower() == "work")
                    {
                        currentPersonDetails.WorkPhone = phoneNumber.PhoneNumberValueObject.ToString();
                    }
                    else if (phoneNumber.Label?.ToLower() == "home")
                    {
                        currentPersonDetails.HomePhone = phoneNumber.PhoneNumberValueObject.ToString();
                    }
                    else if (phoneNumber.Label?.ToLower() == "mobile")
                    {
                        currentPersonDetails.MobilePhone = phoneNumber.PhoneNumberValueObject.ToString();
                    }
                }

                currentPersonDetails.PhoneNumbers = phoneNumberList;
            }

            return currentPersonDetails;
        }
    }
}
