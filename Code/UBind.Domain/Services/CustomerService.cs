// <copyright file="CustomerService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Humanizer;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.Services.AdditionalPropertyValue;

    /// <inheritdoc/>
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly IClock clock;
        private readonly IAdditionalPropertyValueService additionalPropertyValueService;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        /// <param name="customerAggregateRepository">The customer aggregate repository.</param>
        /// <param name="customerReadModelRepository">The customer read model repository.</param>
        /// <param name="userAggregateRepository">The user repository.</param>
        /// <param name="personAggregateRepository">The person repository.</param>
        /// <param name="httpContextPropertiesResolver">The performing user resolver.</param>
        /// <param name="personReadModelRepository">The person read model repository.</param>
        /// <param name="clock">A clock for obtaining time.</param>
        /// <param name="additionalPropertyValueService">Additional property value service.</param>
        public CustomerService(
            ICustomerAggregateRepository customerAggregateRepository,
            ICustomerReadModelRepository customerReadModelRepository,
            IUserAggregateRepository userAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IPersonReadModelRepository personReadModelRepository,
            IClock clock,
            IAdditionalPropertyValueService additionalPropertyValueService,
            ICachingResolver cachingResolver)
        {
            this.customerAggregateRepository = customerAggregateRepository;
            this.customerReadModelRepository = customerReadModelRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.personReadModelRepository = personReadModelRepository;
            this.clock = clock;
            this.additionalPropertyValueService = additionalPropertyValueService;
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public CustomerAggregate? GetCustomerAggregateById(Guid tenantId, Guid id)
        {
            return this.customerAggregateRepository.GetById(tenantId, id);
        }

        /// <inheritdoc/>
        public ICustomerReadModelSummary GetCustomerById(Guid tenantId, Guid id)
        {
            return this.customerReadModelRepository.GetCustomerById(tenantId, id);
        }

        /// <inheritdoc/>
        public async Task<CustomerAggregate> CreateCustomerForNewPerson(
            Guid tenantId,
            DeploymentEnvironment environment,
            IPersonalDetails customerPersonDetails,
            Guid? ownerId,
            Guid? portalId,
            bool isTestData = false,
            List<AdditionalPropertyValueUpsertModel>? additionalProperties = null)
        {
            if (string.IsNullOrEmpty(customerPersonDetails.DisplayName))
            {
                throw new ErrorException(Errors.Customer.CreationFailedMissingValue("display name"));
            }

            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            if (portalId.HasValue)
            {
                var portal = await this.cachingResolver.GetPortalOrThrow(tenantId, portalId.Value);
                if (portal.UserType != PortalUserType.Customer)
                {
                    throw new ErrorException(Errors.User.PortalUserTypeMismatch(
                        portal.Name, portal.UserType.Humanize(), PortalUserType.Customer.Humanize()));
                }
            }

            var person = PersonAggregate.CreatePersonFromPersonalDetails(
                tenant.Id,
                customerPersonDetails.OrganisationId,
                customerPersonDetails,
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.Now(),
                isTestData);

            await this.personAggregateRepository.ApplyChangesToDbContext(person);

            return await this
                .CreateCustomerForExistingPerson(person, environment, ownerId, portalId, isTestData, additionalProperties);
        }

        /// <inheritdoc/>
        public async Task<CustomerAggregate> CreateCustomerForExistingPerson(
            PersonAggregate person,
            DeploymentEnvironment environment,
            Guid? ownerId,
            Guid? portalId,
            bool isTestData = false,
            List<AdditionalPropertyValueUpsertModel>? additionalProperties = null)
        {
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var customerAggregate = CustomerAggregate.CreateNewCustomer(
                person.TenantId, person, environment, performingUserId, portalId, this.clock.Now(), isTestData, ownerId);

            if (additionalProperties?.Any() == true)
            {
                await this.additionalPropertyValueService.UpsertValuesForCustomer(
                    customerAggregate,
                    additionalProperties,
                    performingUserId,
                    this.clock.Now());
            }

            // Insertion requires no concurrency handling.
            await this.customerAggregateRepository.ApplyChangesToDbContext(customerAggregate);

            person.AssociateWithCustomer(customerAggregate.Id, performingUserId, this.clock.Now());
            await this.personAggregateRepository.ApplyChangesToDbContext(person);

            return customerAggregate;
        }

        /// <inheritdoc/>
        public IEnumerable<IPersonReadModelSummary?> GetPersonsForCustomer(Guid tenantId, Guid customerId)
        {
            return this.personReadModelRepository.GetPersonsByCustomerId(tenantId, customerId).ToList();
        }

        public async Task UpdateCustomerDetails(
            Guid tenantId,
            Guid customerId,
            IPersonalDetails details,
            Guid? portalId,
            List<AdditionalPropertyValueUpsertModel>? additionalPropertyValueUpsertModels = null)
        {
            if (details.FullName == null)
            {
                return;
            }

            if (portalId.HasValue)
            {
                var portal = await this.cachingResolver.GetPortalOrThrow(tenantId, portalId.Value);
                if (portal.UserType != PortalUserType.Customer)
                {
                    throw new ErrorException(Errors.User.PortalUserTypeMismatch(
                        portal.Name, portal.UserType.Humanize(), PortalUserType.Customer.Humanize()));
                }
            }

            var stepName = nameof(CustomerService) + "." + nameof(CustomerService.UpdateCustomerDetails);
            using (MiniProfiler.Current.Step(stepName))
            {
                var customer = this.GetCustomerById(tenantId, customerId)
                    ?? throw new ErrorException(Errors.Customer.NotFound(customerId));
                if (tenantId != default && customer.TenantId != tenantId)
                {
                    throw new ErrorException(
                        Errors.General.NotAuthorized("update for customer", "customer", customer.Id));
                }

                PersonAggregate? personAggregate = await this.personAggregateRepository.GetByIdAsync(tenantId, customer.PrimaryPersonId);
                if (personAggregate == null)
                {
                    throw new ErrorException(Errors.Person.NotFound(customer.PrimaryPersonId));
                }
                if (additionalPropertyValueUpsertModels != null)
                {
                    personAggregate.Update(
                        details, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                    await this.additionalPropertyValueService.UpsertValuesForCustomerFromPerson(
                        customerId,
                        personAggregate,
                        additionalPropertyValueUpsertModels,
                        this.httpContextPropertiesResolver.PerformingUserId,
                        this.clock.Now());
                    await this.personAggregateRepository.ApplyChangesToDbContext(personAggregate);
                }

                if (personAggregate.UserId.HasValue)
                {
                    var userAggregate = this.userAggregateRepository.GetById(tenantId, personAggregate.UserId.Value);
                    if (userAggregate == null)
                    {
                        return;
                    }
                    if (string.IsNullOrEmpty(personAggregate.Email))
                    {
                        var additionalDetails = new List<string>
                        {
                            $"Entity Type: Customer",
                            $"Entity Id: {userAggregate.CustomerId}",
                            $"Customer Fullname: {personAggregate.FullName}",
                            $"Login Email: {userAggregate.LoginEmail}",
                        };
                        throw new ErrorException(Errors.Customer.LoginEmailAddressShouldNotBeEmpty(personAggregate.FullName, additionalDetails));
                    }

                    if (userAggregate.LoginEmail != personAggregate.Email)
                    {
                        userAggregate.SetLoginEmail(
                            personAggregate.Email, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                        await this.userAggregateRepository.ApplyChangesToDbContext(userAggregate);
                    }
                }

                if (customer.PortalId != portalId)
                {
                    var customerAggregate = await this.customerAggregateRepository.GetByIdAsync(tenantId, customerId);
                    if (customerAggregate == null)
                    {
                        throw new ErrorException(Errors.Customer.NotFound(customerId));
                    }
                    customerAggregate.ChangePortal(
                        portalId, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                    await this.customerAggregateRepository.ApplyChangesToDbContext(customerAggregate);
                }

                if (personAggregate.UserId != Guid.Empty && personAggregate.UserId.HasValue)
                {
                    var userAggregate = this.userAggregateRepository.GetById(tenantId, personAggregate.UserId.Value);
                    if (userAggregate != null)
                    {
                        if (userAggregate.PortalId != portalId)
                        {
                            userAggregate.ChangePortal(
                                portalId, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                            await this.userAggregateRepository.ApplyChangesToDbContext(userAggregate);
                        }
                    }
                }
            }
        }
    }
}
