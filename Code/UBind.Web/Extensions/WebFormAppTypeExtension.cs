// <copyright file="WebFormAppTypeExtension.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Extensions
{
    using UBind.Domain;
    using UBind.Web.Mapping;

    /// <summary>
    /// An extension for the enum <see cref="WebFormAppType"/>.
    /// </summary>
    public static class WebFormAppTypeExtension
    {
        /// <summary>
        /// Converts a string value to WebFormType enum.
        /// </summary>
        /// <param name="formType">The form type.</param>
        /// <returns>A web form type value.</returns>
        public static WebFormAppType ToWebFormAppType(this FormType formType)
        {
            switch (formType)
            {
                case FormType.Claim:
                    return WebFormAppType.Claim;
                default:
                    return WebFormAppType.Quote;
            }
        }
    }
}
