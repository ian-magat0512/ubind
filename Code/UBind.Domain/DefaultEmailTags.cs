// <copyright file="DefaultEmailTags.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// These are a set of tags which, where appropriate will be associated with emails by default when they are created. Users may be able to add further custom tags however these ones are added by the system automatically.
    /// </summary>
    public static class DefaultEmailTags
    {
        /// <summary>
        /// Gets the quote tag.
        /// </summary>
        public const string Quote = "Quote";

        /// <summary>
        /// Gets the policy tag.
        /// </summary>
        public const string Policy = "Policy";

        /// <summary>
        /// Gets the renewal tag.
        /// </summary>
        public const string Renewal = "Renewal";

        /// <summary>
        /// Gets the Admin tag.
        /// </summary>
        public const string Admin = "Admin";

        /// <summary>
        /// Gets the customer tag.
        /// </summary>
        public const string Customer = "Customer";

        /// <summary>
        /// Gets the user tag.
        /// </summary>
        public const string User = "User";

        /// <summary>
        /// Gets the invitation tag.
        /// </summary>
        public const string Invitation = "Invitation";

        /// <summary>
        /// Gets the Cancellation tag.
        /// </summary>
        public const string Cancellation = "Cancellation";

        /// <summary>
        /// Gets the PasswordReset tag.
        /// </summary>
        public const string PasswordReset = "Password Reset";

        /// <summary>
        /// Gets the Adjustment tag.
        /// </summary>
        public const string Adjustment = "Adjustment";

        /// <summary>
        /// Gets the AccountActivation tag.
        /// </summary>
        public const string AccountActivation = "Account Activation";

        /// <summary>
        /// Gets the Purchase tag.
        /// </summary>
        public const string Purchase = "Purchase";

        /// <summary>
        /// Gets the Quote Association Tag.
        /// </summary>
        public const string QuoteAssociation = "Quote Association";
    }
}
