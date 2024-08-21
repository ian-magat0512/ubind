// <copyright file="AssociateQuoteWithCustomerUserCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Json;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    /// <summary>
    /// Represents the handler for associating a quote to a customer with a user account.
    /// </summary>
    public class AssociateQuoteWithCustomerUserCommandHandler : ICommandHandler<AssociateQuoteWithCustomerUserCommand, Unit>
    {
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IAggregateLockingService aggregateLockingService;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;

        public AssociateQuoteWithCustomerUserCommandHandler(
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            IQuoteAggregateRepository quoteAggregateRepository,
            IUserAggregateRepository userAggregateRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IAggregateLockingService aggregateLockingService)
        {
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.aggregateLockingService = aggregateLockingService;
            this.userAggregateRepository = userAggregateRepository;
            this.customerAggregateRepository = customerAggregateRepository;
            this.personAggregateRepository = personAggregateRepository;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(AssociateQuoteWithCustomerUserCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId);
            using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, AggregateType.Quote))
            {
                var quoteAggregate = this.quoteAggregateRepository.GetById(command.TenantId, quoteAggregateId);
                if (quoteAggregate == null)
                {
                    throw new ErrorException(Errors.Quote.AssociationInvitation.QuoteNotFound(command.QuoteId, command.AssociationInvitationId));
                }

                await this.AssociateWithCustomer(quoteAggregate, command.AssociationInvitationId, command.QuoteId, command.PerformingUserId);
                return Unit.Value;
            }
        }

        private async Task AssociateWithCustomer(QuoteAggregate quoteAggregate, Guid associationInvitationId, Guid quoteId, Guid performingUserId)
        {
            var userAggregate = this.userAggregateRepository.GetById(quoteAggregate.TenantId, performingUserId);
            userAggregate = EntityHelper.ThrowIfNotFound(userAggregate, performingUserId, "user");
            if (userAggregate.CustomerId == default)
            {
                throw new ErrorException(Errors.Quote.AssociationInvitation.CustomerUserNotFound(associationInvitationId));
            }

            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            quoteAggregate.VerifyCustomerAssociationInvitation(associationInvitationId, performingUserId, this.clock.Now());
            userAggregate = EntityHelper.ThrowIfNotFound(userAggregate, performingUserId, "user");
            if (!userAggregate.CustomerId.HasValue)
            {
                throw new ErrorException(
                    Errors.Quote.AssociationInvitation.CustomerUserNotFound(associationInvitationId));
            }
            var customerUserAggregate = this.customerAggregateRepository.GetById(userAggregate.TenantId, userAggregate.CustomerId.Value);
            customerUserAggregate = EntityHelper.ThrowIfNotFound(customerUserAggregate, userAggregate.CustomerId.Value, "customer");
            var personAggregate = this.personAggregateRepository.GetById(userAggregate.TenantId, userAggregate.PersonId);
            personAggregate = EntityHelper.ThrowIfNotFound(personAggregate, userAggregate.PersonId, "person");
            quoteAggregate.RecordAssociationWithCustomer(
                customerUserAggregate,
                personAggregate,
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.Now());

            var personalDetails = new PersonalDetails(personAggregate);
            quoteAggregate.UpdateCustomerDetails(
                personalDetails, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now(), quote.Id);

            var formData = new Domain.Aggregates.Quote.FormData(quote.LatestFormData.Data.Json);
            const string contactNamePath = "contactName";
            if (formData.GetValue(contactNamePath) != personAggregate.FullName)
            {
                formData.PatchFormModelProperty(new JsonPath(contactNamePath), personAggregate.FullName);
            }

            quote.UpdateFormData(formData, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            await this.quoteAggregateRepository.Save(quoteAggregate);
        }
    }
}
