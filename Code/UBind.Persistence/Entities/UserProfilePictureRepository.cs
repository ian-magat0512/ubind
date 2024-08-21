// <copyright file="UserProfilePictureRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Entities
{
    using System;
    using System.Linq;
    using UBind.Domain.Entities;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class UserProfilePictureRepository : IUserProfilePictureRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfilePictureRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The uBind database context.</param>
        public UserProfilePictureRepository(IUBindDbContext dbContext) => this.dbContext = dbContext;

        /// <inheritdoc/>
        public void Add(UserProfilePicture entity)
        {
            this.dbContext.UserProfilePictures.Add(entity);
        }

        /// <inheritdoc/>
        public UserProfilePicture GetById(Guid profilePicId)
        {
            return this.dbContext.UserProfilePictures.SingleOrDefault(pic => pic.Id == profilePicId);
        }

        /// <inheritdoc/>
        public void Delete(UserProfilePicture entity)
        {
            this.dbContext.UserProfilePictures.Remove(entity);
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }
    }
}
