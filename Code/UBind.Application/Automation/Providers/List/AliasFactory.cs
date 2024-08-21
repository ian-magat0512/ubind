// <copyright file="AliasFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Automation;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Sms;

    /// <summary>
    /// Factory for generating item aliases for use in list filtering, based on the type of the list element.
    /// </summary>
    public static class AliasFactory
    {
        private const string DefaultAlias = "item";
        private static Dictionary<Type, string> entityTypes = new Dictionary<Type, string>()
        {
            { typeof(Event), "event" },
            { typeof(ClaimReadModelWithRelatedEntities), "claim" },
            { typeof(ClaimVersionReadModelWithRelatedEntities), "claimVersion" },
            { typeof(CustomerReadModelWithRelatedEntities), "customer" },
            { typeof(DocumentReadModelWithRelatedEntities), "document" },
            { typeof(EmailReadModelWithRelatedEntities), "email" },
            { typeof(SmsReadModelWithRelatedEntities), "sms" },
            { typeof(OrganisationReadModelWithRelatedEntities), "organisation" },
            { typeof(PolicyReadModelWithRelatedEntities), "policy" },
            { typeof(PolicyTransactionReadModelWithRelatedEntities), "policyTransaction" },
            { typeof(QuoteReadModelWithRelatedEntities), "quote" },
            { typeof(QuoteVersionReadModelWithRelatedEntities), "quoteVersion" },
            { typeof(UserReadModelWithRelatedEntities), "user" },
            { typeof(ProductWithRelatedEntities), "product" },
            { typeof(TenantWithRelatedEntities), "tenant" },
            { typeof(PortalWithRelatedEntities), "portal" },
        };

        /// <summary>
        /// Generate an alias for a list element of a given type.
        /// </summary>
        /// <param name="elementType">The type of the list element to generate an alias for.</param>
        /// <returns>The generated alias.</returns>
        public static string Generate(Type elementType) =>
            entityTypes.ContainsKey(elementType)
                ? entityTypes[elementType].ToCamelCase()
                : DefaultAlias;
    }
}
