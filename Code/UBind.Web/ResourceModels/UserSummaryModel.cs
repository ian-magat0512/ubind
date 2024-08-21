// <copyright file="UserSummaryModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;

    /// <summary>
    /// Resource model for users.
    /// </summary>
    public class UserSummaryModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserSummaryModel"/> class form a read model.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <param name="fullName">The user's full name.</param>
        public UserSummaryModel(Guid id, string fullName)
        {
            this.Id = id;
            this.FullName = fullName;
        }

        /// <summary>
        /// Gets the unique identifier of the user.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the full name of the user.
        /// </summary>
        public string FullName { get; private set; }
    }
}
