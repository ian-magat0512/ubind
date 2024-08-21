// <copyright file="IUserProfilePictureRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Entities
{
    using System;

    /// <summary>
    /// Repository for user profile pictures.
    /// </summary>
    public interface IUserProfilePictureRepository
    {
        /// <summary>
        /// Get the user profile picture for a given user.
        /// </summary>
        /// <param name="profilePicId">The profile picture id of the user.</param>
        /// <returns>The user profile picture for that user, if found, otherwise null.</returns>
        UserProfilePicture GetById(Guid profilePicId);

        /// <summary>
        /// Add a new user profile picture to the repository.
        /// </summary>
        /// <param name="entity">The user profile picture.</param>
        void Add(UserProfilePicture entity);

        /// <summary>
        /// Deletes the provided profile picture from repository.
        /// </summary>
        /// <param name="profilePicture">The profile picture to be deleted.</param>
        void Delete(UserProfilePicture profilePicture);

        /// <summary>
        /// Save changes to the repository.
        /// </summary>
        void SaveChanges();
    }
}
