// <copyright file="UserUpdateModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.User
{
    using System;
    using UBind.Domain.Aggregates.Person;

    /// <summary>
    /// User update model.
    /// </summary>
    public class UserUpdateModel : PersonalDetails
    {
        /// <summary>
        /// Gets or sets picture of the user represented as data.
        /// </summary>
        public byte[] PictureData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is blocked or not.
        /// </summary>
        public bool Blocked { get; set; }

        /// <summary>
        /// Gets or sets the ID of the portal the user should log into.
        /// </summary>
        public Guid? PortalId { get; set; }

        public Guid? AuthenticationMethodId { get; set; }

        public string? ExternalUserId { get; set; }

        /// <summary>
        /// Gets or sets the IDs of roles the user should have.
        /// If this is left null, the user's roles will not be changed.
        /// </summary>
        public IEnumerable<Guid>? RoleIds { get; set; }
    }
}
