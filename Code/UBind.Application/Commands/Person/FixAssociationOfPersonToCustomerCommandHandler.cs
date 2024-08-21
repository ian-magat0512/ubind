// <copyright file="FixAssociationOfPersonToCustomerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Person
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Command handler for fixing association of existing person aggregates to customer counterparts.
    /// </summary>
    public class FixAssociationOfPersonToCustomerCommandHandler
        : ICommandHandler<FixAssociationOfPersonToCustomerCommand, Unit>
    {
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly ILogger<FixAssociationOfPersonToCustomerCommandHandler> logger;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly ICachingResolver tenantResolver;
        private readonly IClock clock;

        public FixAssociationOfPersonToCustomerCommandHandler(
            IPersonAggregateRepository personAggregateRepo,
            IUserAggregateRepository userAggregateRepo,
            IPersonReadModelRepository personReadModelRepo,
            ICachingResolver tenantResolver,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ILogger<FixAssociationOfPersonToCustomerCommandHandler> logger,
            IClock clock)
        {
            this.personAggregateRepository = personAggregateRepo;
            this.userAggregateRepository = userAggregateRepo;
            this.personReadModelRepository = personReadModelRepo;
            this.tenantResolver = tenantResolver;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.logger = logger;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(FixAssociationOfPersonToCustomerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId.GetValueOrDefault();
            var tenant = await this.tenantResolver.GetTenantOrNull(request.TenantId);

            if (tenant == null)
            {
                return Unit.Value;
            }

            this.logger.LogInformation($"Starting Migration for fixing person data for tenant {tenant.Details.Alias} : {request.TenantId}");
            foreach (var personId in request.PersonIds)
            {
                var person = this.personReadModelRepository.GetPersonById(request.TenantId, personId);
                if (person == null || !person.CustomerId.HasValue)
                {
                    continue;
                }

                var personAggregate = this.personAggregateRepository.GetById(request.TenantId, person.Id);
                try
                {
                    if (personAggregate.CustomerId.HasValue)
                    {
                        // if aggregate has customer id, no need to update.
                        continue;
                    }

                    personAggregate.AssociateWithCustomer(person.CustomerId.Value, performingUserId, this.clock.GetCurrentInstant());
                    await this.personAggregateRepository.Save(personAggregate);
                    await Task.Delay(300, cancellationToken);

                    // if a user account has been created, update user type to ensure its customer type.
                    if (person.UserHasBeenInvitedToActivate)
                    {
                        var userAggregate = this.userAggregateRepository.GetById(request.TenantId, person.UserId.Value);
                        if (userAggregate == null)
                        {
                            // this should not happen. Log warning.
                            this.logger.LogWarning($"Person {personId} has been invited but associated user with Id:{person.UserId.Value} does not exist. Check.");
                            continue;
                        }

                        if (userAggregate.UserType != UserType.Customer.ToString())
                        {
                            userAggregate.ChangeUserType(UserType.Customer, performingUserId, this.clock.GetCurrentInstant());
                            await this.userAggregateRepository.Save(userAggregate);
                            await Task.Delay(300, cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogInformation($"Error for person: {person.Id}. Message: {ex.Message}");
                    throw;
                }
            }

            return Unit.Value;
        }
    }
}
