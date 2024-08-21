// <copyright file="UserPictureControllerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers.RolesAndPermissions
{
    using UBind.Web.Controllers;

    /// <summary>
    /// Defines the <see cref="UserPictureControllerTest" />.
    /// </summary>
    public class UserPictureControllerTest
    {
        /// <summary>
        /// Defines the userPictureController.
        /// </summary>
        private PictureController userPictureController;

        public UserPictureControllerTest()
        {
            this.userPictureController = new PictureController(null, null);
        }
    }
}
