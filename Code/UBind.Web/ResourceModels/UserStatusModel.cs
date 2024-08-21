// <copyright file="UserStatusModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using UBind.Application.User;

    /// <summary>
    /// Resource model for users.
    /// </summary>
    public class UserStatusModel
    {
        public UserStatusModel(UserModel userModel)
        {
            this.UserId = userModel.Id;
            this.Email = userModel.Email;
            this.TenantId = userModel.TenantId;
            this.Blocked = userModel.Blocked;
            this.FullName = userModel.FullName;
        }

        /// <summary>
        /// Gets the UserId of user.
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// Gets the FullName of user.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Gets the Email of user.
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Gets the tenant Id of the user.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether user is blocked.
        /// </summary>
        public bool Blocked { get; private set; }
    }
}
