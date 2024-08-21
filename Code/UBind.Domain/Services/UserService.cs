// <copyright file="UserService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.User;

    public class UserService : IUserService
    {
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IClock clock;

        public UserService(IUserAggregateRepository userAggregateRepository, IClock clock)
        {
            this.userAggregateRepository = userAggregateRepository;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task SwapRoleForUsers(
            Guid tenantId, Guid? performingUserId, Role oldRole, Role newRole, IEnumerable<UserReadModel> users)
        {
            var now = this.clock.GetCurrentInstant();
            foreach (var user in users)
            {
                var userAggregate = this.userAggregateRepository.GetById(tenantId, user.Id);

                if (!userAggregate.RoleIds.Contains(newRole.Id))
                {
                    userAggregate.AssignRole(newRole, performingUserId.Value, now);
                }

                if (userAggregate.RoleIds.Contains(oldRole.Id))
                {
                    userAggregate.RetractRole(oldRole, performingUserId.Value, now);
                }

                await this.userAggregateRepository.Save(userAggregate);
            }
        }
    }
}
