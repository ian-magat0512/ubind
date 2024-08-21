// <copyright file="CustomerReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LinqKit;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.AdditionalPropertyValue;

    /// <summary>
    /// Reponsible for updating the user read model.
    /// </summary>
    public class CustomerReadModelWriter : ICustomerReadModelWriter
    {
        private readonly IWritableReadModelRepository<CustomerReadModel> customerReadModelRepository;
        private readonly IWritableReadModelRepository<PersonReadModel> personReadModelRepository;
        private readonly IWritableReadModelRepository<UserReadModel> userReadModelRepository;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly IEmailRepository emailRepository;
        private readonly PropertyTypeEvaluatorService propertyTypeEvaluatorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerReadModelWriter"/> class.
        /// </summary>
        /// <param name="customerReadModelRepository">The customer read model repository.</param>
        /// <param name="personReadModelRepository">The person read model repository.</param>
        /// <param name="propertyTypeEvaluatorService">Returns which service to use in persisting the additional
        /// property values.</param>
        public CustomerReadModelWriter(
            IWritableReadModelRepository<CustomerReadModel> customerReadModelRepository,
            IWritableReadModelRepository<PersonReadModel> personReadModelRepository,
            IWritableReadModelRepository<UserReadModel> userReadModelRepository,
            IPolicyReadModelRepository policyReadModelRepository,
            IQuoteReadModelRepository quoteReadModelRepository,
            IEmailRepository emailRepository,
            PropertyTypeEvaluatorService propertyTypeEvaluatorService)
        {
            this.customerReadModelRepository = customerReadModelRepository;
            this.personReadModelRepository = personReadModelRepository;
            this.propertyTypeEvaluatorService = propertyTypeEvaluatorService;
            this.userReadModelRepository = userReadModelRepository;
            this.policyReadModelRepository = policyReadModelRepository;
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.emailRepository = emailRepository;
        }

        public void Dispatch(
            CustomerAggregate aggregate,
            IEvent<CustomerAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Dispatch(
            UserAggregate aggregate,
            IEvent<UserAggregate, Guid> @event,
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

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerInitializedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read model
                this.customerReadModelRepository.DeleteById(@event.TenantId, @event.AggregateId);
            }

            var person = this.GetPersonById(@event.TenantId, @event.PersonData.PersonId);
            var customer = new CustomerReadModel(
                @event.AggregateId,
                person,
                @event.Environment,
                @event.PortalId,
                @event.Timestamp,
                @event.IsTestData);
            if (@event.OwnerUserId.HasValue)
            {
                customer.OwnerUserId = @event.OwnerUserId;
                var ownerPerson = this.GetPersonByUserId(@event.TenantId, @event.OwnerUserId.Value);
                customer.OwnerPersonId = ownerPerson.Id;
                customer.OwnerFullName = ownerPerson.FullName;
            }

            customer.LastModifiedTimestamp = @event.Timestamp;
            this.customerReadModelRepository.Add(customer);
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerImportedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read model
                this.customerReadModelRepository.DeleteById(@event.TenantId, @event.AggregateId);
            }

            var person = this.GetPersonById(@event.TenantId, @event.PersonData.PersonId);
            var customer = new CustomerReadModel(
                @event.AggregateId,
                person,
                @event.Environment,
                @event.PortalId,
                @event.Timestamp,
                @event.IsTestData);
            customer.LastModifiedTimestamp = @event.Timestamp;
            this.customerReadModelRepository.Add(customer);
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerOrganisationMigratedEvent @event, int sequenceNumber)
        {
            var customer = this.GetCustomerById(@event.TenantId, @event.CustomerId);
            customer.OrganisationId = @event.OrganisationId;
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerModifiedTimeUpdatedEvent @event, int sequenceNumber)
        {
            var customer = this.GetCustomerById(@event.TenantId, @event.AggregateId);
            customer.LastModifiedTimestamp = @event.ModifiedTime;
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.OwnershipAssignedEvent @event, int sequenceNumber)
        {
            var customer = this.GetCustomerById(@event.TenantId, @event.AggregateId);
            customer.OwnerUserId = @event.UserId;
            customer.OwnerPersonId = @event.PersonId;
            customer.OwnerFullName = @event.FullName;
            customer.LastModifiedTimestamp = @event.Timestamp;
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.OwnershipUnassignedEvent @event, int sequenceNumber)
        {
            var customer = this.GetCustomerById(@event.TenantId, @event.AggregateId);
            customer.OwnerUserId = default;
            customer.OwnerPersonId = default;
            customer.OwnerFullName = default;
            customer.LastModifiedTimestamp = @event.Timestamp;
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.UserAssociatedEvent @event, int sequenceNumber)
        {
            foreach (var customer in this.GetCustomersByPrimaryPersonId(@event.TenantId, @event.AggregateId))
            {
                customer.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerDeletedEvent @event, int sequenceNumber)
        {
            var customer = this.GetCustomerById(@event.TenantId, @event.CustomerId);
            if (@event.IsPermanentDelete)
            {
                this.customerReadModelRepository.DeleteById(@event.TenantId, @event.CustomerId);
                this.emailRepository.DeleteEmailsAndAttachmentsForEntity(
                    @event.TenantId,
                    aggregate.Environment,
                    EntityType.Customer,
                    @event.CustomerId);

                var quoteIds = this.quoteReadModelRepository.ListQuoteIdsFromCustomer(
                    @event.TenantId,
                    @event.CustomerId,
                    aggregate.Environment);
                if (quoteIds != null)
                {
                    quoteIds.ForEach(q =>
                        this.emailRepository.DeleteEmailsAndAttachmentsForEntity(
                            @event.TenantId,
                            aggregate.Environment,
                            EntityType.Quote,
                            q));
                }

                var policyIds = this.policyReadModelRepository.ListPolicyIdsFromCustomer(
                    @event.TenantId,
                    @event.CustomerId,
                    aggregate.Environment);
                if (policyIds != null)
                {
                    policyIds.ForEach(p =>
                        this.emailRepository.DeleteEmailsAndAttachmentsForEntity(
                            @event.TenantId,
                            aggregate.Environment,
                            EntityType.Policy,
                            p));
                }
            }
            else
            {
                customer.IsDeleted = true;
                customer.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerUndeletedEvent @event, int sequenceNumber)
        {
            var customer = this.GetCustomerById(@event.TenantId, @event.CustomerId);

            if (customer == null)
            {
                return;
            }

            customer.IsDeleted = false;
            customer.LastModifiedTimestamp = @event.Timestamp;
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.FullNameUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var customer in this.GetCustomersByPrimaryPersonId(@event.TenantId, @event.AggregateId))
            {
                customer.OwnerFullName = @event.Value;
                customer.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var customer in this.GetCustomersByPrimaryPersonId(@event.TenantId, @event.AggregateId))
            {
                customer.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.AssociatedWithCustomerEvent @event, int sequenceNumber)
        {
            var customer = this.GetCustomerById(@event.TenantId, @event.CustomerId);
            if (customer == null)
            {
                return;
            }

            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                customer.People.Add(person);
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserInitializedEvent @event, int sequenceNumber)
        {
            if (@event.CustomerId.HasValue && @event.CustomerId.Value != default)
            {
                var customer = this.GetCustomerById(@event.TenantId, @event.CustomerId.Value);
                if (@event.PortalId.HasValue)
                {
                    customer.PortalId = @event.PortalId;
                }

                customer.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserImportedEvent @event, int sequenceNumber)
        {
            if (@event.CustomerId.HasValue && @event.CustomerId.Value != default)
            {
                var customer = this.GetCustomerById(@event.TenantId, @event.CustomerId.Value);
                if (@event.PortalId.HasValue)
                {
                    customer.PortalId = @event.PortalId;
                }

                customer.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserOrganisationMigratedEvent @event, int sequenceNumber)
        {
            var customers = this.GetCustomersByPrimaryPersonId(@event.TenantId, @event.PersonId);
            foreach (var customer in customers)
            {
                customer.OrganisationId = @event.OrganisationId;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            var customers = this.GetCustomersByPrimaryPersonId(@event.TenantId, @event.PersonId);
            foreach (var customer in customers)
            {
                customer.OrganisationId = @event.OrganisationId;
                customer.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.LoginEmailSetEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(UserAggregate aggregate, UserAggregate.RoleAddedEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserTypeUpdatedEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserBlockedEvent @event, int sequenceNumber)
        {
            var customers = this.GetCustomersByPrimaryPersonId(@event.TenantId, @event.PersonId);
            foreach (var customer in customers)
            {
                customer.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserUnblockedEvent @event, int sequenceNumber)
        {
            var customers = this.GetCustomersByPrimaryPersonId(@event.TenantId, @event.PersonId);
            foreach (var customer in customers)
            {
                customer.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.ActivationInvitationCreatedEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserActivatedEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(UserAggregate aggregate, UserAggregate.PasswordResetInvitationCreatedEvent @event, int sequenceNumber)
        {
            // Nop.
        }

        public void Handle(UserAggregate aggregate, UserAggregate.PasswordChangedEvent @event, int sequenceNumber)
        {
            // Nop.
        }

        public void Handle(UserAggregate aggregate, UserAggregate.RoleAssignedEvent @event, int sequenceNumber)
        {
            // Nop.
        }

        public void Handle(UserAggregate aggregate, UserAggregate.RoleRetractedEvent @event, int sequenceNumber)
        {
            // Nop.
        }

        public void Handle(UserAggregate aggregate, UserAggregate.ProfilePictureAssignedToUserEvent @event, int sequenceNumber)
        {
            // Nop.
        }

        public void Handle(UserAggregate aggregate, UserAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserModifiedTimeUpdatedEvent @event, int sequenceNumber)
        {
            // Nothing to do
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            var customer = this.GetCustomerById(@event.TenantId, @event.AggregateId);
            customer.TenantId = @event.TenantId;
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            var customer = this.GetCustomerById(@event.TenantId, @event.AggregateId);
            customer.OrganisationId = @event.OrganisationId;
            customer.LastModifiedTimestamp = @event.Timestamp;
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            var customer = this.customerReadModelRepository
                .SingleMaybe(@event.TenantId, u => u.PrimaryPersonId == @event.AggregateId);
            if (customer.HasValue)
            {
                customer.Value.TenantId = @event.TenantId;
            }
        }

        public void Handle(
            CustomerAggregate aggregate,
            AdditionalPropertyValueUpdatedEvent<CustomerAggregate, ICustomerEventObserver> @event,
            int sequenceNumber)
        {
            this.propertyTypeEvaluatorService.UpdateAdditionalPropertyValue(
                (TGuid<Tenant>)@event.TenantId,
                @event.EntityId,
                @event.AdditionalPropertyDefinitionType,
                (TGuid<AdditionalPropertyDefinition>)@event.AdditionalPropertyDefinitionId,
                (TGuid<AdditionalPropertyValue>)@event.AdditionalPropertyValueId,
                @event.Value);

            var customer = this.GetCustomerById(@event.TenantId, @event.EntityId);
            if (customer != null)
            {
                customer.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(
            CustomerAggregate aggregate,
            AdditionalPropertyValueInitializedEvent<CustomerAggregate, ICustomerEventObserver> @event,
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

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonOrganisationMigratedEvent @event, int sequenceNumber)
        {
            var customer = this.customerReadModelRepository
                .SingleMaybe(@event.TenantId, u => u.PrimaryPersonId == @event.AggregateId);
            if (customer.HasValue)
            {
                customer.Value.OrganisationId = @event.OrganisationId;
            }
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.PortalChangedEvent @event, int sequenceNumber)
        {
            var customer = this.GetCustomerById(@event.TenantId, @event.AggregateId);
            customer.PortalId = @event.Value;
            customer.LastModifiedTimestamp = @event.Timestamp;
        }

        public void Handle(UserAggregate aggregate, UserAggregate.PortalChangedEvent @event, int sequenceNumber)
        {
            // we don't need to handle this because the customer aggregate also has a
            // PortalChangedEvent, which we do handle.
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            var customer = this.customerReadModelRepository
                .SingleMaybe(@event.TenantId, u => u.PrimaryPersonId == @event.AggregateId);
            if (customer.HasValue)
            {
                customer.Value.OrganisationId = @event.OrganisationId;
            }
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerSetPrimaryPersonEvent @event, int sequenceNumber)
        {
            var customer = this.GetCustomerById(@event.TenantId, @event.AggregateId);
            customer.PrimaryPersonId = @event.Value;
            customer.LastModifiedTimestamp = @event.Timestamp;
        }

        public void Handle(UserAggregate.CustomerIdAndEnvironmentUpdatedEvent @event, int sequenceNumber)
        {
            // Nop
        }

        private CustomerReadModel GetCustomerById(Guid tenantId, Guid customerId)
        {
            return this.customerReadModelRepository.GetById(tenantId, customerId);
        }

        private List<CustomerReadModel> GetCustomersByPrimaryPersonId(Guid tenantId, Guid personId)
        {
            return this.customerReadModelRepository
                .Where(tenantId, u => u.PrimaryPersonId == personId)
                .ToList();
        }

        private PersonReadModel GetPersonById(Guid tenantId, Guid personId)
        {
            return this.personReadModelRepository.GetById(tenantId, personId);
        }

        private PersonReadModel GetPersonByUserId(Guid tenantId, Guid userId)
        {
            var user = this.userReadModelRepository.GetById(tenantId, userId);
            if (user == null)
            {
                return null;
            }

            return this.personReadModelRepository.GetById(tenantId, user.PersonId);
        }
    }
}
