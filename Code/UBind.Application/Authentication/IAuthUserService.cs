// <copyright file="IAuthUserService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Authentication
{
    using System.Threading.Tasks;

    /// <summary>
    /// Authentication user service interface.
    /// </summary>
    /// <typeparam name="T">Generic class object for returning method requests.</typeparam>
    public interface IAuthUserService<T>
        where T : class
    {
        /// <summary>
        /// Gets the user object by email address.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<T> GetUserByEmailAsync(string email);

        /// <summary>
        /// Method that creates a new entry for the specified user.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="role">The user's role.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<T> CreateUser(string email, string role);

        /// <summary>
        /// Method that updates an existing user.
        /// </summary>
        /// <param name="userId">The ID of the user that is going to be updated.</param>
        /// <param name="email">The new email address.</param>
        /// <param name="blocked">A value indicating whether the user is blocked..</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<T> UpdateUser(string userId, string email, bool blocked);

        /// <summary>
        /// Method that updates an existing entry.
        /// </summary>
        /// <param name="userId">The ID of the user that is going to be updated.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<T> SetPassword(string userId, string password);
    }
}
