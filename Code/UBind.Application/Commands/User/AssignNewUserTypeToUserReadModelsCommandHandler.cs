// <copyright file="AssignNewUserTypeToUserReadModelsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Assign new user type to user read model ( from ClientAdmin/Agent User Type to Client,
    /// from UbindAdmin to Master)
    /// This migration is needed when implementing changes for UB-4685 refactor on older user records.
    /// </summary>
    public class AssignNewUserTypeToUserReadModelsCommandHandler
        : ICommandHandler<AssignNewUserTypeToUserReadModelsCommand, Unit>
    {
        private readonly ILogger<AssignNewUserTypeToUserReadModelsCommandHandler> logger;
        private readonly ITenantRepository tenantRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        /// <summary>
        /// user types to search for against user read models.
        /// </summary>
        private List<string> oldUserTypes = new List<string>()
            {
                "ClientAdmin",
                "Agent",
                "UBindAdmin",
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignNewUserTypeToUserReadModelsCommandHandler"/> class.
        /// </summary>
        /// <param name="tenantRepository">The repository for tenants.</param>
        /// <param name="userReadModelRepository">The repository for user read models.</param>
        /// <param name="userAggregateRepository">The repository for user aggregate.</param>
        /// <param name="httpContextPropertiesResolver">The resolver for performing user.</param>
        /// <param name="logger">The logger to idenfity issues.</param>
        /// <param name="clock">Represents the clock which can return the time as <see cref="Instant"/>.</param>
        public AssignNewUserTypeToUserReadModelsCommandHandler(
            ITenantRepository tenantRepository,
            IUserReadModelRepository userReadModelRepository,
            IUserAggregateRepository userAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ILogger<AssignNewUserTypeToUserReadModelsCommandHandler> logger,
            IClock clock)
        {
            this.logger = logger;
            this.tenantRepository = tenantRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(
            AssignNewUserTypeToUserReadModelsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tenants = this.tenantRepository.GetTenants();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;

            foreach (var tenant in tenants)
            {
                this.logger.LogInformation($"Starting Migration for Setting New UserType For Users of tenant {tenant.Id}.");

                var users = this.userReadModelRepository.GetAllUsersAsQueryable(tenant.Id)
                    .Where(x => this.oldUserTypes.Contains(x.UserType)).ToList();

                foreach (var user in users)
                {
                    async Task ApplyChange()
                    {
                        var userAggregate = this.userAggregateRepository.GetById(user.TenantId, user.Id);

                        UserType? newUserType = null;

                        if (user.UserType.ToLower() == "clientadmin" || user.UserType.ToLower() == "agent"
                            || userAggregate.UserType.ToLower() == "clientadmin" || userAggregate.UserType.ToLower() == "agent")
                        {
                            newUserType = UserType.Client;
                        }
                        else if (user.UserType.ToLower() == "ubindadmin"
                            || userAggregate.UserType.ToLower() == "ubindadmin")
                        {
                            newUserType = UserType.Master;
                        }

                        if (newUserType.HasValue)
                        {
                            this.logger.LogInformation($"Assigning UserType '{newUserType.Humanize()}' before ( '{user.UserType}' ) to user '{user.Id}'");
                            userAggregate.ChangeUserType(newUserType.Value, performingUserId, this.clock.GetCurrentInstant(), true);
                            await this.userAggregateRepository.Save(userAggregate);
                            await Task.Delay(100, cancellationToken);
                        }
                    }

                    await RetryPolicyHelper.ExecuteAsync<Exception>(() => ApplyChange());
                }
            }

            return Unit.Value;
        }
    }
}
