// <copyright file="UpdateCustomerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;

    public class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand, Unit>
    {
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IClock clock;

        public UpdateCustomerCommandHandler(
            IPersonAggregateRepository personAggregateRepository,
            IUserAggregateRepository userAggregateRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.personAggregateRepository = personAggregateRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.customerAggregateRepository = customerAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        public async Task<Unit> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CustomerAggregate customerAggregate = this.customerAggregateRepository.GetById(request.TenantId, request.CustomerId);
            PersonAggregate personAggregate =
                this.personAggregateRepository.GetById(request.TenantId, customerAggregate.PrimaryPersonId);
            personAggregate.Update(
                request.PersonDetails, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            await this.personAggregateRepository.Save(personAggregate);

            if (personAggregate.UserId.HasValue)
            {
                await this.UpdateUserLogin(personAggregate.TenantId, personAggregate.UserId.Value, personAggregate.Email, personAggregate.FullName);
            }

            return await Task.FromResult(Unit.Value);
        }

        /// <summary>
        /// Updates user login.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="userId">customer userId.</param>
        /// <param name="newEmail"> new email.</param>
        /// <param name="fullName"> customer full name.</param>
        private async Task UpdateUserLogin(Guid tenantId, Guid userId, string newEmail, string fullName)
        {
            UserAggregate userAggregate = this.userAggregateRepository.GetById(tenantId, userId);

            if (string.IsNullOrEmpty(newEmail))
            {
                var additionalDetails = new List<string>
                {
                    $"Entity Type: Customer",
                    $"Entity Id: {userAggregate.CustomerId}",
                    $"Customer Fullname: {fullName}",
                    $"Login Email: {userAggregate.LoginEmail}",
                };
                throw new ErrorException(Errors.Customer.LoginEmailAddressShouldNotBeEmpty(fullName, additionalDetails));
            }

            if (userAggregate != null && userAggregate.LoginEmail != newEmail)
            {
                userAggregate.SetLoginEmail(
                    newEmail, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                await this.userAggregateRepository.Save(userAggregate);
            }
        }
    }
}
