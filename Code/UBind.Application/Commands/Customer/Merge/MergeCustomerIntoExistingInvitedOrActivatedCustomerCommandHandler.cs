// <copyright file="MergeCustomerIntoExistingInvitedOrActivatedCustomerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer.Merge
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.Server;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.Repositories;

    /// <summary>
    /// The merge logic for merging customer records.
    /// </summary>
    public class MergeCustomerIntoExistingInvitedOrActivatedCustomerCommandHandler :
        MergeCustomerBaseCommandHandler<MergeCustomerIntoExistingInvitedOrActivatedCustomerCommand>,
        ICommandHandler<MergeCustomerIntoExistingInvitedOrActivatedCustomerCommand, Unit>
    {
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly IUserService userService;
        private readonly IBackgroundJobClient jobClient;
        private readonly IUserAggregateRepository userAggregateRepository;

        public MergeCustomerIntoExistingInvitedOrActivatedCustomerCommandHandler(
            IClaimAggregateRepository claimAggregateRepository,
            IClaimReadModelRepository claimReadModelRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            ICustomerReadModelRepository customerReadModelRepository,
            IEmailRepository emailRepository,
            IPersonAggregateRepository personAggregateRepository,
            IPersonReadModelRepository personReadModelRepository,
            IUserService userService,
            IQuoteAggregateRepository quoteAggregateRepository,
            IQuoteReadModelRepository quoteReadModelRepository,
            ISmsRepository smsRepository,
            IHttpContextPropertiesResolver propertiesResolver,
            ICqrsMediator mediator,
            IBackgroundJobClient jobClient,
            ILogger<MergeCustomerIntoExistingInvitedOrActivatedCustomerCommandHandler> logger,
            IClock clock,
            IUserAggregateRepository userAggregateRepository)
            : base(
                claimAggregateRepository,
                claimReadModelRepository,
                emailRepository,
                personReadModelRepository,
                quoteAggregateRepository,
                quoteReadModelRepository,
                smsRepository,
                customerAggregateRepository,
                personAggregateRepository,
                propertiesResolver,
                clock,
                mediator,
                logger)
        {
            this.customerReadModelRepository = customerReadModelRepository;
            this.userService = userService;
            this.jobClient = jobClient;
            this.userAggregateRepository = userAggregateRepository;
        }

        public override Task<Unit> Handle(MergeCustomerIntoExistingInvitedOrActivatedCustomerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var destinationPersonAggregate = this.PersonAggregateRepository.GetById(request.TenantId, request.DestinationPersonId);
            var destinationCustomerAggregate = this.CustomerAggregateRepository.GetById(request.TenantId, destinationPersonAggregate.CustomerId.Value);

            Guid destinationPersonId = destinationPersonAggregate.Id;
            Guid destinationCustomerId = destinationCustomerAggregate.Id;

            //// Identify other customer person with the same contact email.
            var relatedCustomerPersons = this.PersonReadModelRepository
                .GetAllPersonsAssociatedWithCustomerByEmailAndOrganisationId(
                    destinationPersonAggregate.TenantId, destinationPersonAggregate.OrganisationId, destinationPersonAggregate.Email, request.Environment)
                .Where(p => p.Id != destinationPersonAggregate.Id)
                .ToList();

            var relatedCustomerPersonWithUser = relatedCustomerPersons.FirstOrDefault(p => p.UserId != null);
            var relatedCustomerPersonsSourceIds = relatedCustomerPersons
                .Where(p => p.UserId == null)
                .Select(x => new SourceIds(x.CustomerId.Value, x.Id)).ToList();

            if (relatedCustomerPersonWithUser != null && relatedCustomerPersonWithUser.CustomerId.HasValue)
            {
                // If related customer has a user, switch the destination person to the related customer.
                relatedCustomerPersonsSourceIds.Add(new SourceIds(destinationCustomerId, destinationPersonId));
                destinationPersonId = relatedCustomerPersonWithUser.Id;
                destinationCustomerId = relatedCustomerPersonWithUser.CustomerId.Value;
            }

            if (relatedCustomerPersonsSourceIds.Any())
            {
                var sourceCustomersList = JsonConvert.SerializeObject(relatedCustomerPersonsSourceIds);
                this.jobClient.Enqueue<MergeCustomerIntoExistingInvitedOrActivatedCustomerCommandHandler>(
                    handler => handler.ProcessCustomerMerging(request.TenantId, destinationPersonId, destinationCustomerId, sourceCustomersList, sourceCustomersList.Count(), null));
            }

            return Task.FromResult(Unit.Value);
        }

        [JobDisplayName("Merging records to Person {0}, Customers to Merge Count {4}")]
        public async Task ProcessCustomerMerging(Guid tenantId, Guid destinationPersonId, Guid destinationCustomerId, string sourceIds, int batchLeft, PerformContext context)
        {
            var destinationPersonAggregate = this.PersonAggregateRepository.GetById(tenantId, destinationPersonId);
            var sourceCustomers = JsonConvert.DeserializeObject<List<SourceIds>>(sourceIds);
            var source = sourceCustomers.FirstOrDefault();
            var sourceCustomerHasUser = false;
            if (source.CustomerId != default)
            {
                var sourceCustomer = this.customerReadModelRepository.GetCustomerById(tenantId, source.CustomerId);
                DestinationPersonMergingModel destinationPersonParam =
                    new DestinationPersonMergingModel(destinationPersonAggregate);
                if (sourceCustomer != null && sourceCustomer.Id != destinationCustomerId)
                {
                    await this.MergeRelatedRecords(tenantId, source.CustomerId, destinationPersonParam);

                    var sourcePerson = this.PersonReadModelRepository.GetPersonById(tenantId, sourceCustomer.PrimaryPersonId);
                    var sourcePersonAggregate = this.PersonAggregateRepository.GetById(tenantId, sourceCustomer.PrimaryPersonId);
                    if (sourcePerson != null && sourcePerson.FirstName == destinationPersonAggregate.FirstName)
                    {
                        var sourcePersonalDetails = new PersonalDetails(sourcePersonAggregate);
                        await this.MergePersonDetails(tenantId, sourcePersonalDetails, destinationPersonParam);
                        sourcePersonAggregate.MarkAsDeleted(this.ContextPropertiesResolver.PerformingUserId, this.Clock.Now());
                        await this.PersonAggregateRepository.Save(sourcePersonAggregate);
                        if (sourcePerson.UserId.HasValue)
                        {
                            // Todo: Use userService.Delete
                            var userAggregate = this.userAggregateRepository.GetById(tenantId, sourcePerson.UserId.Value);
                            if (userAggregate != null)
                            {
                                userAggregate.SoftDelete(this.ContextPropertiesResolver.PerformingUserId, this.Clock.Now());
                                await this.userAggregateRepository.Save(userAggregate);
                            }
                        }
                    }

                    await this.AssignAllPersonsFromSourceToDestinationCustomer(tenantId, source.CustomerId, destinationPersonParam);
                    await this.DeleteCustomer(tenantId, sourceCustomer.Id);
                }
            }

            sourceCustomers = sourceCustomers.Skip(1).ToList();
            if (sourceCustomers.Any())
            {
                sourceIds = JsonConvert.SerializeObject(sourceCustomers);
                this.jobClient.ContinueJobWith<MergeCustomerIntoExistingInvitedOrActivatedCustomerCommandHandler>(
                    context.BackgroundJob.Id,
                    handler => handler.ProcessCustomerMerging(tenantId, destinationPersonId, destinationCustomerId, sourceIds, sourceCustomers.Count(), null));
            }
        }

        private async Task MergePersonDetails(Guid tenantId, PersonalDetails sourcePersonDetails, DestinationPersonMergingModel destinationParam)
        {
            this.Logger.LogInformation("Merging person details..");
            var destinationPersonDetails = destinationParam.PersonalDetails;
            if (string.IsNullOrEmpty(destinationPersonDetails.FirstName) && !string.IsNullOrEmpty(sourcePersonDetails.FirstName))
            {
                destinationPersonDetails.FirstName = sourcePersonDetails.FirstName;
            }

            if (string.IsNullOrEmpty(destinationPersonDetails.LastName) && !string.IsNullOrEmpty(sourcePersonDetails.LastName))
            {
                destinationPersonDetails.LastName = sourcePersonDetails.LastName;
            }

            if (string.IsNullOrEmpty(destinationPersonDetails.MiddleNames) && !string.IsNullOrEmpty(sourcePersonDetails.MiddleNames))
            {
                destinationPersonDetails.MiddleNames = sourcePersonDetails.MiddleNames;
            }

            if (string.IsNullOrEmpty(destinationPersonDetails.FullName) && !string.IsNullOrEmpty(sourcePersonDetails.FullName))
            {
                destinationPersonDetails.FullName = sourcePersonDetails.FullName;
            }

            if (string.IsNullOrEmpty(destinationPersonDetails.PreferredName) && !string.IsNullOrEmpty(sourcePersonDetails.PreferredName))
            {
                destinationPersonDetails.PreferredName = sourcePersonDetails.PreferredName;
            }

            if (string.IsNullOrEmpty(destinationPersonDetails.NamePrefix) && !string.IsNullOrEmpty(sourcePersonDetails.NamePrefix))
            {
                destinationPersonDetails.NamePrefix = sourcePersonDetails.NamePrefix;
            }

            if (string.IsNullOrEmpty(destinationPersonDetails.NameSuffix) && !string.IsNullOrEmpty(sourcePersonDetails.NameSuffix))
            {
                destinationPersonDetails.NameSuffix = sourcePersonDetails.NameSuffix;
            }

            if (string.IsNullOrEmpty(destinationPersonDetails.Company) && !string.IsNullOrEmpty(sourcePersonDetails.Company))
            {
                destinationPersonDetails.Company = sourcePersonDetails.Company;
            }

            if (string.IsNullOrEmpty(destinationPersonDetails.Title) && !string.IsNullOrEmpty(sourcePersonDetails.Title))
            {
                destinationPersonDetails.Title = sourcePersonDetails.Title;
            }

            if (string.IsNullOrEmpty(destinationPersonDetails.Email) && !string.IsNullOrEmpty(sourcePersonDetails.Email))
            {
                destinationPersonDetails.Email = sourcePersonDetails.Email;
            }

            if (sourcePersonDetails.EmailAddresses?.Any() ?? false)
            {
                var destinationEmailAddresses = destinationPersonDetails.EmailAddresses.ToList();
                foreach (var sourceEmailAddress in sourcePersonDetails.EmailAddresses)
                {
                    int index = destinationEmailAddresses.FindIndex(ea => ea.Label == sourceEmailAddress.Label);

                    if (index != -1)
                    {
                        destinationEmailAddresses[index] = sourceEmailAddress;
                    }
                    else
                    {
                        destinationEmailAddresses.Add(sourceEmailAddress);
                    }
                }

                destinationPersonDetails.EmailAddresses = destinationEmailAddresses;
            }

            if (sourcePersonDetails.StreetAddresses?.Any() ?? false)
            {
                var destinationStreetAddresses = destinationPersonDetails.StreetAddresses.ToList();
                foreach (var sourceStreetAddress in sourcePersonDetails.StreetAddresses)
                {
                    int index = destinationStreetAddresses.FindIndex(sa => sa.Label == sourceStreetAddress.Label);

                    if (index != -1)
                    {
                        destinationStreetAddresses[index] = sourceStreetAddress;
                    }
                    else
                    {
                        destinationStreetAddresses.Add(sourceStreetAddress);
                    }
                }

                destinationPersonDetails.StreetAddresses = destinationStreetAddresses;
            }

            if (sourcePersonDetails.PhoneNumbers?.Any() ?? false)
            {
                var destinationPhoneNumberList = destinationPersonDetails.PhoneNumbers.ToList();

                // We assume that the application quote could only provide one of each phone types
                foreach (var sourcePhoneNumber in sourcePersonDetails.PhoneNumbers)
                {
                    int index = destinationPhoneNumberList.FindIndex(phone => phone.Label == sourcePhoneNumber.Label);

                    if (index != -1)
                    {
                        destinationPhoneNumberList[index] = sourcePhoneNumber;
                    }
                    else
                    {
                        destinationPhoneNumberList.Add(sourcePhoneNumber);
                    }

                    if (destinationPersonDetails.WorkPhone == null && sourcePhoneNumber.Label?.ToLower() == "work")
                    {
                        destinationPersonDetails.WorkPhone = sourcePhoneNumber.PhoneNumberValueObject.ToString();
                    }
                    else if (destinationPersonDetails.HomePhone == null && sourcePhoneNumber.Label?.ToLower() == "home")
                    {
                        destinationPersonDetails.HomePhone = sourcePhoneNumber.PhoneNumberValueObject.ToString();
                    }
                    else if (destinationPersonDetails.MobilePhone == null && sourcePhoneNumber.Label?.ToLower() == "mobile")
                    {
                        destinationPersonDetails.MobilePhone = sourcePhoneNumber.PhoneNumberValueObject.ToString();
                    }
                }

                destinationPersonDetails.PhoneNumbers = destinationPhoneNumberList;
            }

            if (sourcePersonDetails.MessengerIds?.Any() ?? false)
            {
                var destinationMessengerIds = destinationPersonDetails.MessengerIds.ToList();
                foreach (var sourceMessengerId in sourcePersonDetails.MessengerIds)
                {
                    int index = destinationMessengerIds.FindIndex(m => m.Label == sourceMessengerId.Label);

                    if (index != -1)
                    {
                        destinationMessengerIds[index] = sourceMessengerId;
                    }
                    else
                    {
                        destinationMessengerIds.Add(sourceMessengerId);
                    }
                }

                destinationPersonDetails.MessengerIds = destinationMessengerIds;
            }

            if (sourcePersonDetails.WebsiteAddresses?.Any() ?? false)
            {
                var destinationWebsiteAddresses = destinationPersonDetails.WebsiteAddresses.ToList();
                foreach (var sourceWebsiteAddress in sourcePersonDetails.WebsiteAddresses)
                {
                    int index = destinationWebsiteAddresses.FindIndex(wa => wa.Label == sourceWebsiteAddress.Label);

                    if (index != -1)
                    {
                        destinationWebsiteAddresses[index] = sourceWebsiteAddress;
                    }
                    else
                    {
                        destinationWebsiteAddresses.Add(sourceWebsiteAddress);
                    }
                }

                destinationPersonDetails.WebsiteAddresses = destinationWebsiteAddresses;
            }

            if (sourcePersonDetails.SocialMediaIds?.Any() ?? false)
            {
                var destinationSocialMediaIds = destinationPersonDetails.SocialMediaIds.ToList();
                foreach (var sourceSocialMediaId in sourcePersonDetails.SocialMediaIds)
                {
                    int index = destinationSocialMediaIds.FindIndex(sm => sm.Label == sourceSocialMediaId.Label);

                    if (index != -1)
                    {
                        destinationSocialMediaIds[index] = sourceSocialMediaId;
                    }
                    else
                    {
                        destinationSocialMediaIds.Add(sourceSocialMediaId);
                    }
                }

                destinationPersonDetails.SocialMediaIds = destinationSocialMediaIds;
            }

            async Task UpdatePersonAggregate()
            {
                var destinationPersonAggregate = this.PersonAggregateRepository.GetById(tenantId, destinationParam.PersonId);
                destinationPersonAggregate.Update(destinationPersonDetails, this.ContextPropertiesResolver.PerformingUserId, this.Clock.Now());
                await this.PersonAggregateRepository.Save(destinationPersonAggregate);
            }

            await ConcurrencyPolicy.ExecuteWithRetriesAsync(UpdatePersonAggregate);
        }

        internal class SourceIds
        {
            public SourceIds(Guid customerId, Guid personId)
            {
                this.CustomerId = customerId;
                this.PersonId = personId;
            }

            [JsonProperty]
            public Guid CustomerId { get; set; }

            [JsonProperty]
            public Guid PersonId { get; set; }
        }
    }
}
