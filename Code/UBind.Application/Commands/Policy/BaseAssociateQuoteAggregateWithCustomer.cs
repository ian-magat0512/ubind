// <copyright file="BaseAssociateQuoteAggregateWithCustomer.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Policy
{
    using System;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Commands;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using Errors = UBind.Domain.Errors;

    /// <summary>
    /// A command handler that is responsible for associating a quote aggregate with an existing customer record.
    /// </summary>
    public class BaseAssociateQuoteAggregateWithCustomer
    {
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly IClock clock;

        public BaseAssociateQuoteAggregateWithCustomer(
            IQuoteAggregateRepository quoteAggregateRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            ICustomerReadModelRepository customerReadModelRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.customerAggregateRepository = customerAggregateRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.customerReadModelRepository = customerReadModelRepository;
            this.HttpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        protected IHttpContextPropertiesResolver HttpContextPropertiesResolver { get; set; }

        public async Task<Unit> AssociateQuoteAggregateWithCustomer(Guid tenantId, Guid customerId, QuoteAggregate quoteAggregate, Guid? quoteId)
        {
            var customerAggregate = this.GetCustomerAggregate(tenantId, customerId);
            this.ThrowIfCustomerDoesntHaveEmailAddress(tenantId, customerId);

            var personAggregate = this.personAggregateRepository.GetById(customerAggregate.TenantId, customerAggregate.PrimaryPersonId);
            if (personAggregate == null)
            {
                throw new ErrorException(Errors.Person.NotFound(customerAggregate.PrimaryPersonId));
            }

            var personalDetails = new PersonalDetails(personAggregate);

            // This updates the customer details and assignment on the aggregate level and its read model
            quoteAggregate.RecordAssociationWithCustomer(
                customerAggregate, personAggregate, this.HttpContextPropertiesResolver.PerformingUserId, this.clock.Now());

            // This updates the customer details on the quote aggregate quote entity level (aggregate can have multiple quotes)
            var quote = quoteId.HasValue ? quoteAggregate.GetQuote(quoteId.Value) : null;
            if (quote != null)
            {
                quoteAggregate.UpdateCustomerDetails(
                personalDetails, this.HttpContextPropertiesResolver.PerformingUserId, this.clock.Now(), quote.Id);
            }

            if (quoteAggregate.Policy != null)
            {
                var contactEmailPatchCommand = this.PatchPolicyData(
                new JsonPath("contactEmail"), new JsonPath("questions.contact.contactEmail"), personAggregate.Email);
                quoteAggregate.PatchFormData(contactEmailPatchCommand, this.HttpContextPropertiesResolver.PerformingUserId, this.clock.Now());

                var contactNamePatchCommand = this.PatchPolicyData(
                    new JsonPath("contactName"), new JsonPath("questions.contact.contactName"), personAggregate.FullName);
                quoteAggregate.PatchFormData(contactNamePatchCommand, this.HttpContextPropertiesResolver.PerformingUserId, this.clock.Now());

                var mobilePhonePatchCommand = this.PatchPolicyData(
                    new JsonPath("contactMobile"), new JsonPath("questions.contact.contactMobile"), personAggregate.MobilePhoneNumber);
                quoteAggregate.PatchFormData(mobilePhonePatchCommand, this.HttpContextPropertiesResolver.PerformingUserId, this.clock.Now());

                var homePhonePatchCommand = this.PatchPolicyData(
                    new JsonPath("contactPhone"), new JsonPath("questions.contact.contactPhone"), personAggregate.HomePhoneNumber);
                quoteAggregate.PatchFormData(homePhonePatchCommand, this.HttpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            }

            await this.quoteAggregateRepository.Save(quoteAggregate);

            return Unit.Value;
        }

        protected CustomerReadModelDetail GetCustomerDetails(Guid tenantId, Guid customerId)
        {
            var customerDetails = this.customerReadModelRepository.GetCustomerById(tenantId, customerId);
            if (customerDetails == null)
            {
                throw new ErrorException(Errors.Customer.NotFound(customerId));
            }

            return customerDetails;
        }

        private PolicyDataPatchCommand PatchPolicyData(JsonPath formDataPath, JsonPath calculationResultPath, string value)
        {
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                formDataPath, calculationResultPath, value, PolicyDataPatchScope.CreateGlobalPatchScope(), PatchRules.None);
            return patchCommand;
        }

        private CustomerAggregate GetCustomerAggregate(Guid tenantId, Guid customerId)
        {
            // Make sure that the new customer Id is an existing record
            var customerAggregate = this.customerAggregateRepository.GetById(tenantId, customerId);
            if (customerAggregate == null)
            {
                throw new ErrorException(Errors.Customer.NotFound(customerId));
            }

            return customerAggregate;
        }

        private CustomerReadModelDetail ThrowIfCustomerDoesntHaveEmailAddress(Guid tenantId, Guid customerId)
        {
            // Expecting that all customer aggregate must have a customer read model and an email address
            var customer = this.customerReadModelRepository.GetCustomerById(tenantId, customerId);
            if (customer == null)
            {
                throw new ErrorException(Errors.Customer.NotFound(customerId));
            }

            return customer;
        }
    }
}
