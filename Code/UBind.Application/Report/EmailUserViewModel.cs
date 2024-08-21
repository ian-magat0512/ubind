// <copyright file="EmailUserViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using DotLiquid;
    using UBind.Domain.ReadModel.Email;

    /// <summary>
    /// Email user view model for dot liquid template.
    /// </summary>
    public class EmailUserViewModel : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailUserViewModel"/> class.
        /// </summary>
        /// <param name="user">The user data.</param>
        public EmailUserViewModel(UserData user)
        {
            this.Type = user.UserType;
            this.PreferredName = user.PreferredName;
            this.FullName = user.FullName;
            this.Email = user.Email;
            this.AlternativeEmail = user.AlternativeEmail;
            this.MobilePhoneNumber = user.MobilePhoneNumber;
            this.WorkPhoneNumber = user.WorkPhoneNumber;
            this.HomePhoneNumber = user.HomePhoneNumber;
        }

        /// <summary>
        /// Gets the type of user.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the preffered name of the user.
        /// </summary>
        public string PreferredName { get; }

        /// <summary>
        /// Gets the fullname of the user.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Gets the email of the user.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Gets the alternative email of the user.
        /// </summary>
        public string AlternativeEmail { get; }

        /// <summary>
        /// Gets the mobile phone numberof the user.
        /// </summary>
        public string MobilePhoneNumber { get; }

        /// <summary>
        /// Gets the work phone number of the user.
        /// </summary>
        public string WorkPhoneNumber { get; }

        /// <summary>
        /// Gets the home phone number of the user.
        /// </summary>
        public string HomePhoneNumber { get; }
    }
}
