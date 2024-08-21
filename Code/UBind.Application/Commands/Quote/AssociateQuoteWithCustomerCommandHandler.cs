// <copyright file="AssociateQuoteWithCustomerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Application.Commands.Messages;
    using UBind.Application.Commands.Policy;
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
    /// A command handler that is responsible for associating quote with an existing customer record.
    /// </summary>
    public class AssociateQuoteWithCustomerCommandHandler
        : BaseAssociateQuoteAggregateWithCustomer, ICommandHandler<AssociateQuoteWithCustomerCommand, Unit>
    {
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly IEmailRepository emailRepository;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly ICqrsMediator cqrsMediator;

        public AssociateQuoteWithCustomerCommandHandler(
            IQuoteReadModelRepository quoteReadModelRepository,
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
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.emailRepository = emailRepository;
            this.cqrsMediator = cqrsMediator;
        }

        public async Task<Unit> Handle(AssociateQuoteWithCustomerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quote = this.quoteReadModelRepository.GetById(request.TenantId, request.QuoteId);
            this.ThrowIfCustomerIsNotInlineWithQuote(request.TenantId, request.CustomerId, quote);
            await this.AssociateQuoteWithCustomer(request.TenantId, request.QuoteId, quote.CustomerId.GetValueOrDefault(), request.CustomerId);
            return Unit.Value;
        }

        private async Task AssociateQuoteWithCustomer(Guid tenantId, Guid quoteId, Guid previousCustomerId, Guid customerId)
        {
            // get the quote aggregate for policy issued with a quote or for a policy issued without quote
            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(tenantId, quoteId);
            await this.AssociateQuoteAggregateWithCustomer(tenantId, customerId, quoteAggregate, quoteId);
            await this.AssociateQuoteEmailsRelationshipWithCustomer(tenantId, quoteId, previousCustomerId, customerId);
        }

        private async Task AssociateQuoteEmailsRelationshipWithCustomer(Guid tenantId, Guid quoteId, Guid previousCustomerId, Guid newCustomerId)
        {
            var emailIds = this.emailRepository.GetByQuoteId(tenantId, quoteId, new EntityListFilters()).Select(email => email.Id)?.ToList();
            if (emailIds?.Any() ?? false)
            {
                var command = new AssociateEmailRelationshipWithCustomerCommand(tenantId, emailIds, previousCustomerId, newCustomerId);
                await this.cqrsMediator.Send(command);
            }
        }

        private void ThrowIfCustomerIsNotInlineWithQuote(Guid tenantId, Guid customerId, NewQuoteReadModel quote)
        {
            var customerDetail = this.GetCustomerDetails(tenantId, customerId);

            // Make sure that we are not assigning the same associated customer to the quote
            if (quote.CustomerId != null && customerDetail.Id == quote.CustomerId.Value)
            {
                throw new ErrorException(Domain.Errors.Quote.CannotAssociateWithTheSameCustomerId(customerDetail.Id));
            }

            if (customerDetail.Environment != quote.Environment)
            {
                throw new ErrorException(Domain.Errors.Quote.AssociationWithCustomer.MismatchedEnvironment(
                    quote.Id, quote.Environment, customerDetail.Id, customerDetail.Environment));
            }

            if (customerDetail.TenantId != quote.TenantId)
            {
                throw new ErrorException(Domain.Errors.Quote.AssociationWithCustomer.MismatchedTenant(
                    quote.Id, quote.TenantId, customerDetail.Id, customerDetail.TenantId));
            }

            if (customerDetail.OrganisationId != quote.OrganisationId)
            {
                throw new ErrorException(Domain.Errors.Quote.AssociationWithCustomer.MismatchedOrganisation(
                    quote.Id, quote.OrganisationId, customerDetail.Id, customerDetail.OrganisationId));
            }
        }
    }
}
