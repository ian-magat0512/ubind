// <copyright file="ColumnMapping.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Entities
{
    using System.Collections.Generic;
    using Humanizer;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ReadWriteModel.Email;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using Portal = UBind.Domain.Portal;
    using Product = UBind.Domain.Product.Product;
    using PT = UBind.Domain.ReadModel.Policy.PolicyTransaction;
    using SerialisedPortal = UBind.Domain.SerialisedEntitySchemaObject.Portal;
    using SerialisedProduct = UBind.Domain.SerialisedEntitySchemaObject.Product;
    using SerialisedPT = UBind.Domain.SerialisedEntitySchemaObject.PolicyTransaction;
    using SerialisedTenant = UBind.Domain.SerialisedEntitySchemaObject.Tenant;
    using Tenant = UBind.Domain.Tenant;

    /// <summary>
    /// This class will hold all the column mapping from serialized entity schema to database column.
    /// </summary>
    public static class ColumnMapping
    {
        /// <summary>
        /// List of all timezones supported by ubind application.
        /// </summary>
        private static readonly IDictionary<(string entityName, string propertyName), string> Columns = new Dictionary<(string entityName, string propertyName), string>
        {
            { (entityName: "quote", propertyName: $"{nameof(Quote.CreatedDateTime)}"), $"quote.{nameof(NewQuoteReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "quote", propertyName: $"{nameof(Quote.CreatedDate)}"), $"quote.{nameof(NewQuoteReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "quote", propertyName: $"{nameof(Quote.ExpiryDateTime)}"), $"quote.{nameof(NewQuoteReadModel.ExpiryTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "quote", propertyName: $"{nameof(Quote.ExpiryDate)}"), $"quote.{nameof(NewQuoteReadModel.ExpiryTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "quote", propertyName: $"{nameof(Quote.LastModifiedDateTime)}"), $"quote.{nameof(NewQuoteReadModel.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "quote", propertyName: $"{nameof(Quote.LastModifiedDate)}"), $"quote.{nameof(NewQuoteReadModel.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "quote", propertyName: $"{nameof(Quote.State)}"), $"quote.{nameof(NewQuoteReadModel.QuoteState).ToCamelCase()}" },
            { (entityName: "quoteVersion", propertyName: $"{nameof(QuoteVersion.CreatedDateTime)}"), $"quoteVersion.{nameof(QuoteVersionReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "quoteVersion", propertyName: $"{nameof(QuoteVersion.CreatedDate)}"), $"quoteVersion.{nameof(QuoteVersionReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "quoteVersion", propertyName: $"{nameof(QuoteVersion.LastModifiedDateTime)}"), $"quoteVersion.{nameof(QuoteVersionReadModel.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "quoteVersion", propertyName: $"{nameof(QuoteVersion.LastModifiedDate)}"), $"quoteVersion.{nameof(QuoteVersionReadModel.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "claim", propertyName: $"{nameof(Claim.CreatedDateTime)}"), $"claim.{nameof(ClaimReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "claim", propertyName: $"{nameof(Claim.CreatedDate)}"), $"claim.{nameof(ClaimReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "claim", propertyName: $"{nameof(Claim.LastModifiedDateTime)}"), $"claim.{nameof(ClaimReadModel.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "claim", propertyName: $"{nameof(Claim.LastModifiedDate)}"), $"claim.{nameof(ClaimReadModel.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "claim", propertyName: $"{nameof(Claim.State)}"), $"claim.{nameof(ClaimReadModel.Status).ToCamelCase()}" },
            { (entityName: "claimVersion", propertyName: $"{nameof(ClaimVersion.CreatedDateTime)}"), $"claimVersion.{nameof(ClaimVersion.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "claimVersion", propertyName: $"{nameof(ClaimVersion.CreatedDate)}"), $"claimVersion.{nameof(ClaimVersion.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "claimVersion", propertyName: $"{nameof(ClaimVersion.LastModifiedDateTime)}"), $"claimVersion.{nameof(ClaimVersion.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "claimVersion", propertyName: $"{nameof(ClaimVersion.LastModifiedDate)}"), $"claimVersion.{nameof(ClaimVersion.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "policy", propertyName: $"{nameof(Policy.CreatedDateTime)}"), $"policy.{nameof(PolicyReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "policy", propertyName: $"{nameof(Policy.CreatedDate)}"), $"policy.{nameof(PolicyReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "policy", propertyName: $"{nameof(Policy.ExpiryDateTime)}"), $"policy.{nameof(PolicyReadModel.ExpiryTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "policy", propertyName: $"{nameof(Policy.ExpiryDate)}"), $"policy.{nameof(PolicyReadModel.ExpiryTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "policy", propertyName: $"{nameof(Policy.LastModifiedDateTime)}"), $"policy.{nameof(PolicyReadModel.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "policy", propertyName: $"{nameof(Policy.LastModifiedDate)}"), $"policy.{nameof(PolicyReadModel.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "policy", propertyName: $"{nameof(Policy.Status)}"), $"policy.{nameof(PolicyReadModel.PolicyState).ToCamelCase()}" },
            { (entityName: "policyTransaction", propertyName: $"{nameof(SerialisedPT.CreatedDateTime)}"), $"policyTransaction.{nameof(PT.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "policyTransaction", propertyName: $"{nameof(SerialisedPT.CreatedDate)}"), $"policyTransaction.{nameof(PT.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "policyTransaction", propertyName: $"{nameof(SerialisedPT.LastModifiedDateTime)}"), $"policyTransaction.{nameof(PT.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "policyTransaction", propertyName: $"{nameof(SerialisedPT.LastModifiedDate)}"), $"policyTransaction.{nameof(PT.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "policyTransaction", propertyName: $"{nameof(SerialisedPT.EffectiveDateTime)}"), $"policyTransaction.{nameof(PT.EffectiveTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "policyTransaction", propertyName: $"{nameof(SerialisedPT.EffectiveDate)}"), $"policyTransaction.{nameof(PT.EffectiveTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "policyTransaction", propertyName: $"{nameof(SerialisedPT.ExpiryDateTime)}"), $"policyTransaction.{nameof(PT.ExpiryTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "policyTransaction", propertyName: $"{nameof(SerialisedPT.ExpiryDate)}"), $"policyTransaction.{nameof(PT.ExpiryTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "customer", propertyName: $"{nameof(Customer.CreatedDateTime)}"), $"customer.{nameof(CustomerReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "customer", propertyName: $"{nameof(Customer.CreatedDate)}"), $"customer.{nameof(CustomerReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "customer", propertyName: $"{nameof(Customer.LastModifiedDateTime)}"), $"customer.{nameof(CustomerReadModel.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "customer", propertyName: $"{nameof(Customer.LastModifiedDate)}"), $"customer.{nameof(CustomerReadModel.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "user", propertyName: $"{nameof(User.CreatedDateTime)}"), $"user.{nameof(UserReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "user", propertyName: $"{nameof(User.CreatedDate)}"), $"user.{nameof(UserReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "user", propertyName: $"{nameof(User.LastModifiedTime)}"), $"user.{nameof(UserReadModel.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "user", propertyName: $"{nameof(User.LastModifiedDate)}"), $"user.{nameof(UserReadModel.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "email", propertyName: $"{nameof(EmailMessage.CreatedDateTime)}"), $"email.{nameof(Email.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "email", propertyName: $"{nameof(EmailMessage.CreatedDate)}"), $"email.{nameof(Email.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "sms", propertyName: $"{nameof(SmsMessage.CreatedDateTime)}"), $"sms.{nameof(Sms.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "sms", propertyName: $"{nameof(SmsMessage.CreatedDate)}"), $"sms.{nameof(Sms.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "product", propertyName: $"{nameof(SerialisedProduct.CreatedDateTime)}"), $"product.{nameof(Product.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "product", propertyName: $"{nameof(SerialisedProduct.CreatedDate)}"), $"product.{nameof(Product.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "tenant", propertyName: $"{nameof(SerialisedTenant.CreatedDateTime)}"), $"tenant.{nameof(Tenant.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "tenant", propertyName: $"{nameof(SerialisedTenant.CreatedDate)}"), $"tenant.{nameof(Tenant.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "organisation", propertyName: $"{nameof(Organisation.CreatedDateTime)}"), $"organisation.{nameof(OrganisationReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "organisation", propertyName: $"{nameof(Organisation.CreatedDate)}"), $"organisation.{nameof(OrganisationReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "organisation", propertyName: $"{nameof(Organisation.LastModifiedDateTime)}"), $"organisation.{nameof(OrganisationReadModel.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "organisation", propertyName: $"{nameof(Organisation.LastModifiedDate)}"), $"organisation.{nameof(OrganisationReadModel.LastModifiedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "document", propertyName: $"{nameof(Document.CreatedDateTime)}"), $"document.{nameof(QuoteDocumentReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "document", propertyName: $"{nameof(Document.CreatedDate)}"), $"document.{nameof(QuoteDocumentReadModel.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "portal", propertyName: $"{nameof(SerialisedPortal.CreatedDateTime)}"), $"portal.{nameof(Portal.CreatedTicksSinceEpoch).ToCamelCase()}" },
            { (entityName: "portal", propertyName: $"{nameof(SerialisedPortal.CreatedDate)}"), $"portal.{nameof(Portal.CreatedTicksSinceEpoch).ToCamelCase()}" },
        };

        /// <summary>
        /// Get equivalent database column.
        /// </summary>
        /// <param name="entityName">The entity name.</param>
        /// <param name="propertyName">The column name.</param>
        /// <returns>The database column name.</returns>
        public static string GetEquivalentDatabaseColumn(string entityName, string propertyName)
        {
            if (!Columns.ContainsKey((entityName, propertyName.Pascalize())))
            {
                return default;
            }

            return Columns[(entityName, propertyName.Pascalize())];
        }
    }
}
