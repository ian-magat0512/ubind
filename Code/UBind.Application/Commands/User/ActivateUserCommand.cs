// <copyright file="ActivateUserCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Command for activating user by setting initial password.
    /// </summary>
    [CreateTransactionThatSavesChangesIfNoneExists]
    public class ActivateUserCommand : ICommand<UserReadModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivateUserCommand"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="userId">The user Id.</param>
        /// <param name="performingUserId">The performing user Id.</param>
        /// <param name="cleartextPassword">The clear text password.</param>
        public ActivateUserCommand(
            Guid tenantId,
            Guid userId,
            Guid? performingUserId,
            string cleartextPassword)
        {
            this.TenantId = tenantId;
            this.UserId = userId;
            this.PerformingUserId = performingUserId;
            this.ClearTextPassword = cleartextPassword;
        }

        /// <summary>
        /// Gets the tenantId.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the user Id.
        /// </summary>
        public Guid UserId { get; }

        /// <summary>
        /// Gets the performing user Id.
        /// </summary>
        public Guid? PerformingUserId { get; }

        /// <summary>
        /// Gets the clear text password.
        /// </summary>
        public string ClearTextPassword { get; }
    }
}
