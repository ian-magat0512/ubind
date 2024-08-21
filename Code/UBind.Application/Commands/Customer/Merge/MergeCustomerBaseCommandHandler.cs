// <copyright file="MergeCustomerBaseCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer.Merge
{
    using MediatR;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Application.Queries.Email;
    using UBind.Application.Queries.Sms;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.Repositories;

    /// <summary>
    /// A base class used by multiple merge customer command handlers.
    /// A common class to share logic for merging customers.
    /// </summary>
    public abstract class MergeCustomerBaseCommandHandler<T> : ICommandHandler<T, Unit>
        where T : ICommand
    {
        protected readonly ILogger Logger;
        protected readonly IClock Clock;

        protected readonly IClaimAggregateRepository ClaimAggregateRepository;
        protected readonly IPersonAggregateRepository PersonAggregateRepository;
        protected readonly ICustomerAggregateRepository CustomerAggregateRepository;
        protected readonly IClaimReadModelRepository ClaimReadModelRepository;
        protected readonly IEmailRepository EmailRepository;
        protected readonly IPersonReadModelRepository PersonReadModelRepository;
        protected readonly IQuoteAggregateRepository QuoteAggregateRepository;
        protected readonly IQuoteReadModelRepository QuoteReadModelRepository;
        protected readonly ISmsRepository SmsRepository;
        protected readonly IHttpContextPropertiesResolver ContextPropertiesResolver;
        protected readonly ICqrsMediator Mediator;

        public MergeCustomerBaseCommandHandler(
            IClaimAggregateRepository claimAggregateRepository,
            IClaimReadModelRepository claimReadModelRepository,
            IEmailRepository emailRepository,
            IPersonReadModelRepository personReadModelRepository,
            IQuoteAggregateRepository quoteAggregateRepository,
            IQuoteReadModelRepository quoteReadModelRepository,
            ISmsRepository smsRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            IHttpContextPropertiesResolver contextPropertiesResolver,
            IClock clock,
            ICqrsMediator mediator,
            ILogger logger)
        {
            this.PersonAggregateRepository = personAggregateRepository;
            this.CustomerAggregateRepository = customerAggregateRepository;
            this.ClaimAggregateRepository = claimAggregateRepository;
            this.ClaimReadModelRepository = claimReadModelRepository;
            this.PersonReadModelRepository = personReadModelRepository;
            this.QuoteAggregateRepository = quoteAggregateRepository;
            this.QuoteReadModelRepository = quoteReadModelRepository;
            this.ContextPropertiesResolver = contextPropertiesResolver;
            this.EmailRepository = emailRepository;
            this.SmsRepository = smsRepository;
            this.Logger = logger;
            this.Mediator = mediator;
            this.Clock = clock;
        }

        /// <summary>
        /// handle that contains the main logic of the command handler.
        /// </summary>
        public abstract Task<Unit> Handle(T request, CancellationToken cancellationToken);

        public async Task DeleteCustomer(Guid tenantId, Guid customerId)
        {
            var peopleOfCustomer = this.PersonReadModelRepository.GetPersonsByCustomerId(tenantId, customerId).ToList();

            // dont delete if there is still people, might be some sort of issue happening.
            if (peopleOfCustomer.Any())
            {
                throw new ErrorException(Errors.Customer.Merge.DeletingCustomerStillHasPerson(customerId));
            }

            // delete customer.
            CustomerAggregate aggregate = this.CustomerAggregateRepository.GetById(tenantId, customerId);
            aggregate?.MarkAsDeleted(this.ContextPropertiesResolver.PerformingUserId, this.Clock.Now());
            await this.CustomerAggregateRepository.Save(aggregate);
        }

        /// <summary>
        /// Transfer all person records from source customer to destination customer.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="sourceCustomerId">The source customer Id.</param>
        /// <param name="destinationParam">The destination param where primary person details of the customer
        /// and some additional details that will help with the merge.</param>
        /// <returns></returns>
        internal async Task AssignAllPersonsFromSourceToDestinationCustomer(
            Guid tenantId,
            Guid sourceCustomerId,
            DestinationPersonMergingModel destinationParam)
        {
            // associate dst person with a new customer.
            var sourcePeople = this.PersonReadModelRepository.GetPersonsByCustomerId(tenantId, sourceCustomerId, true).ToList();
            sourcePeople.ForEach(x => x.SetEmailIfNull());

            // check if the person has an account with duplicate emails.
            foreach (var sourcePerson in sourcePeople)
            {
                PersonAggregate sourcePersonAggregate = this.PersonAggregateRepository.GetById(tenantId, sourcePerson.Id);
                if (sourcePersonAggregate == null || sourcePersonAggregate.IsDeleted)
                {
                    continue;
                }

                var destinationPeople = this.PersonReadModelRepository.GetPersonsByCustomerId(tenantId, destinationParam.PersonCustomerId, true)
                    .ToList()
                    .OrderBy(person => person.Id != destinationParam.PersonId)
                    .ToList();
                destinationPeople.ForEach(x => x.SetEmailIfNull());

                if (destinationPeople.Any(x => x.Email == sourcePerson.Email))
                {
                    await this.AssignPersonFromSourceToDestinationCustomerWithSameEmail(
                        tenantId,
                        sourcePersonAggregate,
                        destinationPeople,
                        destinationParam);
                }
                else
                {
                    sourcePersonAggregate.AssociateWithCustomer(
                        destinationParam.PersonCustomerId,
                        this.ContextPropertiesResolver.PerformingUserId,
                        this.Clock.Now());
                    await this.PersonAggregateRepository.Save(sourcePersonAggregate);
                }
            }
        }

        /// <summary>
        /// Assigns a person from source to destination customer if they both have the same email.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="sourcePersonAggregate">The source person aggregate.</param>
        /// <param name="destinationPeople">The destination people.</param>
        internal async Task AssignPersonFromSourceToDestinationCustomerWithSameEmail(
            Guid tenantId,
            PersonAggregate sourcePersonAggregate,
            List<IPersonReadModelSummary> destinationPeople,
            DestinationPersonMergingModel destinationParam)
        {
            foreach (var destinationPerson in destinationPeople)
            {
                var destinationPersonAggregate = this.PersonAggregateRepository.GetById(tenantId, destinationPerson.Id);

                // if has same email.
                if ((destinationPersonAggregate != null || !destinationPersonAggregate.IsDeleted)
                    && sourcePersonAggregate.Email == destinationPersonAggregate.Email)
                {
                    var destinationPersonWasInvitedOrActivated = destinationPersonAggregate.UserId != null;
                    var sourcePersonWasInvitedOrActivated = sourcePersonAggregate.UserId != null;

                    // if src person is not activated but dst person is activated.
                    if (destinationPersonWasInvitedOrActivated && !sourcePersonWasInvitedOrActivated)
                    {
                        sourcePersonAggregate.MarkAsDeleted(this.ContextPropertiesResolver.PerformingUserId, this.Clock.Now());
                        await this.PersonAggregateRepository.Save(sourcePersonAggregate);
                        break;
                    }

                    // if dst person is not activated but src person is activated.
                    else if (sourcePersonWasInvitedOrActivated && !destinationPersonWasInvitedOrActivated)
                    {
                        destinationPersonAggregate.MarkAsDeleted(this.ContextPropertiesResolver.PerformingUserId, this.Clock.Now());
                        await this.PersonAggregateRepository.Save(destinationPersonAggregate);
                        sourcePersonAggregate.AssociateWithCustomer(destinationPersonAggregate.CustomerId.Value, this.ContextPropertiesResolver.PerformingUserId, this.Clock.Now());
                        await this.PersonAggregateRepository.Save(sourcePersonAggregate);
                        break;
                    }

                    // just assign it to a new customer.
                    else
                    {
                        sourcePersonAggregate.AssociateWithCustomer(
                           destinationParam.PersonCustomerId,
                           this.ContextPropertiesResolver.PerformingUserId,
                           this.Clock.Now());
                        await this.PersonAggregateRepository.Save(sourcePersonAggregate);
                        break;
                    }
                }
            }
        }

        internal async Task MergeRelatedRecords(
            Guid tenantId,
            Guid sourceCustomerId,
            DestinationPersonMergingModel destinationPersonParams)
        {
            await this.MergeQuotes(tenantId, sourceCustomerId, destinationPersonParams);
            await this.MergeClaims(tenantId, sourceCustomerId, destinationPersonParams);
            await this.MergeMessages(tenantId, sourceCustomerId, destinationPersonParams);
        }

        private async Task MergeMessages(Guid tenantId, Guid sourceCustomerId, DestinationPersonMergingModel destinationParam)
        {
            this.Logger.LogInformation("Merging messages..");
            var messageFilter = new EntityListFilters
            {
                TenantId = tenantId,
                EntityId = sourceCustomerId,
                EntityType = EntityType.Customer,
            };

            var smsQuery = new GetAllSmsByFilterQuery(tenantId, messageFilter);
            var smsList = await this.Mediator.Send(smsQuery);
            var destinationPersonDetails = destinationParam.PersonalDetails;

            foreach (var sms in smsList)
            {
                var relationships = this.SmsRepository.GetSmsRelationships(sms.TenantId, sms.Id);
                var relatedCustomerMessage = relationships.FirstOrDefault(r => r.Type == RelationshipType.CustomerMessage);
                var relatedMessageRecipient = relationships.FirstOrDefault(r => r.Type == RelationshipType.MessageRecipient);

                if (relatedCustomerMessage != null)
                {
                    this.SmsRepository.RemoveSmsRelationship(relatedCustomerMessage);
                    var customerMessage = new Domain.ReadWriteModel.Relationship(
                        tenantId,
                        EntityType.Customer,
                        destinationParam.PersonCustomerId,
                        RelationshipType.CustomerMessage,
                        EntityType.Message,
                        sms.Id,
                        this.Clock.Now());
                    this.SmsRepository.InsertSmsRelationship(customerMessage);
                }

                if (relatedMessageRecipient != null)
                {
                    this.SmsRepository.RemoveSmsRelationship(relatedMessageRecipient);
                    var messageRecipient = new Domain.ReadWriteModel.Relationship(
                        tenantId,
                        EntityType.Person,
                        destinationParam.PersonId,
                        RelationshipType.MessageRecipient,
                        EntityType.Message,
                        sms.Id,
                        this.Clock.Now());
                    this.SmsRepository.InsertSmsRelationship(messageRecipient);
                }
            }

            var emailQuery = new GetEmailSummariesByFilterQuery(tenantId, messageFilter);
            var emails = await this.Mediator.Send(emailQuery);

            foreach (var email in emails)
            {
                var relationships = this.EmailRepository.GetRelationships(tenantId, email.Id);
                var relatedCustomerMessage = relationships.FirstOrDefault(r => r.Type == RelationshipType.CustomerMessage);
                var relatedMessageRecipient = relationships.FirstOrDefault(r => r.Type == RelationshipType.MessageRecipient);

                if (relatedCustomerMessage != null)
                {
                    this.EmailRepository.RemoveEmailRelationship(relatedCustomerMessage);
                    var customerMessage = new Domain.ReadWriteModel.Relationship(
                        tenantId,
                        EntityType.Customer,
                        destinationParam.PersonCustomerId,
                        RelationshipType.CustomerMessage,
                        EntityType.Message,
                        email.Id,
                        this.Clock.Now());
                    this.EmailRepository.InsertEmailRelationship(customerMessage);
                }

                if (relatedMessageRecipient != null)
                {
                    this.EmailRepository.RemoveEmailRelationship(relatedMessageRecipient);
                    var messageRecipient = new Domain.ReadWriteModel.Relationship(
                        tenantId,
                        EntityType.Person,
                        destinationParam.PersonId,
                        RelationshipType.MessageRecipient,
                        EntityType.Message,
                        email.Id,
                        this.Clock.Now());
                    this.EmailRepository.InsertEmailRelationship(messageRecipient);
                }
            }
        }

        private async Task MergeClaims(Guid tenantId, Guid sourceCustomerId, DestinationPersonMergingModel destinationParam)
        {
            this.Logger.LogInformation("Merging claims..");
            var claimFilters = new EntityListFilters
            {
                TenantId = tenantId,
                CustomerId = sourceCustomerId,
            };

            var destinationPersonDetails = destinationParam.PersonalDetails;
            var claims = this.ClaimReadModelRepository.ListClaims(tenantId, claimFilters).ToList();

            foreach (var claim in claims)
            {
                async Task UpdateClaim()
                {
                    var claimAggregate = this.ClaimAggregateRepository.GetById(claim.TenantId, claim.Id);
                    claimAggregate.AssignToCustomer(
                        destinationParam.PersonCustomerId,
                        destinationParam.PersonId,
                        destinationPersonDetails,
                        this.ContextPropertiesResolver.PerformingUserId,
                        this.Clock.Now());

                    // change owner if there is a new owner, and its a different owner from the quote.
                    if (destinationParam.AssignToNewOwner)
                    {
                        var ownerPerson = this.PersonReadModelRepository.GetPersonById(tenantId, destinationParam.OwnerPersonId.Value);
                        if (ownerPerson.UserId != null
                            && claimAggregate.OwnerUserId != ownerPerson.UserId.Value)
                        {
                            claimAggregate.AssignToOwner(
                                ownerPerson.UserId.Value,
                                this.ContextPropertiesResolver.PerformingUserId,
                                this.Clock.Now());
                        }
                    }

                    await this.ClaimAggregateRepository.Save(claimAggregate);
                }

                await ConcurrencyPolicy.ExecuteWithRetriesAsync(UpdateClaim);
            }
        }

        /// <summary>
        /// Merges the quotes from the source customer to the destination person (which is the primary person of the dst customer) .
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="sourceCustomerId">The customer id where the quotes will came from.</param>
        /// <param name="destinationPersonParam">this param the person where the quotes will be transfered association to
        /// and the new owners of the newly merged records.</param>
        /// <param name="newOwnerPersonId">reassign owner of the quote to this person Id, if it has one.</param>
        /// <returns></returns>
        private async Task MergeQuotes(Guid tenantId, Guid sourceCustomerId, DestinationPersonMergingModel destinationPersonParam)
        {
            this.Logger.LogInformation("Merging quotes..");
            var quoteFilters = new QuoteReadModelFilters
            {
                TenantId = tenantId,
                CustomerId = sourceCustomerId,
            };

            var quotes = this.QuoteReadModelRepository.ListQuotes(tenantId, quoteFilters).ToList();

            var destinationPersonDetails = destinationPersonParam.PersonalDetails;
            foreach (var quote in quotes)
            {
                async Task UpdateQuote()
                {
                    var quoteAggregate = this.QuoteAggregateRepository.GetById(quote.TenantId, quote.AggregateId);
                    quoteAggregate.RecordAssociationWithCustomer(
                        destinationPersonParam.PersonCustomerId,
                        destinationPersonParam.PersonId,
                        destinationPersonDetails,
                        this.ContextPropertiesResolver.PerformingUserId,
                        this.Clock.Now());

                    quoteAggregate.UpdateCustomerDetails(
                        destinationPersonDetails, this.ContextPropertiesResolver.PerformingUserId, this.Clock.Now(), quote.QuoteId);

                    // change owner if there is a new owner, and its a different owner from the quote.
                    if (destinationPersonParam.AssignToNewOwner)
                    {
                        var ownerPerson = this.PersonReadModelRepository.GetPersonById(tenantId, destinationPersonParam.OwnerPersonId.Value);
                        if (ownerPerson.UserId != null
                            && quoteAggregate.OwnerUserId != ownerPerson.UserId.Value)
                        {
                            quoteAggregate.AssignToOwner(
                                ownerPerson.UserId.Value,
                                ownerPerson.Id,
                                ownerPerson.FullName,
                                this.ContextPropertiesResolver.PerformingUserId,
                                this.Clock.Now());
                        }
                    }

                    await this.QuoteAggregateRepository.Save(quoteAggregate);
                }

                await ConcurrencyPolicy.ExecuteWithRetriesAsync(UpdateQuote);
            }
        }
    }
}
