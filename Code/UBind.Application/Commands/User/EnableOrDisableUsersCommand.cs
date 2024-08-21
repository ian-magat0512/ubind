// <copyright file="EnableOrDisableUsersCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.User
{
    using System.Collections.Generic;
    using UBind.Application.User;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for enabling or disabling a list of users
    /// </summary>
    [CreateTransactionThatSavesChangesIfNoneExists]
    public class EnableOrDisableUsersCommand : ICommand<List<UserModel>>
    {
        public EnableOrDisableUsersCommand(
            IEnumerable<UserModel> users, bool blocked)
        {
            this.Blocked = blocked;
            this.Users = users;
        }

        public IEnumerable<UserModel> Users { get; }

        /// <summary>
        /// Gets a value indicating whether the users are to be disabled.
        /// </summary>
        public bool Blocked { get; }
    }
}
