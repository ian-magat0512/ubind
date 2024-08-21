// <copyright file="CreateCustomerPersonUserAccountCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.Commands.Customer.Merge;
    using UBind.Application.Person;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using IClock = NodaTime.IClock;
    using IUserService = UBind.Application.User.IUserService;

    /// <summary>
    /// Command handler responsible to create a customer user and merge existing customers that use the same contact email.
    /// </summary>
    public class CreateCustomerPersonUserAccountCommandHandler
        : ICommandHandler<CreateCustomerPersonUserAccountCommand, Guid>
    {
        private readonly ICustomerService customerService;
        private readonly IPersonService personService;
        private readonly IUserService userService;
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IMediator mediator;
        private readonly IClock clock;
        private readonly IUserActivationInvitationService userActivationInvitationService;

        public CreateCustomerPersonUserAccountCommandHandler(
            ICustomerService customerService,
            IPersonService personService,
            IUserService userService,
            IUserActivationInvitationService userActivationInvitationService,
            IPersonReadModelRepository personReadModelRepository,
            IPersonAggregateRepository personAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IMediator mediator,
            IClock clock)
        {
            this.customerService = customerService;
            this.personService = personService;
            this.userService = userService;
            this.userActivationInvitationService = userActivationInvitationService;
            this.personReadModelRepository = personReadModelRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.mediator = mediator;
            this.clock = clock;
        }

        public async Task<Guid> Handle(
            CreateCustomerPersonUserAccountCommand command, CancellationToken cancellationToken)
        {
            Guid? performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            PersonReadModel? person = this.GetPersonByIdOrEmail(command);
            var isNewCustomer = person == null;
            PersonAggregate? personAggregate = null;
            CustomerAggregate? customerAggregate = null;

            if (isNewCustomer)
            {
                personAggregate = await this.personService.CreateNewPerson(command.TenantId, command.PersonDetails);
                var personId = personAggregate.Id;
                customerAggregate = await this.customerService.CreateCustomerForExistingPerson(
                    personAggregate,
                    command.Environment,
                    performingUserId,
                    command.PortalId);
                person = PersonReadModel.CreateFromPersonData(
                    command.TenantId,
                    personId,
                    personAggregate.CustomerId,
                    personAggregate.UserId,
                    new PersonData(command.PersonDetails, personId),
                    this.clock.Now());
            }

            var personEmail = !string.IsNullOrEmpty(command.PersonDetails.Email)
                    ? person.Email
                    : person.EmailAddresses.FirstOrDefault()?.EmailAddress;
            PersonReadModel? relatedPersonWithUser = null;
            if (personEmail != null)
            {
                relatedPersonWithUser = this.personReadModelRepository.GetPersonAssociatedWithUserByEmailAndOrganisationId(
                       command.TenantId, command.OrganisationId, personEmail, command.Environment);
            }

            if (!person.UserId.HasValue && relatedPersonWithUser == null)
            {
                personAggregate ??= this.personAggregateRepository.GetById(command.TenantId, person.Id);
                var userAggregate = await this.userService.CreateUserForPerson(personAggregate, customerAggregate);
                person.UserId = userAggregate.Id;
            }

            var personWithUser = person.UserId.HasValue ? person : relatedPersonWithUser;
            if (personWithUser.UserHasBeenActivated)
            {
                await this.userActivationInvitationService.SendAccountAlreadyActivatedEmail(
                    command.TenantId, personWithUser.UserId.GetValueOrDefault(), command.Environment, command.PortalId);
            }
            else if (!string.IsNullOrEmpty(command.PersonDetails.Email) || command.PersonDetails.EmailAddresses.Any())
            {
                await this.userActivationInvitationService.CreateActivationInvitationAndQueueEmail(
                    command.TenantId, personWithUser.UserId.GetValueOrDefault(), command.Environment, command.PortalId);
            }

            if (!isNewCustomer)
            {
                var mergeCustomerCommand = new MergeCustomerIntoExistingInvitedOrActivatedCustomerCommand(
                    command.TenantId, command.Environment, person.Id);
                await this.mediator.Send(mergeCustomerCommand);
            }

            return person.Id;
        }

        private PersonReadModel? GetPersonByIdOrEmail(CreateCustomerPersonUserAccountCommand request)
        {
            PersonReadModel? person = null;

            if (request.PersonId.HasValue)
            {
                person = this.personReadModelRepository.GetPersonByIdAndOrganisationId(
                    request.TenantId, request.OrganisationId, request.PersonId.Value);
            }
            else if (!string.IsNullOrEmpty(request.PersonDetails.Email) || request.PersonDetails.EmailAddresses.Any())
            {
                var email = !string.IsNullOrEmpty(request.PersonDetails.Email)
                    ? request.PersonDetails.Email
                    : request.PersonDetails.EmailAddresses.FirstOrDefault()?.EmailAddressValueObject.ToString();

                var persons = this.personReadModelRepository.GetAllPersonsAssociatedWithCustomerByEmailAndOrganisationId(
                        request.TenantId, request.OrganisationId, email, request.Environment);
                person = persons.FirstOrDefault(p => p.UserHasBeenInvitedToActivate) ?? persons.FirstOrDefault();
            }

            return person;
        }
    }
}
