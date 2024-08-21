// <copyright file="Organisation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace UBind.Domain
{
    using System;
    using System.Net;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Allows enumeration of all application errors as outlined here:
    /// https://enterprisecraftsmanship.com/posts/advanced-error-handling-techniques/.
    /// </summary>
    public static partial class Errors
    {
        public static class Organisation
        {
            public static Error Disabled(string organisationName) => new Error(
                "organisation.disabled",
                $"Organisation disabled",
                $"Organisation '{organisationName}' is disabled. Please contact customer support if you would like to enable this organisation again.",
                HttpStatusCode.Conflict);

            public static Error NotFound(Guid organisationId) => new Error(
                "organisation.with.id.not.found",
                $"We couldn't find organisation '{organisationId}'",
                $"When trying to find organisation '{organisationId}', nothing came up. Please check if you've entered the correct organisation Id.",
                HttpStatusCode.NotFound,
                null,
                new JObject()
                {
                    { "organisationId", organisationId },
                });

            public static Error AliasIsNull(string alias) =>
                new Error(
                    "organisation.alias.cannot.be.the.word.null",
                    $"Organisation alias cannot be the word null",
                    $"The organisation alias cannot be the word \"{alias}\". Please enter a valid name for the organisation alias",
                    HttpStatusCode.BadRequest,
                    null,
                    new JObject()
                    {
                        { "alias", alias },
                    });

            public static Error AliasNotFound(string alias) =>
                new Error(
                    "organisation.alias.not.found",
                    $"Organisation alias not found",
                    $"When trying to resolve an organisation entity using the alias \"{alias}\", the attempt failed because the specified alias could not be matched against one of the organisations in this tenancy."
                    + "To resolve this problem, please ensure that the specified organisation alias matches an organisation in this tenancy. If you require further assistance please contact technical support.",
                    HttpStatusCode.NotFound,
                    null,
                    null);

            public static Error FailedToMigrateForOrganisation(Guid quoteId, Guid organisationId) => new Error(
                "failed.to.migrate.for.organisation",
                $"We couldn't migrate quote '{quoteId}' because it already has an organisation",
                $"The quote already has an organisation '{organisationId}'. Please check if you've entered the correct quote Id.",
                HttpStatusCode.Conflict,
                null,
                new JObject()
                {
                    { "quoteId", quoteId },
                    { "organisationId", organisationId },
                });

            public static Error NotFound(string organisationAlias) => new Error(
                "organisation.with.alias.not.found",
                $"Organisation '{organisationAlias}' not found",
                $"When trying to find an organisation with the alias '{organisationAlias}', nothing came up. Please check if you've entered the correct organisation alias.",
                HttpStatusCode.NotFound,
                null,
                new JObject()
                {
                    { "organisationAlias", organisationAlias },
                });

            public static Error NotFound() => new Error(
                "organisation.not.found",
                $"Organisation not found",
                "The provided organisation ID or alias is either null, empty or non-existing in the record. Please ensure that you are passing the correct ID/alias or contact customer support if you are experiencing this error in the portal.",
                HttpStatusCode.NotFound);

            public static Error TenantNotFound(Guid tenantId) => new Error(
                "organisation.tenant.id.not.found",
                $"We couldn't find tenant with ID '{tenantId}'.",
                $"When trying to find tenant with ID '{tenantId}', nothing came up. Please check if you've entered the correct tenant Id.",
                HttpStatusCode.NotFound);

            public static Error TenantNotFound(string tenantId) => new Error(
                "organisation.tenant.id.not.found",
                $"We couldn't find tenant '{tenantId}'",
                $"When trying to find tenant '{tenantId}', nothing came up. Please check if you've entered the correct tenant Id.",
                HttpStatusCode.NotFound);

            public static Error NewQuoteForProductNotAllowed(string organisationId, string productId, JObject errorData) => new Error(
                "organisation.new.quote.for.product.not.allowed",
                $"New quote for product \"{productId}\" is not allowed for organisation \"{organisationId}\".",
                $"We can't create a quote for the product \"{productId}\", this product is not allowed for organization \"{organisationId}\"" +
                "Please choose a different product that is allowed in the organization. ",
                HttpStatusCode.BadRequest,
                null,
                errorData);

            public static Error AliasUnderTenantAlreadyExists(string tenantAlias, string alias) => new Error(
                "organisation.alias.under.tenant.already.exists",
                "Organisation alias must be unique",
                $"The alias '{alias}' already belongs to the tenant with alias '{tenantAlias}'. Please try a different alias.",
                HttpStatusCode.Conflict);

            public static Error NameUnderTenantAlreadyExists(string tenantAlias, string name) => new Error(
                "organisation.name.under.tenant.already.exists",
                "Organisation name must be unique",
                $"The organisation name '{name}' already belongs to the tenant with alias '{tenantAlias}'. Please try a different name.",
                HttpStatusCode.Conflict,
                null,
                new JObject()
                {
                    { "tenantAlias", tenantAlias },
                    { "name", name },
                });

            public static Error LinkedIdentityAlreadyExists(string organisationExternalId, JObject? data = null) => new Error(
                "organisation.linked.identity.external.id.already.exists",
                "Organisation linked identity external Id already exists",
                $"The linked identity external identifier '{organisationExternalId}' already belongs to another organisation " +
                $"that uses the same authentication method. " +
                $"Please use a unique organisation external identifier.",
                HttpStatusCode.Conflict,
                null,
                data);

            public static Error CannotSetOrganisationToDefaultWithoutAnyOrganisationAdminUser(string organisationName) => new Error(
                "organisation.cannot.set.to.default.without.any.organisation.admin.user",
                "You'll need to create an Organisation Admin user first",
                $"If you want to set {organisationName} to be the default organisation for this tenancy, "
                + "you'll need to ensure there is at least one user with the \"Organisation Admin\" role."
                + "When setting an organisation to be the default organisation, the \"Organisation Admin\" users "
                + "will become \"Tenant Admin\" users, and there needs to be at least one.",
                HttpStatusCode.PreconditionFailed);

            public static Error DuplicateCustomerInDestinationOrganisation(string customerEmail, Guid organisationId, IEnumerable<string>? additionalDetails = null) => new Error(
                "user.transfer.has.duplicate.customer.in.destination.organisation",
                "Unable to transfer user due to duplicate customer email in the destination organisation",
                $"Customer with an email of '{customerEmail}' already exists in the organisation Id '{organisationId}'. "
                + $"Please contact customer support if you would like to transfer this user.",
                HttpStatusCode.Conflict,
                additionalDetails,
                new JObject()
                {
                    { "customerEmail", customerEmail },
                    { "organisationId", organisationId },
                });

            public static Error DuplicateCustomerWithUserInBothOrganisations(
                string sourceCustomerName,
                string sourceCustomerOrganisationName,
                string conflictingEmailAddress,
                string destinationCustomerName,
                string destinationOrganisationName,
                string sourceCustomerConflictingPersonName,
                string destinationPersonName,
                JObject errorData) => new Error(
                    "customer.transfer.has.duplicate.user.in.both.organisations",
                    "Unable to transfer customer due to duplicate customer email with user accounts in both organisations",
                    $"When trying to transfer the customer \"{sourceCustomerName}\" from the organisation \"{sourceCustomerOrganisationName}\" "
                    + $"to the organisation \"{destinationOrganisationName}\", there was a conflict which meant we couldn't proceed. "
                    + $"The customer has a person \"{sourceCustomerConflictingPersonName}\" with a user account with the email address "
                    + $"\"{conflictingEmailAddress}\" and there is a customer in the destination organisation \"{destinationCustomerName}\" "
                    + $"which has a person \"{destinationPersonName}\" with a user account already using that email address. "
                    + "It's not possible to have two different people using the same login email address in the same organisation. "
                    + "You may wish to disable or delete one of the user accounts, or change the email address for one of the "
                    + "conflicting people before trying again.",
                    HttpStatusCode.Conflict,
                    null,
                    errorData);

            public static Error DuplicateDomainNameWithInOrganisation(Guid organisationId, string organisationName, string domainName) => new Error(
                "duplicate.domain.name.within.organisation",
                "Unable to save DKIM Settings due to duplicate domain name with in the organisation",
                $"The domain name '{domainName}' already exists in the organisation {organisationName}. Please check your entry." +
                $"If you believe this is a mistake, or you would like assistance, please contact customer support.",
                HttpStatusCode.Conflict,
                null,
                new JObject()
                {
                    { "organisationName", organisationName },
                    { "organisationId", organisationId },
                    { "domainName", domainName },
                });

            public static Error AlreadyDefault(string name) => new Error(
                "organisation.already.default",
                $"Organisation \"{name}\" is already the default organisation",
                $"The organisation \"{name}\" is already the default organisation.",
                HttpStatusCode.Conflict,
                null,
                new JObject()
                {
                    { "organisationName", name },
                });

            public static Error AlreadyHasALocalAccountAuthMethod(string organisationName) => new Error(
                "organisation.already.has.local.account.auth.method",
                $"When trying to add a \"Local Account\" authentication method to the organisation "
                + "\"{organisationName}\", we found an existing \"Local Account\" authorisation method.",
                $"Each organisation can only have one \"Local Account\" authorisation method.",
                HttpStatusCode.InternalServerError,
                null,
                new JObject()
                {
                    { "organisationName", organisationName },
                });

            public static Error LinkedIdentityProviderAlreadyExists(string organisationName, Guid authenticationMethodId) => new Error(
                "organisation.linked.identity.provider.already.exists",
                "The linked identity provider already exists",
                $"The organisation \"{organisationName} already has a linked identity provider with an ID of \"{authenticationMethodId}\". "
                    + "Please remove that linked identity and try again.",
                HttpStatusCode.Conflict,
                new List<string> {
                    $"Authentication method ID: {authenticationMethodId}",
                    $"OrganisationName: {organisationName}",
                });

            public static Error LinkedIdentityProviderDoesNotExistWhenUpdating(string organisationName, Guid authenticationMethodId)
                => new Error(
                    "organisation.linked.identity.provider.does.not.exist.when.updating",
                    "That linked identity provider doesn't exist",
                    $"When trying to update a linked identity for the organisation \"{organisationName}\" "
                    + $"with the authentication method \"{authenticationMethodId}\", no such linked identity exists. ",
                    HttpStatusCode.InternalServerError,
                    new List<string> {
                        $"Authentication method ID: {authenticationMethodId}",
                        $"OrganisationName: {organisationName}",
                    });

            public static Error RenewalInvitationEmailsDisabled(
                string tenantAlias,
                string organisationName) =>
                new Error(
                    "renewal.invitation.emails.disabled",
                    "Renewal invitation emails are disabled",
                    $"The setting that allows agents to send renewal invitation emails "
                    + "to customers has been disabled for this organisation. Please contact "
                    + "your administrator for assistance. We apologise for any inconvenience.",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    new JObject()
                    {
                        { "tenantAlias", tenantAlias },
                        { "organisationName", organisationName },
                    });

            public static class Login
            {
                public static Error Disabled(string organisationName) => new Error(
                    "login.organisation.with.alias.disabled",
                    $"Organisation disabled",
                    $"Organisation {organisationName} has been disabled. Please contact customer support if you would like to enable this organisation again.",
                    HttpStatusCode.Conflict);

                public static Error OrganisationPortalDisabled(string organisationPortalName) => new Error(
                   "login.organisation.portal.disabled",
                   $"Organisation portal disabled",
                   $"Portal {organisationPortalName} has been disabled. Please contact customer support if you would like to enable this portal again.",
                   HttpStatusCode.Conflict);
            }
        }
    }
}
