// <copyright file="AssociatePolicyWithCustomerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Policy
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Application.Commands.Claim;
    using UBind.Application.Commands.Messages;
    using UBind.Application.Queries.Policy;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;

    /// <summary>
    /// Command handler for associating a policy with a customer. The policy can be:
    /// - an issued policy from a completed quote
    /// - an imported policy without a quote
    /// This also associates related claims to the customer.
    /// </summary>
    public class AssociatePolicyWithCustomerCommandHandler
        : BaseAssociateQuoteAggregateWithCustomer, ICommandHandler<AssociatePolicyWithCustomerCommand>
    {
        private readonly IEmailRepository emailRepository;
        private readonly ICqrsMediator cqrsMediator;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;

        public AssociatePolicyWithCustomerCommandHandler(
            IEmailRepository emailRepository,
            ICqrsMediator cqrsMediator,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IQuoteAggregateRepository quoteAggregateRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            ICustomerReadModelRepository customerReadModelRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
            : base(
                  quoteAggregateRepository,
                  customerAggregateRepository,
                  personAggregateRepository,
                  customerReadModelRepository,
                  httpContextPropertiesResolver,
                  clock)
        {
            this.emailRepository = emailRepository;
            this.cqrsMediator = cqrsMediator;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
        }

        public async Task<Unit> Handle(AssociatePolicyWithCustomerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.AssociatePolicyWithCustomer(request.TenantId, request.PolicyId, request.CustomerId);
            return Unit.Value;
        }

        protected async Task AssociatePolicyWithCustomer(Guid tenantId, Guid policyId, Guid customerId)
        {
            var policySummary = await this.GetPolicySummary(tenantId, policyId);
            this.ThrowIfNewCustomerIsNotInlineWithPolicy(tenantId, customerId, policySummary);

            // get the quote aggregate for policy issued with a quote or for a policy issued without quote
            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForPolicy(tenantId, policyId);
            await this.AssociateQuoteAggregateWithCustomer(tenantId, customerId, quoteAggregate, policySummary.QuoteId);

            // Since a policy can exist without a customer, we only associate email relationships for the existing customer emails
            if (policySummary.CustomerId != null)
            {
                await this.AssociatePolicyEmailsRelationshipWithCustomer(tenantId, policySummary.PolicyId, policySummary.CustomerId.Value, customerId);
            }

            await this.AssociateClaimsWithCustomer(tenantId, policySummary.PolicyId, customerId);
        }

        private async Task AssociateClaimsWithCustomer(Guid tenantId, Guid policyId, Guid newCustomerId)
        {
            var command = new AssociateClaimWithCustomerCommand(tenantId, policyId, newCustomerId);
            await this.cqrsMediator.Send(command);
        }

        private async Task AssociatePolicyEmailsRelationshipWithCustomer(Guid tenantId, Guid policyId, Guid previousCustomerId, Guid newCustomerId)
        {
            var emailIds = this.emailRepository.GetByPolicyId(tenantId, policyId, new EntityListFilters()).Select(email => email.Id)?.ToList();
            if (emailIds?.Any() ?? false)
            {
                var command = new AssociateEmailRelationshipWithCustomerCommand(tenantId, emailIds, previousCustomerId, newCustomerId);
                await this.cqrsMediator.Send(command);
            }
        }

        private async Task<IPolicyReadModelSummary> GetPolicySummary(Guid tenantId, Guid policyId)
        {
            var policySummaryQuery = new GetPolicySummaryByIdQuery(tenantId, policyId);
            return await this.cqrsMediator.Send(policySummaryQuery);
        }

        private void ThrowIfNewCustomerIsNotInlineWithPolicy(Guid tenantId, Guid customerId, IPolicyReadModelSummary policySummary)
        {
            var customerDetail = this.GetCustomerDetails(tenantId, customerId);

            // Make sure that we are not assigning the same associated customer to the quote
            if (policySummary.CustomerId.HasValue && customerDetail.Id == policySummary.CustomerId.Value)
            {
                throw new ErrorException(Domain.Errors.Policy.CannotAssociateWithTheSameCustomerId(customerDetail.Id));
            }

            if (customerDetail.Environment != policySummary.Environment)
            {
                throw new ErrorException(Domain.Errors.Policy.AssociationWithCustomer.MismatchedEnvironment(
                    policySummary.PolicyId, policySummary.Environment, customerDetail.Id, customerDetail.Environment));
            }

            if (customerDetail.TenantId != policySummary.TenantId)
            {
                throw new ErrorException(Domain.Errors.Policy.AssociationWithCustomer.MismatchedTenant(
                    policySummary.PolicyId, policySummary.TenantId, customerDetail.Id, customerDetail.TenantId));
            }

            if (customerDetail.OrganisationId != policySummary.OrganisationId)
            {
                throw new ErrorException(Domain.Errors.Policy.AssociationWithCustomer.MismatchedOrganisation(
                    policySummary.PolicyId, policySummary.OrganisationId, customerDetail.Id, customerDetail.OrganisationId));
            }
        }
    }
}
