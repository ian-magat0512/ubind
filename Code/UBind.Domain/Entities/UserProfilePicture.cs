// <copyright file="UserProfilePicture.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Entities
{
    using System;
    using UBind.Domain.Extensions;

    /// <summary>
    /// For storing user profile pictures.
    /// </summary>
    /// <remarks>
    /// User profile pictures are handled outside of user aggregates because:
    ///  1) They are purely cosmetic and data synchronicity etc. is not important.
    ///  2) They are larger than other user data and could slow things down.
    /// .</remarks>
    public class UserProfilePicture
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfilePicture"/> class.
        /// </summary>
        /// <param name="pictureData">The picture data.</param>
        public UserProfilePicture(byte[] pictureData)
        {
            pictureData.ThrowIfArgumentNull(nameof(pictureData));
            if (pictureData.Length == 0)
            {
                throw new ArgumentException("pictureData must not be empty.");
            }

            this.Id = Guid.NewGuid();
            this.PictureData = pictureData;
        }

        // Parameterless constructor for EF.
        private UserProfilePicture()
        {
        }

        /// <summary>
        /// Gets the ID of the user the picture is for.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the picture data.
        /// </summary>
        public byte[] PictureData { get; private set; }

        /// <summary>
        /// Update the picture data.
        /// </summary>
        /// <param name="pictureData">The new picture data.</param>
        public void UpdatePicture(byte[] pictureData)
        {
            this.PictureData = pictureData;
        }
    }
}
