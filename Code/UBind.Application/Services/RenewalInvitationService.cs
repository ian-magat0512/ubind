// <copyright file="RenewalInvitationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Humanizer;
    using NodaTime;
    using UBind.Application.Queries.Customer;
    using UBind.Application.Queries.Organisation;
    using UBind.Application.Queries.Portal;
    using UBind.Application.Queries.User;
    using UBind.Application.Services.SystemEmail;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Models;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using IUserService = UBind.Application.User.IUserService;

    /// <inheritdoc/>
    public class RenewalInvitationService : IRenewalInvitationService
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IClock clock;
        private readonly IPolicyService policyService;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly ICqrsMediator mediator;
        private readonly ISystemEmailService systemEmailService;
        private readonly ICachingResolver cachingResolver;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IProductFeatureSettingService productFeatureService;
        private readonly IUserService userService;
        private readonly IUserLoginEmailRepository userLoginEmailRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenewalInvitationService"/> class.
        /// </summary>
        /// <param name="emailConfiguration">The email invitation configuration.</param>
        /// <param name="personRepository">The person repository.</param>
        /// <param name="tenantRepository">The tenant repository.</param>
        /// <param name="userAggregateRepository">The user Aggregate repository.</param>
        /// <param name="policyService">The policy service.</param>
        /// <param name="systemEmailService">The system email service.</param>
        /// <param name="customerAggregateRepository">The Customer Aggregate repository.</param>
        /// <param name="quoteAggregateRepository">The quoteAggregate repository.</param>
        /// <param name="httpContextPropertiesResolver">The performing user resolver.</param>
        /// <param name="organisationReadModelRepository">The organisation read model repository.</param>
        /// <param name="userLoginEmailRepository">The user login email repository.</param>
        /// <param name="productFeatureService">The product feature service.</param>
        /// <param name="contextAccessor">The http context accessor.</param>
        /// <param name="clock">Clock for obtaining current time.</param>
        public RenewalInvitationService(
            IPersonAggregateRepository personRepository,
            ITenantRepository tenantRepository,
            IUserAggregateRepository userAggregateRepository,
            IPolicyService policyService,
            ISystemEmailService systemEmailService,
            ICustomerAggregateRepository customerAggregateRepository,
            IQuoteAggregateRepository quoteAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IOrganisationReadModelRepository organisationReadModelRepository,
            IUserLoginEmailRepository userLoginEmailRepository,
            IProductFeatureSettingService productFeatureService,
            IUserService userService,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver,
            IClock clock)
        {
            this.cachingResolver = cachingResolver;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.personAggregateRepository = personRepository;
            this.tenantRepository = tenantRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.policyService = policyService;
            this.systemEmailService = systemEmailService;
            this.customerAggregateRepository = customerAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.productFeatureService = productFeatureService;
            this.userService = userService;
            this.userLoginEmailRepository = userLoginEmailRepository;
            this.clock = clock;
            this.mediator = mediator;
        }

        /// <inheritdoc/>
        public async Task SendPolicyRenewalInvitation(
            Guid tenantId,
            DeploymentEnvironment environment,
            Guid policyId,
            Guid userId,
            bool isMutual,
            bool isUserAccountRequired,
            string parentUrl = "")
        {
            IPolicyReadModelDetails policyDetails = await this.policyService.GetPolicy(tenantId, policyId);
            var renewalTransaction = ProductFeatureSettingItem.RenewalPolicyTransactions;
            await this.productFeatureService.ThrowIfFeatureIsNotEnabled(
                policyDetails.TenantId, policyDetails.ProductId, renewalTransaction, "renewal");
            if (policyDetails.TenantId != tenantId)
            {
                throw new ErrorException(Errors.Policy.Renewal.UserNotFound(userId, isMutual));
            }

            if (policyDetails == null)
            {
                throw new ErrorException(Errors.Policy.Renewal.NoPolicyExists(isMutual));
            }

            await this.ThrowIfPolicyNotAllowedForSendingRenewal(policyDetails, isMutual);

            var tenant = this.tenantRepository.GetTenantById(policyDetails.TenantId);

            if (policyDetails.CustomerId.IsNullOrEmpty())
            {
                // we currently don't support sending a renewal to a customer's email address if no
                // customer object has been created. However we may relax this requirement in the future
                // and get the email address from the quote data using a dataLocator.
                throw new ErrorException(Errors.Policy.Renewal
                    .CannotSendRenewalInvitationWithoutCustomer(policyDetails.PolicyNumber));
            }

            var entitySetting = await this.mediator.Send(
                new GetOrganisationEntitySettingsQuery(tenantId, policyDetails.OrganisationId))
                ?? new OrganisationEntitySettings();

            if (!entitySetting.AllowOrganisationRenewalInvitation)
            {
                throw new ErrorException(
                    Errors.Organisation.RenewalInvitationEmailsDisabled(tenant.Details.Alias, policyDetails.OrganisationName));
            }

            var customerAggregate = this.customerAggregateRepository.GetById(policyDetails.TenantId, policyDetails.CustomerId.Value);
            var personAggregate = this.personAggregateRepository.GetById(customerAggregate.TenantId, customerAggregate.PrimaryPersonId);

            UserAggregate userAggregate = null;
            if (personAggregate.UserId.HasValue)
            {
                // We need to try to get the user aggregate, because if it exists we still want to pass it through the
                // liquid template
                userAggregate = this.userAggregateRepository.GetById(personAggregate.TenantId, personAggregate.UserId.Value);
            }

            if (userAggregate == null)
            {
                var customerHasUserAccountQuery = new CustomerHasUserAccountQuery(customerAggregate.TenantId, customerAggregate.Id);
                var hasOtherUserAccount = await this.mediator.Send(customerHasUserAccountQuery);
                if (hasOtherUserAccount)
                {
                    var nextAvailableUserForCustomer = new GetNextAvailableUserForCustomerQuery(customerAggregate.TenantId, customerAggregate.Id);
                    var customerUserReadModel = await this.mediator.Send(nextAvailableUserForCustomer);

                    userAggregate = this.userAggregateRepository.GetById(customerUserReadModel.TenantId, customerUserReadModel.Id);
                }
            }

            // If the user aggregate is still null, ensure that it will contain a customer user
            if (userAggregate == null && isUserAccountRequired)
            {
                userAggregate = await this.CreateUserAccountForPerson(personAggregate);
            }

            var quoteAggregate = this.quoteAggregateRepository.GetById(tenantId, policyId);
            var latestQuote = quoteAggregate.GetLatestQuote();
            Guid? renewalQuoteId = null;
            if (latestQuote is RenewalQuote)
            {
                renewalQuoteId = latestQuote.Id;
            }

            EmailDrop renewalEmailDrop = await this.CreateRenewalEmailDrop(
                userAggregate,
                personAggregate,
                policyDetails,
                tenant,
                environment,
                customerAggregate.PortalId,
                renewalQuoteId,
                !isUserAccountRequired);

            var policyTransaction = quoteAggregate.Policy.Transactions.LastOrDefault();
            if (policyTransaction == null)
            {
                var message = "Cannot attach policy renewal email when there is no policy upsert transaction.";
                message = TenantHelper.CheckAndChangeTextToMutual(message, isMutual);
                throw new InvalidOperationException(message);
            }

            var quote = quoteAggregate.GetQuoteOrThrow(policyTransaction.QuoteId.Value);
            this.systemEmailService.SendAndPersistPolicyRenewalEmail(
                renewalEmailDrop, quoteAggregate, customerAggregate.PrimaryPersonId, policyTransaction, quote);
        }

        private async Task<UserAggregate> CreateUserAccountForPerson(PersonAggregate personAggregate)
        {
            var userLogin = this.userLoginEmailRepository.GetUserLoginByEmail(
                personAggregate.TenantId, personAggregate.OrganisationId, personAggregate.Email);
            if (userLogin != null)
            {
                throw new DuplicateUserEmailException(Domain.Errors.Customer.EmailAddressInUseByAnotherUser(personAggregate.Email));
            }

            // Create a user record from the customer
            var userAggregate = await this.userService.CreateUserForPerson(personAggregate);
            return userAggregate;
        }

        private async Task ThrowIfPolicyNotAllowedForSendingRenewal(IPolicyReadModelDetails policyDetails, bool isMutual)
        {
            var productFeature = this.productFeatureService.GetProductFeature(policyDetails.TenantId, policyDetails.ProductId);
            var numberOfDaysToExpire = policyDetails.GetDaysToExpire(this.clock.Today());
            var allowableDaysToRenewAfterExpiry = LocalDateExtensions.SecondsToDays(productFeature.ExpiredPolicyRenewalDurationInSeconds);
            var policyStatus = policyDetails.GetPolicyStatus(this.clock.Now());
            if (!this.policyService.IsRenewalAllowedAtTheCurrentTime(policyDetails, isMutual))
            {
                throw new ErrorException(Errors.Policy.Renewal.SendingExpiredPolicyRenewalInviteNotAllowed(
                    policyDetails.PolicyNumber,
                    isMutual,
                    productFeature.IsRenewalAllowedAfterExpiry,
                    numberOfDaysToExpire,
                    allowableDaysToRenewAfterExpiry));
            }

            var isActiveOrExpired = policyStatus == PolicyStatus.Active || policyStatus == PolicyStatus.Expired;
            var policyExpiryIsWithIn60Days = isActiveOrExpired && numberOfDaysToExpire <= 60;

            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(policyDetails.TenantId);
            if (tenantAlias != "demo" && !policyExpiryIsWithIn60Days)
            {
                throw new ErrorException(Errors.Policy.Renewal.SendingRenewalInviteNotWithIn60DaysAfterExpiryNotAllowed(policyDetails.PolicyNumber, isMutual));
            }
        }

        private async Task<EmailDrop> CreateRenewalEmailDrop(
            UserAggregate userAggregate,
            PersonAggregate personAggregate,
            IPolicyReadModelDetails policyDetails,
            Tenant tenant,
            DeploymentEnvironment environment,
            Guid? portalId,
            Guid? renewalQuoteId,
            bool skipUserAccountCreation)
        {
            if (userAggregate != null)
            {
                userAggregate = this.userAggregateRepository.GetById(userAggregate.TenantId, userAggregate.Id);
            }

            var link = string.Empty;
            Guid invitationId = Guid.Empty;
            OrganisationReadModel organisation = this.organisationReadModelRepository
                .Get(personAggregate.TenantId, personAggregate.OrganisationId);
            if (userAggregate != null)
            {
                portalId = portalId
                    ?? userAggregate.PortalId
                    ?? await this.mediator.Send(new GetDefaultPortalIdQuery(
                        tenant.Id, userAggregate.OrganisationId, PortalUserType.Customer));
            }

            string path = null;
            string portalActivationPath = null;
            string portalLoginPath = null;
            if (!skipUserAccountCreation || userAggregate != null)
            {
                if (userAggregate.Activated)
                {
                    path = "/login";
                    portalLoginPath = path;
                }
                else
                {
                    async Task CreateAndSaveInvitation()
                    {
                        invitationId = userAggregate.CreateActivationInvitation(this.httpContextPropertiesResolver.PerformingUserId, this.clock.GetCurrentInstant());
                        await this.userAggregateRepository.Save(userAggregate);
                    }

                    await ConcurrencyPolicy.ExecuteWithRetriesAsync(
                        CreateAndSaveInvitation,
                        () => userAggregate = this.userAggregateRepository.GetById(userAggregate.TenantId, userAggregate.Id));

                    path = $"activate/{userAggregate.Id}?invitationId={invitationId}";
                    portalActivationPath = path;
                }
            }

            link = await this.mediator.Send(
                new GetPortalUrlQuery(tenant.Id, organisation.Id, portalId, environment, path));
            string portalActivationLink = null;
            string portalLoginLink = null;
            if (userAggregate != null && userAggregate.PortalId.HasValue)
            {
                if (portalActivationPath.IsNotNullOrEmpty())
                {
                    portalActivationLink = link;
                }

                if (portalLoginPath.IsNotNullOrEmpty())
                {
                    portalLoginLink = link;
                }
            }

            var userDrop = userAggregate != null ?
                new UserDrop(
                    personAggregate.TenantId,
                    environment.Humanize(),
                    userAggregate.Id,
                    personAggregate.Email,
                    personAggregate.AlternativeEmail,
                    personAggregate.PreferredName,
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
                    userAggregate.Blocked,
                    userAggregate.CreatedTimestamp) :
               null;
            var personDrop = new PersonDrop(
                personAggregate.Email,
                personAggregate.AlternativeEmail,
                personAggregate.PreferredName,
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
                personAggregate.HomePhone);

            var tenantDrop = new TenantDrop(tenant.Id, tenant.Details.Name, tenant.Details.Alias);
            var policyDrop = new PolicyDrop(
                policyDetails.Id, policyDetails.PolicyNumber, policyDetails.ExpiryDateTime?.ToString());
            var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(tenant.Id, policyDetails.ProductId);
            var policyRenewalDrop = new PolicyRenewalDrop(
                link,
                portalLoginLink,
                portalActivationLink,
                productAlias,
                renewalQuoteId);

            var organisationDrop = new OrganisationDrop(organisation.Id, organisation.Alias, organisation.Name);

            var rewnewalEmailDrop = EmailDrop.CreatePolicyRenewalInvation(
                    policyDetails.TenantId,
                    policyDetails.ProductId,
                    portalId,
                    tenantDrop,
                    policyDrop,
                    organisationDrop,
                    policyRenewalDrop,
                    userDrop,
                    personDrop);
            return rewnewalEmailDrop;
        }
    }
}
