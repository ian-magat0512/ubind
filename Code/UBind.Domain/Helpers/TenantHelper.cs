// <copyright file="TenantHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Helper for tenant properties.
    /// </summary>
    public static class TenantHelper
    {
        private static readonly string[] MutualTenants = { "amm", "others_here" };
        private static readonly Dictionary<string, string> MutualReplaceTerms = new Dictionary<string, string>
        {
            { "Insurance", "Risk Protection" },
            { "Premium", "Contribution" },
            { "Premiums", "Contributions" },
            { "Policy", "Protection" },
            { "Policies", "Protections" },
            { "Indemnity", "Protection" },
            { "Indemnities", "Protections" },
            { "Insurer", "Product issuer" },
            { "Insured", "Member" },
            { "Insure", "Cover" }, // Note: placed the 'insure' after 'ensured' to avoid wrong replacement
        };

        /// <summary>
        /// Checks whether the tenant is mutual or not.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the tenant.</param>
        /// <returns>If the tenant is mutual or not.</returns>
        public static bool IsMutual(string tenantAlias)
        {
            return MutualTenants.Contains(tenantAlias);
        }

        /// <summary>
        /// Changed text to Mutual terms by mutual flag.
        /// </summary>
        /// <param name="text">The text to check and replace with mutual terms.</param>
        /// <param name="isMutual">If a mutual tenant.</param>
        /// <returns>The checked and changed text.</returns>
        public static string CheckAndChangeTextToMutual(string text, bool isMutual)
        {
            if (!isMutual)
            {
                return text;
            }

            foreach (var term in MutualReplaceTerms)
            {
                // this replace all title cased terms
                text = text.Replace(term.Key, term.Value);

                // this replace all lower cased terms
                text = text.Replace(term.Key.ToLower(), term.Value.ToLower());
            }

            return text;
        }

        /// <summary>
        /// Changed text to Mutual terms by tenant ID.
        /// </summary>
        /// <param name="text">The text to check and replace with mutual terms.</param>
        /// <param name="tenantAlias">The Alias of the tenant.</param>
        /// <returns>The checked and changed text.</returns>
        public static string CheckAndChangeTextToMutual(string text, string tenantAlias)
        {
            var isMutual = IsMutual(tenantAlias);

            return CheckAndChangeTextToMutual(text, isMutual);
        }

        /// <summary>
        /// Changed text to Mutual terms for error object.
        /// </summary>
        /// <param name="title">The title of the error object.</param>
        /// <param name="message">The message of the error object.</param>
        /// <param name="isMutual">If a mutual tenant.</param>
        public static void CheckAndChangeTextToMutualForErrorObject(ref string title, ref string message, bool isMutual)
        {
            if (!isMutual)
            {
                return;
            }

            foreach (var term in MutualReplaceTerms)
            {
                // this replace all title cased terms
                title = title.Replace(term.Key, term.Value);
                message = message.Replace(term.Key, term.Value);

                // this replace all lower cased terms
                title = title.Replace(term.Key.ToLower(), term.Value.ToLower());
                message = message.Replace(term.Key.ToLower(), term.Value.ToLower());
            }
        }

        public static void ThrowIfTenantNotActive(Guid tenantId, Tenant tenant)
        {
            if (tenant == null)
            {
                throw new ErrorException(Errors.General.NotFound("tenant", tenantId));
            }

            if (tenant.Details.Disabled)
            {
                throw new ErrorException(Errors.Tenant.Disabled(tenant.Details.Alias));
            }

            if (tenant.Details.Deleted)
            {
                throw new ErrorException(Errors.Tenant.Deleted(tenant.Details.Alias));
            }
        }
    }
}
