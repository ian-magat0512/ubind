// <copyright file="ValidationExpressions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Validation
{
    /// <summary>
    /// Validation expressions for use throughout the application.
    /// </summary>
    public static class ValidationExpressions
    {
        /// <summary>
        /// Regular expression for validating alias in general.
        /// </summary>
        public const string Alias = @"^(?!.*\bnull\b)(?=.*[a-z])[a-z0-9][a-z]*(?:-?[a-z0-9]+)*$";

        /// <summary>
        /// Regular expression for validating a strong password.
        /// </summary>
        public const string StrongPassword = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z0-9]).{12,}$";

        /// <summary>
        /// Regular expression for validating name.
        /// </summary>
        public const string Name = "^([a-zA-Z]+[-'. ,]*)+$";

        /// <summary>
        /// Regular expression for validating a name of an entity instance.
        /// </summary>
        public const string EntityName = @"(^(?!.*\bnull\b)(?=.*[a-zA-Z])[a-zA-Z0-9](?:-?[a-zA-Z0-9. ,']+)*$)";

        /// <summary>
        /// Regular expression for validating name.
        /// </summary>
        public const string AustralianMobileNumber = "^\\s*(0|\\+\\s*6\\s*1)\\s*(4|5)(\\s*\\d){8}\\s*$";

        /// <summary>
        /// Regular expression for validating name.
        /// </summary>
        public const string AustralianPhoneNumber = "^\\s*(0|\\+\\s*6\\s*1)(\\s*\\d){9}\\s*$";

        /// <summary>
        /// Regular expression for validating stylesheet urls.
        /// </summary>
        public const string StylesheetUrl = @"^(https:\/\/|http:\/\/)(((\*|[\w\d]+(-[\w\d]+)*)\.)*([-\w\d]+)(\.\w{1,4})(\.\w{0,4})?|localhost)(\:(\d{1,5}))?(\/\w+){0,10}(\w+.css)((\?))?(\w+=\w+(\&)?){0,5}$";

        /// <summary>
        /// Regular expression for validating custom labels.
        /// </summary>
        public const string CustomLabel = @"^[a-zA-Z0-9\-\s]+$";
    }
}
