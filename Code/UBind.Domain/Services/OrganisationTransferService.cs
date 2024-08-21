// <copyright file="OrganisationTransferService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;
    using Hangfire.Server;
    using Microsoft.Extensions.Logging;
    using MoreLinq;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Loggers;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Repositories;

    public class OrganisationTransferService : IOrganisationTransferService
    {
        private readonly IClock clock;

        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IClaimReadModelRepository claimReadModelRepository;
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IEmailRepository emailRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IOrganisationService organisationService;
        private readonly IProgressLoggerFactory progressLoggerFactory;

        public OrganisationTransferService(
            IUserReadModelRepository userReadModelRepository,
            IPersonAggregateRepository personAggregateRepository,
            IPersonReadModelRepository personReadModelRepository,
            IUserAggregateRepository userAggregateRepository,
            ICustomerReadModelRepository customerReadModelRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            IClaimReadModelRepository claimReadModelRepository,
            IClaimAggregateRepository claimAggregateRepository,
            IQuoteReadModelRepository quoteReadModelRepository,
            IQuoteAggregateRepository quoteAggregateRepository,
            IEmailRepository emailRepository,
            IRoleRepository roleRepository,
            IOrganisationService organisationService,
            IProgressLoggerFactory progressLoggerFactory,
            IClock clock)
        {
            this.userReadModelRepository = userReadModelRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.personReadModelRepository = personReadModelRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.customerReadModelRepository = customerReadModelRepository;
            this.customerAggregateRepository = customerAggregateRepository;
            this.claimReadModelRepository = claimReadModelRepository;
            this.claimAggregateRepository = claimAggregateRepository;
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.emailRepository = emailRepository;
            this.roleRepository = roleRepository;
            this.organisationService = organisationService;
            this.progressLoggerFactory = progressLoggerFactory;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task ValidateUserDataForTransfer(Guid tenantId, Guid userId, Guid destinationOrganisationId)
        {
            var user = this.userReadModelRepository.GetUser(tenantId, userId);

            // Make sure the user does exist in the record
            this.ThrowIfUserNotFound(user, userId);

            // Make sure that you are moving the user to a different organisation
            this.ThrowIfUserBelongsToTheSameOrganisation(userId, user.OrganisationId, destinationOrganisationId);

            // Make sure that the old organisation must still have an existing client administrator upon transfer
            await this.ThrowIfSingleUserToAdministrateItsOwnOrganisation(tenantId, userId, user.OrganisationId);

            // Make sure that the user doesn't have any duplicate emails upon transfer
            this.ThrowIfUserHasDuplicateCustomerEmailsInTheDestinationOrganisation(tenantId, userId, user.OrganisationId, destinationOrganisationId);
        }

        /// <inheritdoc/>
        public void ValidateCustomerDataForTransfer(Guid tenantId, Guid customerId, Guid destinationOrganisationId)
        {
            var customer = this.customerReadModelRepository.GetCustomerById(tenantId, customerId);

            // Make sure the customer does exist in the record
            this.ThrowIfCustomerNotFound(customer, customerId);

            // Make sure that you are moving the customer to a different organisation
            this.ThrowIfCustomerBelongsToTheSameOrganisation(customerId, customer.OrganisationId, destinationOrganisationId);

            // Make sure that the customer doesn't have any duplicate emails upon transfer
            this.ThrowIfTransferringCustomerWouldResultInTwoUserAccountsWithTheSameEmailAddressInTheOrganisation(tenantId, customerId, destinationOrganisationId);
        }

        /// <inheritdoc/>
        public async Task TransferCustomerToAnotherOrganisation(Guid tenantId, Guid customerId, Guid destinationOrganisationId, Guid? performingUserId, PerformContext performContext)
        {
            var customer = this.customerReadModelRepository.GetCustomerById(tenantId, customerId);

            using (var transaction = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    // Transfers claims, quotes, login and emails, and the customer aggregate itself
                    await this.TransferCustomerRelatedEntities(customer, destinationOrganisationId, performingUserId);
                    transaction.Complete();
                }
                catch (TransactionException ex)
                {
                    var progressLogger = this.progressLoggerFactory.Invoke(performContext);
                    progressLogger?.Log(LogLevel.Error, ex.Message);
                }
            }
        }

        public async Task TransferUserAggregateIncludingLoginAndEmailsToAnotherOrganisation(Guid tenantId, Guid userId, Guid currentOrganisationId, Guid destinationOrganisationId, Guid? performingUserId)
        {
            var userAggregate = this.userAggregateRepository.GetById(tenantId, userId);
            var currentInstant = this.clock.GetCurrentInstant();
            var sourceIsDefaultOrganisation = await this.organisationService.IsOrganisationDefaultForTenant(userAggregate.TenantId, currentOrganisationId);
            userAggregate.TransferToAnotherOrganisation(userAggregate.TenantId, currentOrganisationId, destinationOrganisationId, sourceIsDefaultOrganisation, performingUserId, currentInstant);
            await this.userAggregateRepository.Save(userAggregate);
            await Task.Delay(100);

            var personAggregate = this.personAggregateRepository.GetById(userAggregate.TenantId, userAggregate.PersonId);
            personAggregate.TransferToAnotherOrganisation(userAggregate.TenantId, destinationOrganisationId, performingUserId, currentInstant);
            await this.personAggregateRepository.Save(personAggregate);
            await Task.Delay(100);

            var filter = new EntityListFilters
            {
                TenantId = userAggregate.TenantId,
                OrganisationIds = new Guid[] { currentOrganisationId },
                OwnerUserId = userAggregate.Id,
            };
            var emails = this.emailRepository.GetAll(userAggregate.TenantId, filter).ToList();
            foreach (var email in emails)
            {
                email.SetOrganisationId(destinationOrganisationId);
            }

            this.emailRepository.SaveChanges();
            await Task.Delay(100);
        }

        private async Task TransferCustomerRelatedEntities(ICustomerReadModelSummary customer, Guid destinationOrganisationId, Guid? performingUserId)
        {
            await this.TransferClaimsToAnotherOrganisation(customer.TenantId, customer.OrganisationId, customer.Id, destinationOrganisationId, performingUserId);
            await this.TransferQuotesToAnotherOrganisation(customer.TenantId, customer.OrganisationId, customer.Id, destinationOrganisationId, performingUserId);
            await this.TransferCustomerAggregateToAnotherOrganisation(customer, destinationOrganisationId, performingUserId);
        }

        private async Task TransferCustomerAggregateToAnotherOrganisation(ICustomerReadModelSummary customer, Guid destinationOrganisationId, Guid? performingUserId)
        {
            var currentInstant = this.clock.GetCurrentInstant();
            var customerAggregate = this.customerAggregateRepository.GetById(customer.TenantId, customer.Id);

            // Get the source organisation Id before transferring to another organisation
            var sourceOrganisationId = customer.OrganisationId;

            // Transfer organisation for customer aggregate
            customerAggregate.TransferToAnotherOrganisation(destinationOrganisationId, performingUserId, currentInstant);
            await this.customerAggregateRepository.Save(customerAggregate);
            await Task.Delay(100);

            // Transfer organisation for person aggregate
            var personAggregate = this.personAggregateRepository.GetById(customer.TenantId, customer.PrimaryPersonId);
            personAggregate.TransferToAnotherOrganisation(customer.TenantId, destinationOrganisationId, performingUserId, currentInstant);
            await this.personAggregateRepository.Save(personAggregate);
            await Task.Delay(100);

            // Getting the emails for customer
            var filter = new EntityListFilters
            {
                TenantId = customer.TenantId,
                OrganisationIds = new Guid[] { sourceOrganisationId },
                CustomerId = customer.Id,
            };
            var emails = this.emailRepository.GetEmailsForCustomer(customer.TenantId, customer.Id, filter).ToList();
            foreach (var email in emails)
            {
                email.SetOrganisationId(destinationOrganisationId);
            }

            this.emailRepository.SaveChanges();
            await Task.Delay(100);

            // If it happens that the customer has a user, then change it too
            if (personAggregate.UserId != null && personAggregate.UserId != Guid.Empty)
            {
                var userAggregate = this.userAggregateRepository.GetById(personAggregate.TenantId, personAggregate.UserId.Value);
                var sourceIsDefaultOrganisation = await this.organisationService.IsOrganisationDefaultForTenant(userAggregate.TenantId, sourceOrganisationId);
                userAggregate.TransferToAnotherOrganisation(userAggregate.TenantId, sourceOrganisationId, destinationOrganisationId, sourceIsDefaultOrganisation, performingUserId, currentInstant);
                await this.userAggregateRepository.Save(userAggregate);
                await Task.Delay(100);

                var userFilter = new EntityListFilters
                {
                    TenantId = userAggregate.TenantId,
                    OrganisationIds = new Guid[] { sourceOrganisationId },
                    OwnerUserId = userAggregate.Id,
                };
                var userEmails = this.emailRepository.GetAll(userAggregate.TenantId, userFilter).ToList();
                foreach (var email in userEmails)
                {
                    email.SetOrganisationId(destinationOrganisationId);
                }

                this.emailRepository.SaveChanges();
                await Task.Delay(100);
            }
        }

        private async Task TransferClaimsToAnotherOrganisation(Guid customerTenantId, Guid customerOrganisationId, Guid customerId, Guid destinationOrganisationId, Guid? performingUserId)
        {
            var currentInstant = this.clock.GetCurrentInstant();
            var claimsToBeTransferred = this.claimReadModelRepository.GetClaimsForCustomerId(customerTenantId, customerOrganisationId, customerId).Where(claim => claim.PersonId.HasValue).ToList();

            foreach (var claim in claimsToBeTransferred)
            {
                // Transfer organisation for claim aggregate
                var claimAggregate = this.claimAggregateRepository.GetById(claim.TenantId, claim.Id);
                claimAggregate.TransferToAnotherOrganisation(destinationOrganisationId, claim.PersonId.Value, performingUserId, currentInstant);
                await this.claimAggregateRepository.Save(claimAggregate);
                await Task.Delay(100);

                var filter = new EntityListFilters
                {
                    TenantId = claim.TenantId,
                    OrganisationIds = new Guid[] { customerOrganisationId },
                    ClaimId = claim.Id,
                };
                var emails = this.emailRepository.GetByClaimId(customerTenantId, claim.Id, filter).ToList();
                foreach (var email in emails)
                {
                    email.SetOrganisationId(destinationOrganisationId);
                }

                this.emailRepository.SaveChanges();
                await Task.Delay(100);
            }
        }

        private async Task TransferQuotesToAnotherOrganisation(Guid customerTenantId, Guid customerOrganisationId, Guid customerId, Guid destinationOrganisationId, Guid? performingUserId)
        {
            var quotesToBeTransferred = this.quoteReadModelRepository.GetQuotesForCustomerIdTenantIdAndOrganisationId(customerTenantId, customerOrganisationId, customerId).ToList();
            var currentInstant = this.clock.GetCurrentInstant();
            foreach (var quote in quotesToBeTransferred)
            {
                // Transfer organisation for quote aggregate
                var quoteAggregate = this.quoteAggregateRepository.GetById(quote.TenantId, quote.AggregateId);
                quoteAggregate.TransferToAnotherOrganisation(destinationOrganisationId, quote.Id, performingUserId, currentInstant);
                await this.quoteAggregateRepository.Save(quoteAggregate);
                await Task.Delay(100);

                var filter = new EntityListFilters
                {
                    TenantId = quote.TenantId,
                    OrganisationIds = new Guid[] { customerOrganisationId },
                    QuoteId = quote.Id,
                };
                var emails = this.emailRepository.GetByQuoteId(customerTenantId, quote.Id, filter).ToList();
                foreach (var email in emails)
                {
                    email.SetOrganisationId(destinationOrganisationId);
                }

                this.emailRepository.SaveChanges();
                await Task.Delay(100);
            }
        }

        private void ThrowIfUserNotFound(UserReadModel user, Guid userId)
        {
            if (user == null)
            {
                throw new ErrorException(Errors.User.NotFound(userId));
            }
        }

        private void ThrowIfCustomerNotFound(CustomerReadModelDetail customer, Guid customerId)
        {
            if (customer == null)
            {
                throw new ErrorException(Errors.User.NotFound(customerId));
            }
        }

        private void ThrowIfUserHasDuplicateCustomerEmailsInTheDestinationOrganisation(Guid tenantId, Guid userId, Guid sourceOrganisationId, Guid destinationOrganisationId)
        {
            // We only need to check the existing customer user accounts since the user will only be transferred within
            // the same tenancy.
            var customersToBeTransferred = this.customerReadModelRepository
                .GetCustomersMatchingFilter(tenantId, new EntityListFilters()
                {
                    OrganisationIds = new Guid[] { sourceOrganisationId },
                    OwnerUserId = userId,
                })
                .Select(c => c.PrimaryPerson.Email)
                .ToList();
            var toOrganisationCustomers = this.customerReadModelRepository
                .GetCustomersMatchingFilter(tenantId, new EntityListFilters()
                {
                    OrganisationIds = new Guid[] { destinationOrganisationId },
                })
                .Select(c => c.PrimaryPerson.Email)
                .ToList();

            foreach (var toOrganisationCustomer in toOrganisationCustomers)
            {
                if (customersToBeTransferred.Contains(toOrganisationCustomer))
                {
                    throw new ErrorException(Errors.Organisation.DuplicateCustomerInDestinationOrganisation(toOrganisationCustomer, destinationOrganisationId));
                }
            }
        }

        private void ThrowIfTransferringCustomerWouldResultInTwoUserAccountsWithTheSameEmailAddressInTheOrganisation(
            Guid tenantId, Guid customerId, Guid destinationOrganisationId)
        {
            // We only need to check the existing customer user accounts since the user will only be transferred within
            // the same tenancy.
            var sourceCustomer = this.customerReadModelRepository.GetCustomerById(tenantId, customerId);
            var sourcePersonsWhichHaveAUserAccount = this.personReadModelRepository.GetPersonsWhichHaveAUserAccountInOrganisationByCustomerId(
                tenantId, sourceCustomer.OrganisationId, sourceCustomer.Id, sourceCustomer.Environment);
            var listOfConflictingPersons = sourcePersonsWhichHaveAUserAccount.Select(person =>
                new
                {
                    SourcePerson = person,
                    DestinationPerson = this.personReadModelRepository.GetPersonWhoHasAUserAccountInOrganisationByEmail(
                        tenantId, destinationOrganisationId, person.Email, sourceCustomer.Environment),
                }).Where(persons => persons.DestinationPerson != null);

            if (listOfConflictingPersons.Any())
            {
                var listOfConflictingEmailAddresses = listOfConflictingPersons.Select(person => person.SourcePerson.Email);
                var conflictEmailAddresses = string.Empty;
                listOfConflictingEmailAddresses.ForEach(email =>
                {
                    conflictEmailAddresses = conflictEmailAddresses.IsNotNullOrEmpty()
                                                    ? $"{conflictEmailAddresses}, {email}"
                                                    : $"{email}";
                });
                var sourcePerson = listOfConflictingPersons.Select(person => person.SourcePerson).FirstOrDefault();
                var destinationPerson = listOfConflictingPersons.Select(person => person.DestinationPerson).FirstOrDefault();
                var destinationCustomer = this.customerReadModelRepository.GetCustomerById(tenantId, destinationPerson.CustomerId.Value);
                var errorData = new JObject()
                {
                    { "sourceCustomerId", sourceCustomer.Id.ToString() },
                    { "sourceOrganisationId", sourceCustomer.OrganisationId.ToString() },
                    { "destinationCustomerId", destinationCustomer.Id.ToString() },
                    { "destinationOrganisationId", destinationCustomer.OrganisationId.ToString() },
                    { "sourcePersonId", sourcePerson.Id.ToString() },
                    { "destinationPersonId", destinationPerson.Id.ToString() },
                    { "sourceCustomerName", sourceCustomer.FullName },
                    { "sourceCustomerOrganisationName", sourceCustomer.OrganisationName },
                    { "conflictingEmailAddress",  conflictEmailAddresses },
                    { "destinationCustomerName", destinationCustomer.FullName },
                    { "destinationOrganisationName", destinationCustomer.OrganisationName },
                    { "sourceCustomerConflictingPersonName", sourcePerson.FullName },
                    { "destinationPersonName", destinationPerson.FullName },
                };

                throw new ErrorException(
                    Errors.Organisation.DuplicateCustomerWithUserInBothOrganisations(
                        sourceCustomer.FullName,
                        sourceCustomer.OrganisationName,
                        listOfConflictingEmailAddresses.FirstOrDefault(),
                        destinationCustomer.FullName,
                        destinationCustomer.OrganisationName,
                        sourcePerson.FullName,
                        destinationPerson.FullName,
                        errorData));
            }
        }

        private async Task ThrowIfSingleUserToAdministrateItsOwnOrganisation(Guid tenantId, Guid userId, Guid sourceOrganisationId)
        {
            // The checking for the remaining tenant administrator only applies for default organisation.
            if (!(await this.organisationService.IsOrganisationDefaultForTenant(tenantId, sourceOrganisationId)))
            {
                return;
            }

            // Get all administrator roles from the database based from tenant ID
            var role = this.roleRepository.GetAdminRoleForTenant(tenantId);
            var hasAnotherUserToAdministrate = role?.Users
                .Where(u => u.TenantId == tenantId)
                .Where(u => u.OrganisationId == sourceOrganisationId)
                .Where(u => u.UserStatus == UserStatus.Active)
                .Where(u => u.Id != userId)
                .Any() ?? false;
            if (!hasAnotherUserToAdministrate)
            {
                throw new ErrorException(Errors.User.CannotTransferToAnotherOrganisation(userId, sourceOrganisationId));
            }
        }

        private void ThrowIfUserBelongsToTheSameOrganisation(Guid userId, Guid sourceOrganisationId, Guid destinationOrganisationId)
        {
            if (sourceOrganisationId == destinationOrganisationId)
            {
                throw new ErrorException(Errors.User.BelongsToTheSameOrganisation(userId, destinationOrganisationId));
            }
        }

        private void ThrowIfCustomerBelongsToTheSameOrganisation(Guid customerId, Guid sourceOrganisationId, Guid destinationOrganisationId)
        {
            if (sourceOrganisationId == destinationOrganisationId)
            {
                throw new ErrorException(Errors.Customer.BelongsToTheSameOrganisation(customerId, destinationOrganisationId));
            }
        }

        private TransactionScope CreateTransactionScope()
        {
            var transactionOptions = new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.FromMinutes(15) };
            return new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }
    }
}
