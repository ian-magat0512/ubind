// <copyright file="IUserService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Domain.Entities;
    using UBind.Domain.ReadModel.User;

    public interface IUserService
    {
        /// <summary>
        /// Swaps the role for user.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="performingUserId">The Id of the performing user.</param>
        /// <param name="oldRole">The old role to change.</param>
        /// <param name="newRole">The new role to replace.</param>
        /// <param name="users">Enumerable of <see cref="UserReadModel"/> to apply for swap.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SwapRoleForUsers(
            Guid tenantId, Guid? performingUserId, Role oldRole, Role newRole, IEnumerable<UserReadModel> users);
    }
}
