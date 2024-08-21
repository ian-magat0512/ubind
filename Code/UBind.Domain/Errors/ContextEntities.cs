// <copyright file="ContextEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain;

using Humanizer;
using Newtonsoft.Json.Linq;
using System.Net;

public static partial class Errors
{
    public static partial class ContextEntities
    {
        public static Error NotConfigured(string tenantAlias, string productAlias, WebFormAppType formType)
        {
            return new Error(
                "context.entities.not.configured",
                "Context entities are not configured",
                "An attempt was made to access context entities when they have not been configured for this product component. "
                + "This is a product configuration issue which a product developer needs to resolve. "
                + "Please get in touch with customer support. We apologise for the inconvenience.",
                HttpStatusCode.MethodNotAllowed,
                new List<string>
                {
                    $"Tenant: {tenantAlias}",
                    $"Product: {productAlias}",
                    $"Form type: {formType.Humanize()}",
                },
                new JObject
                {
                    { "tenantAlias", tenantAlias },
                    { "productAlias", productAlias },
                    { "formType", formType.Humanize() },
                });
        }

        public static Error NotConfiguredForClaims(string tenantAlias, string productAlias)
        {
            return new Error(
                "context.entities.not.configured.for.claims",
                "Context entities are not configured for claims",
                "An attempt was made to access context entities when they have not been configured for the claim product component. "
                + "This is a product configuration issue which a product developer needs to resolve. "
                + "Please get in touch with customer support. We apologise for the inconvenience.",
                HttpStatusCode.MethodNotAllowed,
                new List<string>
                {
                    $"Tenant: {tenantAlias}",
                    $"Product: {productAlias}",
                },
                new JObject
                {
                    { "tenantAlias", tenantAlias },
                    { "productAlias", productAlias },
                });
        }

        public static Error NotConfiguredForQuoteType(string tenantAlias, string productAlias, QuoteType quoteType)
        {
            return new Error(
                "context.entities.not.configured.for.quote.type",
                $"Context entities are not configured for {quoteType.Humanize()} quotes",
                $"An attempt was made to access context entities when they have not been configured for this {quoteType.Humanize()} quotes. "
                + "This is a product configuration issue which a product developer needs to resolve. "
                + "Please get in touch with customer support. We apologise for the inconvenience.",
                HttpStatusCode.MethodNotAllowed,
                new List<string>
                {
                    $"Tenant: {tenantAlias}",
                    $"Product: {productAlias}",
                    $"Quote type: {quoteType.Humanize()}",
                },
                new JObject
                {
                    { "tenantAlias", tenantAlias },
                    { "productAlias", productAlias },
                    { "quoteType", quoteType.Humanize() },
                });
        }
    }
}
