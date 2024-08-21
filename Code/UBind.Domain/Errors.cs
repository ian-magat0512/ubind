// <copyright file="Errors.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

///////////////////////////////////////////////////////////////////////////////////////////
//// THIS IS A PARTIAL CLASS. THERE ARE OTHER FILES WHICH HAVE MORE ERROR DEFS IN THEM ////
///////////////////////////////////////////////////////////////////////////////////////////
namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Humanizer;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;

    /// <summary>
    /// Allows enumeration of all application errors as outlined here: https://enterprisecraftsmanship.com/posts/advanced-error-handling-techniques/.
    /// </summary>
    public static partial class Errors
    {
        public static class Patch
        {
            public static Error FailedToPatch(string detail) => new Error("failed.patch", "Failed to patch", detail);
        }

        public static class RefundRules
        {
            public static Error InvalidCancellationRefundRule(RefundRule? cancellationRefundRule) =>
                new Error(
                    "cancellation.refund.rule.is.invalid",
                    "Cancellation refund rule is not valid",
                    $"The cancellation refund rule '{cancellationRefundRule}' is not valid or not set when trying to check the cancellation setting." +
                    $" We apologise for the inconvenience.  If you believe this is a mistake, or you would like assistance, please contact customer support.",
                    HttpStatusCode.Conflict,
                    default,
                    new JObject()
                    {
                        { "cancellationRefundRule", cancellationRefundRule.ToString() },
                    });

            public static Error InvalidLastNumberOfYears(int? lastNumberOfYears) =>
            new Error(
                "last.number.of.years.is.invalid",
                "The last number of years is invalid",
                $"when trying to check the cancellation setting, the last number of years period selection is selected but the last number of years was not set or the value is zero." +
                $" We apologise for the inconvenience.  If you believe this is a mistake, or you would like assistance, please contact customer support.",
                HttpStatusCode.Conflict,
                default,
                new JObject()
                {
                      { "lastNumberOfYears", lastNumberOfYears.ToString() },
                });
        }

        public static class Claim
        {
            public static Error NotFound(Guid claimId) =>
                new Error(
                    "claim.not.found",
                    $"Claim not found",
                    $"When trying to find claim '{claimId}', nothing came up. Please ensure that you are passing the correct ID or contact customer support if you are experiencing this error in the portal.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "claimId", claimId },
                    });

            public static Error AlreadyAssociated(Guid claimId, Guid policyId, bool isMutual)
            {
                var code = "claim.already.associated.with.policy";
                var title = "Claim already associated with policy";
                var message = $"When trying to associate the claim with ID \"{claimId}\" with the policy {policyId}, we found that it was already associated, so there was nothing to be done.";
                TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                return new Error(
                    code,
                    title,
                    message,
                    HttpStatusCode.PreconditionFailed);
            }

            public static Error NoPermissionToAccessClaim(Guid claimId) =>
                new Error(
                    "no.permission.access.claim",
                    "You don't have access to that claim",
                    $"You tried to access the claim with ID \"{claimId}\", but it is not your claim or you do not have permission to access this claim. If you believe you should have access to this claim, please get in touch with your administrator, or contact customer support.",
                    HttpStatusCode.Forbidden);

            public static Error NoPermissionToDeleteClaimsAssociatedWithPolicy(Guid policyId) =>
                new Error(
                    "policy.claim.delete.not.authorized",
                    "You don't have permission to delete claims",
                    $"You tried to delete the claims associated to policy with ID \"{policyId}\", but you do not have the necessary permission."
                    + $" If you believe you should have access to this claim, please get in touch with your administrator, or contact customer support.",
                    HttpStatusCode.Forbidden);

            public static Error MustCreateAgainstPolicy(string productAlias, string productName, string operationName) =>
                new Error(
                    $"must.create.claim.against.policy",
                    $"You need to create the claim against the policy.",
                    $"The product \"{productName}\" has been configured to only allow claims to be created against an "
                    + "existing policy. We apologise for the inconvenience. "
                    + "If you believe this is a mistake, or you would like assistance, please contact customer support.",
                    HttpStatusCode.Forbidden,
                    default,
                    new JObject()
                    {
                        { "productAlias", productAlias },
                        { "productName", productName },
                        { "operationName", operationName },
                    });

            public static Error CannotDisassociateWithEnabledCreateClaimAgainstPolicy() =>
                new Error(
                    $"policy.deletion.claim.disassociation.disabled",
                    $"Policy cannot be deleted because claim disassociation is not allowed for this product",
                    $"When trying to delete a policy, the attempt failed because the specified policy was associated with one or more claims that could not be disassociated from the policy based on the product settings."
                    + $" To resolve this issue, either change the value for the \"associatedClaimAction\" parameter to \"delete\", or update the product settings to disable the \"Must create claims against a policy\" option."
                    + $" If you require further assistance please contact technical support.",
                    HttpStatusCode.Forbidden);

            public static Error WorkflowConfigurationNotFound(IProductContext productContext) =>
                new Error(
                    "claim.workflow.configuration.not.found",
                    "Claim workflow configuration not found",
                    "The workflow configuration was not found for this claim. "
                    + "Please ensure you have defined a product.json file with the object \"claimWorkflow\". "
                    + "This is a product misconfiguration.",
                    HttpStatusCode.NotFound,
                    null,
                    JObject.FromObject(productContext));

            public static Error OperationNotPermittedForState(
                string operationName,
                string fromState,
                string toState,
                List<string> allowedStates) =>
                new Error(
                    "claim.workflow.operation.not.permitted.for.state",
                    $"That operation is not permitted for the current claim state",
                    "The claim workflow is configured to allow certain operations when in certain states. "
                    + $"When trying to perform the operation \"{operationName}\" the state was \"{fromState}\", "
                    + $"however it is only allowed from the following states: {string.Join(", ", allowedStates)}. "
                    + $"We were therefore unable to transition to the state \"{toState}\". "
                    + "This is a product configuration issue, which can be resolved by either ensuring a prior "
                    + "transition has happened before performing this one, or configuring the workflow to allow "
                    + $"the \"{operationName}\" operation to be performed when the claim is in the \"{fromState}\" "
                    + "state.",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    new JObject
                    {
                        { "operationName", operationName },
                        { "fromState", fromState },
                        { "toState", toState },
                        { "allowedStates", JArray.FromObject(allowedStates) },
                    });
        }

        public static class ProductFeatureSetting
        {
            public static Error AlreadyEnabled(Guid tenantId, Guid productId, string settingName) =>
                new Error(
                    "product.feature.setting.already.enabled",
                    "Product feature setting already enabled",
                    $"The product feature setting \"{settingName}\" for productId {productId} and tenantId {tenantId} "
                    + "has already been enabled so you cannot perform the operation \"enable\" on it. "
                    + "Please refresh or reload your screen. We apologise for the inconvenience. "
                    + "If you believe this is a mistake, or you would like assistance, please contact customer support.",
                    HttpStatusCode.Conflict,
                    default,
                    new JObject()
                    {
                        { "tenantId", tenantId },
                        { "productId", productId },
                    });

            public static Error AlreadyDisabled(Guid tenantId, Guid productId, string settingName) =>
                new Error(
                    "product.feature.setting.already.disabled",
                    "Product feature setting already disabled",
                    $"The product feature setting \"{settingName}\" for productId {productId} and tenantId {tenantId} "
                    + "has already been disabled so you cannot perform the operation \"disable\" on it. "
                    + "Please refresh or reload your screen. We apologise for the inconvenience. "
                    + "If you believe this is a mistake, or you would like assistance, please contact customer support.",
                    HttpStatusCode.Conflict,
                    default,
                    new JObject()
                    {
                        { "tenantId", tenantId },
                        { "productId", productId },
                    });

            public static Error IncorrectExpiredPolicyRenewalDuration(int renewalDurationIndays) =>
                new Error(
                "incorrect.expired.policy.renewal.duration",
                "Incorrect expired policy renewal duration",
                $"The expired policy renewal duration must be between 1 and 365 days but you entered {renewalDurationIndays.ToString()}."
                + " Please check you've entered the correct details. We apologise for the inconvenience. If you believe this is a mistake, or you would like assistance, please contact customer support.",
                HttpStatusCode.BadRequest,
                default,
                new JObject()
                {
                           { "renewalDurationIndays", renewalDurationIndays },
                });

            public static Error ProductFeatureDisabledForTheOperation(Guid productId, string productName, string featureName, string operationName) =>
                new Error(
                    $"{featureName.ToLower()}.transactions.disabled.on.product",
                    $"{featureName} transactions disabled on product",
                    $"Because {featureName} transactions have been disabled on the {productName} product you cannot perform the \"{operationName}\" operation." +
                    $" We apologise for the inconvenience. If you believe this is a mistake, or you would like assistance, please contact customer support.",
                    HttpStatusCode.Forbidden,
                    default,
                    new JObject()
                    {
                        { "productId", productId },
                        { "productName", productName },
                        { "operationName", operationName },
                    });

            public static Error ProductFeatureDisabled(Guid productId, string productName) =>
                new Error(
                    $"purchase.transactions.disabled.on.product",
                    $"purchase transactions disabled on product",
                    $"Because purchase transactions have been disabled on the {productName} product you cannot perform the quote/policy creation." +
                    $" We apologise for the inconvenience. If you believe this is a mistake, or you would like assistance, please contact customer support.",
                    HttpStatusCode.Forbidden,
                    default,
                    new JObject()
                    {
                        { "productId", productId },
                        { "productName", productName },
                    });

            public static Error QuoteTypeDisabled(Guid productId, string productName, string operationName)
            {
                var article = operationName.ToLower() == "adjustment" ? "an" : "a";
                return new Error(
                    $"quote.creation.{operationName.ToLower()}.quote.type.disabled",
                    $"{operationName} quotes are disabled for this product",
                    $"When trying to create {article} {operationName} quote for the {productName} product, the attempt failed because the product settings for {productName} " +
                    $"prevent the creation of {operationName} quotes. To resolve this issue please enable {operationName} quotes in the product settings for the " +
                    $"{productName} product. If you need further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    default,
                    new JObject()
                    {
                        { "productId", productId },
                        { "productName", productName },
                        { "operationName", operationName },
                    });
            }

            public static Error QuoteNewBusinessDisabled(Guid productId, string productName) =>
                new Error(
                    $"quote.creation.new.business.quote.type.disabled",
                    $"We've suspended new quotes for this product",
                    $"Due to current business arrangements, we're not currently supporting the quoting of new policies under this product. This isn't necessarily a " +
                    $"permanent arrangement. If you would like to understand more, and if or when this restriction will be lifted, please get in touch with support.",
                    HttpStatusCode.BadRequest,
                    default,
                    new JObject()
                    {
                        { "productId", productId },
                        { "productName", productName },
                    });

            public static Error PolicyTransactionTypeDisabled(Guid productId, string productName, string operationName) =>
                new Error(
                    $"policy.{operationName.ToLower()}.transaction.type.disabled",
                    $"{operationName} policy transactions are disabled for this product",
                    $"When trying to perform {operationName} to the policy, the attempt failed because the product settings for {productName} " +
                    $"prevent the creation of {operationName} policy transactions. To resolve this issue please enable {operationName} policy transactions in the product settings " +
                    $"for the {productName} product. If you need further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    default,
                    new JObject()
                    {
                        { "productId", productId },
                        { "productName", productName },
                        { "operationName", operationName },
                    });

            public static Error PolicyNewBusinessDisabled(Guid productId, string productName) =>
                new Error(
                    $"policy.issuance.new.business.transaction.type.disabled",
                    $"New business policy transactions are disabled for this product",
                    $"When trying to issue a new policy for the {productName} product, the attempt failed because the product settings for {productName} " +
                    $"prevent the creation of new business policy transactions. To resolve this issue please enable new business policy transactions in the product settings for the " +
                    $"{productName} product. If you need further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    default,
                    new JObject()
                    {
                        { "productId", productId },
                        { "productName", productName },
                    });

            public static Error InvalidProductFeatureType(ProductFeatureSettingItem productFeatureType) =>
                new Error(
                    "product.feature.setting.type.is.invalid",
                    "Product feature setting type is invalid",
                    $"The product feature type '{productFeatureType}' is not valid. " +
                    $" Please check you've entered correct the correct details. We apologise for the inconvenience.  If you believe this is a mistake, or you would like assistance, please contact customer support.",
                    HttpStatusCode.Conflict,
                    default,
                    new JObject()
                    {
                        { "productFeatureType", productFeatureType.ToString() },
                    });

            public static Error NotFound(Guid tenantId, Guid productId) =>
                new Error(
                    "product.feature.settings.not.found",
                    "Product feature settings not found",
                    $"When trying to find a product feature with the tenantId {tenantId} and productId {productId}, nothing came up. Please check you've entered the correct details. We apologise for the inconvenience. If you think this is bug, please contact customer support.",
                    HttpStatusCode.NotFound,
                    default,
                    new JObject()
                    {
                        { "productId", productId },
                        { "tenantId", tenantId },
                    });

            public static Error PolicyNewBusinessFeatureDisabled(Guid productId, string productName) =>
                new Error(
                    "policy.issuance.product.new.business.transactions.disabled",
                    "New business policy transactions were disabled for product",
                    $"When trying to issue a new policy for the {productName} product, the attempt failed because the product settings for {productName}" +
                    $" prevent the creation of new business policy transactions. To resolve this issue please enable \"Purchase\" policy transactions " +
                    $"in the product settings for the {productName} product. If you need further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    default,
                    new JObject()
                    {
                        { "productId", productId },
                        { "productName", productName }
                    });
        }

        public static class Application
        {
            public static Error CannotPerformOperationOnSubmittedApplication(string operationName) =>
                new Error(
                    "cannot.perform.operation.on.submitted.application",
                    "Application already submitted",
                    $"This application has already been submitted so you cannot perform the operation '{operationName}' on it.",
                    HttpStatusCode.Conflict);

            public static Error AlreadySubmitted() =>
                new Error(
                    "application.already.submitted",
                    "Application already submitted",
                    "Your attempt to submit this application failed because it has already been submitted. An application can't be submitted more than once.",
                    HttpStatusCode.PreconditionFailed);

            public static Error SubmissionRequiresFormData() =>
                new Error(
                    "application.submission.requires.form.data",
                    "There's no form data",
                    "Your attempt to submit this application failed because it wasn't accompanied with form data. Please ensure that the form is filled out before attempting to submit an application.",
                    HttpStatusCode.PreconditionFailed);

            public static Error NoPasswordToVerify(Guid quoteId) =>
                new Error(
                    "application.verify.password.none",
                    "No password stored against quote",
                    $"When trying to verify the password for quote {quoteId}, there was no password found.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "quoteId", quoteId.ToString() },
                    });

            public static Error CannotRetrieveRowsAboveLimit(int rowCount, int maxRows) =>
                new Error(
                    "cannot.retrieve.rows.above.limit",
                    "Cannot retrieve rows above limit",
                    $"Cannot retrieve {rowCount} rows which is greater than the allowable maximum row count of {maxRows}. " +
                    $"We apologise for the inconvenience. " +
                    $"If you believe this is a mistake, or you would like assistance, please contact customer support.",
                    HttpStatusCode.RequestEntityTooLarge);
        }

        public static class SearchIndex
        {
            public static Error DeleteItemsFromIndexNoIdsProvided() =>
               new Error(
                   "delete.indexes.called.but.no.ids.provided",
                   "No ids provided for deletion.",
                   "An attempt to delete lucene indexes occurred but no Ids are provided.",
                   HttpStatusCode.PreconditionFailed);

            public static Error SearchIndexInconsistency(string objectType) =>
                new Error(
                    "search.index.inconsistency",
                    "Search Indexes Need Regenerating",
                    $"An inconsistency was detected when trying to access the search index for {objectType}. Please regenerate the search indexes to resolve this issue.",
                    HttpStatusCode.BadRequest);

            public static Error ProductNotFound(string tenantId, string productId, JObject errorData) =>
               new Error(
                   $"search.index.product.name.does.not.exist",
                   $"The product name was not found",
                   $"We are trying to build a search index document and was trying to retrieve the product name with the Id: {productId}, and tenantId: {tenantId} but it was not found. " +
                   $"If you think this is bug, we apologise for the inconvenience, and please get in touch with customer support.",
                   HttpStatusCode.NotFound,
                   null,
                   errorData);

            public static Error InvalidSortingPropertyName(string sortingPropertyName) =>
               new Error(
                   $"search.index.invalid.sorting.property.name",
                   $"Invalid sorting property name",
                   $"When trying to search index using sorting property name \"{sortingPropertyName}\", it failed because it is not a valid sorting property name. " +
                   $"If you think this is bug, we apologise for the inconvenience, and please get in touch with customer support.",
                   HttpStatusCode.BadRequest);

            public static Error InvalidDateFilteringPropertyName(string dateFilteringPropertyName) =>
               new Error(
                   $"search.index.invalid.date.filtering.property.name",
                   $"Invalid date filtering property name",
                   $"When trying to search index using date filtering property name \"{dateFilteringPropertyName}\", it failed because it is not a valid date filtering property name. " +
                   $"If you think this is bug, we apologise for the inconvenience, and please get in touch with customer support.",
                   HttpStatusCode.BadRequest);

            public static Error SearchIndexNotFound(string tenantId, DeploymentEnvironment environment, string indexName) =>
                new Error(
                    "search.index.not.found",
                    "Search results are still being indexed",
                    $"The {indexName} search results are not yet available as they are still being indexed. "
                    + "Please try again soon. We apologise for the inconvenience.",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    new JObject()
                    {
                        { "tenantId", tenantId },
                        { "environment", environment.ToString() },
                        { "indexName", indexName },
                    });
        }

        public static class BackgroundJob
        {
            public static Error JobAlreadyAcknowledge(string backgroundJobId) =>
                new Error(
                    "background.job.already.acknowledged",
                    "Background job already acknowledged",
                    $"The background job with ID {backgroundJobId} has already been acknowledged. A background job can only be acknowledged once, and you cannot change the acknowledgement details after they have been set.",
                    HttpStatusCode.Conflict);

            public static Error JobDoesNotExist(string backgroundJobId) =>
                new Error(
                    "background.job.does.not.exist",
                    "Background job does not exist",
                    $"The background job with ID {backgroundJobId} does not exist. Please confirm the background job ID before trying again.",
                    HttpStatusCode.NotFound);
        }

        public static class QuoteVersion
        {
            public static Error CustomerNotYetCreated() =>
                new Error(
                    "quote.version.customer.not.yet.created",
                    "Customer needs to be created first",
                    "An attempt was made to create a quote version, but no customer record has been created. In order to create a quote version, a customer must have been created first.",
                    HttpStatusCode.PreconditionFailed);
        }

        public static class JsonDocument
        {
            public static Error JsonInvalid(
                string jsonDocumentName,
                string message,
                int lineNumber,
                int linePosition,
                string path,
                string sourceJson) =>
                new Error(
                    "json.invalid",
                    "Json invalid",
                    $"The JSON document '{jsonDocumentName}' does not contain valid JSON. "
                    + message + ". "
                    + "Please run it through a JSON Validator such as https://jsonlint.com/ then fix the errors before trying again.",
                    HttpStatusCode.BadRequest,
                    new string[]
                    {
                        $"Path: {path}",
                        $"Line number: {lineNumber}",
                        $"Position: {linePosition}",
                    },
                    new JObject()
                    {
                        { "jsonDocumentName", jsonDocumentName },
                        { "jsonPath", path },
                        { "lineNumber", lineNumber },
                        { "linePosition", linePosition },
                        { "sourceJson", sourceJson?.LimitLengthWithEllipsis(100000) },
                    });

            public static Error JsonInvalidMissingPropertyValue(string jsonDocumentName, string jsonPath, string sourceJson) =>
                new Error(
                    "json.invalid.missing.property.value",
                    "Json invalid - missing property value",
                    $"The JSON document \"{jsonDocumentName}\" does not contain valid JSON. "
                    + $"There was no value defined for the property at path \"{jsonPath}\". "
                    + "Please run it through a JSON Validator such as https://jsonlint.com/ then fix the errors before trying again.",
                    HttpStatusCode.BadRequest,
                    null,
                    new JObject()
                    {
                        { "jsonDocumentName", jsonDocumentName },
                        { "jsonPath", jsonPath },
                        { "sourceJson", sourceJson?.LimitLengthWithEllipsis(100000) },
                    });

            public static Error SchemaVersionPropertyNotFound(string jsonDocumentName) =>
                new Error(
                    "json.schemaVersion.property.not.found",
                    "Schema version property missing",
                    $"The JSON document '{jsonDocumentName}' must contain a property 'schemaVersion' "
                    + "containing the version of the schema which it should be validated against, e.g. 2.0.0.",
                    HttpStatusCode.NotFound);

            public static Error SchemaVersionNotFound(string schemaName, string schemaVersion) =>
                new Error(
                    "json.schema.version.not.found",
                    "Schema version not found",
                    $"Could not find a JSON schema '{schemaName}' with version '{schemaVersion}. "
                    + "Check that you have specified the correct schema version in your 'schemaVersion' property",
                    HttpStatusCode.NotFound);

            public static Error SchemaValidationFailure(string jsonDocumentName, string jsonSchemaName, IEnumerable<string> additionalDetails) =>
                new Error(
                    "json.schema.validation.failure",
                    "Schema validation failure",
                    $"Validation of the JSON document '{jsonDocumentName}' against the schema '{jsonSchemaName} failed.",
                    HttpStatusCode.BadRequest,
                    additionalDetails);

            public static Error UnexpectedToken(
                JsonReader reader, Type objectType, object existingValue, string? exceptionMessage = null)
            {
                var existingValueString = existingValue?.ToString()?.LimitLengthWithEllipsis(100000);
                var additionalDetails = new JObject
                {
                    { "objectType", objectType.ToString() },
                    { "existingValue", existingValueString },
                    { "unexpectedTokenType", reader.TokenType.ToString() },
                };
                if (exceptionMessage != null)
                {
                    additionalDetails.Add("exceptionMessage", exceptionMessage);
                }

                return new Error(
                    "unexpected.token",
                    "Unexpected JSON Token",
                    $"Unexpected JSON token '{reader.TokenType}' encountered while deserializing {objectType}. " +
                    $"The existing value is: '{existingValueString}'."
                    + "Please validate your JSON using a tool like https://jsonlint.com/ and fix any errors before retrying.",
                    HttpStatusCode.BadRequest,
                    null,
                    additionalDetails);
            }

            public static Error CannotDeserializeType(string stringValue, string typeName) =>
                new Error(
                    "cannot.deserialize.type",
                    "Cannot deserialize type",
                    $"Cannot deserialize '{stringValue.LimitLengthWithEllipsis(100000)}' to type '{typeName}' from string representation. "
                    + "Please make sure that there is a suitable 'Parse' method or constructor that takes a 'string' parameter for this type.",
                    HttpStatusCode.BadRequest,
                    null,
                    new JObject
                    {
                        { "stringValue", stringValue.LimitLengthWithEllipsis(100000) },
                        { "typeName", typeName },
                    });
        }

        public static class Phone
        {
            public static Error PhoneEmpty() =>
               new Error(
                   "phone.number.empty",
                   "Phone number empty",
                   "Thee phone number was empty. Please ensure you specify a phone number.");

            public static Error PhoneInvalid(string phoneNumber) =>
               new Error(
                   "phone.number.invalid",
                   "Phone number invalid",
                   $"The phone number provided '{phoneNumber}' was not valid. Please ensure the phone number format is valid in your region.");
        }

        public static class WebAddress
        {
            public static Error WebAddressEmpty() =>
                new Error(
                    "web.address.empty",
                    "Web Address empty",
                    "The web address was empty. Please ensure you specify a web address.");

            public static Error WebAddressInvalid(string webAddress) =>
                new Error(
                    "web.address.invalid",
                    "Web Address invalid",
                    $"The web address provided '{webAddress}' was not valid. Please ensure the web address is a valid URL.");
        }

        public static class Email
        {
            public static Error NotFound(Guid emailId) =>
                new Error(
                    "email.not.found",
                    $"Email not found",
                    $"When trying to find email '{emailId}', nothing came up. Please ensure that you are passing the correct ID or contact customer support if you are experiencing this error in the portal.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "emailId", emailId },
                    });

            public static Error AddressEmpty() =>
                new Error(
                    "email.address.empty",
                    "Email address empty",
                    "The email address was empty. Please ensure you specify an email address.");

            public static Error AddressInvalid(string emailAddress) =>
                new Error(
                    "email.address.invalid",
                    "Email address invalid",
                    $"The email address provided '{emailAddress}' was not valid. Please ensure you specify a valid RFC 5322 format email address.");

            public static Error NoBodySpecified() =>
                new Error(
                    "email.body.not.specified",
                    "Email body not specified",
                    "You must ensure that your email has a body. Please specify at least one of: textBody, textBodyTemplate, htmlBody, htmlBodyTemplate");

            public static Error TextBodySourceConflict() =>
                new Error(
                    "email.text.body.source.conflict",
                    "Email text boday source conflict",
                    "You must not specify both a textBody and a textBodyTemplate. Choose one or the other.",
                    HttpStatusCode.Conflict);

            public static Error HtmlBodySourceConflict() =>
                new Error(
                    "email.html.body.source.conflict",
                    "Email html body source conflict",
                    "You must not specify both a htmlBody and a htmlBodyTemplate. Choose one or the other.",
                    HttpStatusCode.Conflict);

            public static Error NoRecipientSpecified() =>
                new Error(
                    "email.no.recipient.specified",
                    "No email recipient specified",
                    "You must ensure there is at least one email recipient (to address).");
        }

        public static class Operations
        {
            public static Error NotFound(string operationId) =>
                new Error(
                    "operation.not.found",
                    "Operation not found",
                    $"An operation with id '{operationId}' was not found. Please check that you've defined an operation with the exact id.",
                    HttpStatusCode.NotFound);

            public static Error EnvironmentMisMatch(string objectType) =>
                new Error(
                    "environment.mismatch",
                    "Environment Mismatch",
                    $"The {objectType} you are trying to operate on belongs to a different environment than the one specified in the operation",
                    HttpStatusCode.BadRequest);

            public static Error UnsupportedEventType(string operationId, ApplicationEventType applicationEventType) =>
                new Error(
                    "operation.event.not.supported",
                    "Operation event not supported",
                    $"The event '{Enum.GetName(typeof(ApplicationEventType), applicationEventType)}' is not supported for the operation with id '{operationId}'.",
                    HttpStatusCode.Conflict);

            public static class Bind
            {
                public static Error NotPermittedForQuoteState(string invalidState) =>
                    new Error(
                        "operation.bind.not.permitted.for.quote.state",
                        "Invalid quote state for binding",
                        $"An attempt was made to bind a quote, however the quote was in the \"{invalidState}\" state "
                        + $"and according to the workflow definition, this is not one of the permitted states where "
                        + "binding is permitted. Typically the quote state needs to be \"approved\" in order to bind."
                        + "This is likely a problem with the product configuration. Please get in touch with customer support. "
                        + "We apologise for the inconvenience.",
                        HttpStatusCode.BadRequest);

                public static Error InvalidCalculationStateDetected(string invalidState, string requiredState) =>
                    new Error(
                        "operation.bind.invalid.calculation.state",
                        "Invalid calculation state for binding",
                        $"An attempt was made to bind a quote, however the calculation was in the \"{invalidState}\" state "
                        + $"and it's required to be in the the state \"{requiredState}\". "
                        + "This is likely a problem with the product configuration. Please get in touch with customer support. "
                        + "We apologise for the inconvenience.",
                        HttpStatusCode.BadRequest);

                public static Error SettlementRequired() =>
                    new Error(
                        "operation.bind.settlement.required",
                        "Settlement is required prior to completion of operation",
                        $"The operation requires payment to be settled prior to completion",
                        HttpStatusCode.PaymentRequired);

                public static Error CalculationIdPassedIsNotTheLatestForQuote() =>
                    new Error(
                        "operation.bind.not.permitted",
                        "There's a pending calculation",
                        $"We couldn't complete your transaction yet because there's a pending calculation. "
                        + "Please wait a few seconds and try again.",
                        HttpStatusCode.PreconditionFailed);

                public static Error CalculationIdNotProvided() =>
                    new Error(
                        "operation.bind.calculation.id.not.found",
                        "Calculation Id not found for binding",
                        $"An attempt was made to bind a quote, however the calculation Id was not provided"
                        + "Please get in touch with customer support.",
                        HttpStatusCode.PreconditionFailed);

                public static Error FormDataReferencedByCalculationIsNotFound(Guid calculationResultId) =>
                    new Error(
                        "operation.bind.calculation.form.model.cannot.be.found",
                        "Form model referenced by calculation cannot be found",
                        "When trying to retrieve the form model assosciated with the given calculation result,  "
                        + "the form model retrieved turned out to be null. Please get in touch with customer support. "
                        + "We apologise for the inconvenience.",
                        HttpStatusCode.PreconditionFailed,
                        new List<string>
                        {
                            $"Calculation Result: {calculationResultId}",
                        });
            }
        }

        public static class Invoice
        {
            public static Error AlreadyIssued() =>
                new Error(
                    "invoice.already.issued",
                    "Invoice already issued",
                    "An invoice has already been issued in association with this quote, so we cannot issue another one.",
                    HttpStatusCode.PreconditionFailed);

            public static Error RequiresFormData() =>
                new Error(
                    "invoice.requires.form.data",
                    "There's no form data",
                    "Your attempt to create an invoice for this quote failed because it wasn't accompanied with form data. Please ensure that the form is filled out before attempting to generate an invoice for a quote.",
                    HttpStatusCode.PreconditionFailed);
        }

        public static class Maintenance
        {
            public static Error DbLogFileShrinkFailed(int numberOfAttempts, decimal thresholdMb, decimal actualSizeMb) => new Error(
                "maintenance.db.log.file.shrink.failed",
                "Database log file shrink failed",
                $"The database log file shrink failed after {numberOfAttempts} attempts. The log file size is "
                + $"{actualSizeMb} MB, which is greater than the threshold of {thresholdMb} MB. "
                + "Please get in touch with a systems administrator to find out why the log file isn't shrinking, "
                + "and to attempt to perform the shrinking manually.",
                HttpStatusCode.InternalServerError);

            public static class Rollback
            {
                public static Error NoPriorSequenceNumbers() =>
                    new Error(
                        "maintenance.rollback.no.prior.sequence.numbers",
                        "Cannot roll back",
                        "There are no prior sequence numbers to roll back to.",
                        HttpStatusCode.Conflict);

                public static Error CannotRollbackToSameSequenceNumber(int targetSequenceNumber, int currentSequenceNumber) =>
                    new Error(
                        "maintenance.rollback.cannot.rollback.to.same.sequence.number",
                        "Cannot roll back",
                        $"The sequence number specified to roll back to ({targetSequenceNumber}) cannot be the same as the current sequence number ({currentSequenceNumber}).",
                        HttpStatusCode.Conflict);

                public static Error TargetSequenceNumberNotFound(int targetSequenceNumber, int currentSequenceNumber) =>
                    new Error(
                        "maintenance.rollback.target.sequence.number.not.found",
                        "Cannot roll back",
                        $"The sequence number specified to roll back to ({targetSequenceNumber}) does not exist. The current sequence number is {currentSequenceNumber}.",
                        HttpStatusCode.NotFound);
            }
        }

        public static class Tenant
        {
            public static Error AliasIsNull(string alias) =>
                new Error(
                    "tenant.alias.cannot.be.the.word.null",
                    $"Tenant alias cannot be the word null",
                    $"The tenant alias cannot be the word \"{alias}\". Please enter a valid name for the tenant alias",
                    HttpStatusCode.BadRequest,
                    null,
                    new JObject()
                    {
                        { "alias", alias },
                    });

            public static Error Disabled(string tenantName) =>
                new Error(
                    "tenant.disabled",
                    "Tenant disabled",
                    $"Tenant {tenantName} has been disabled. Please contact customer support if you would like to enable this tenant again.",
                    HttpStatusCode.NotFound);

            public static Error Deleted(string tenantAlias) =>
                new Error(
                    "tenant.deleted",
                    "Tenant deleted",
                    $"Tenant \"{tenantAlias}\" was deleted. Please contact customer support if you would like to restore this tenant.",
                    HttpStatusCode.Gone);

            public static Error NameInUse(string name) =>
                new Error(
                    "tenant.name.in.use",
                    "That name is taken",
                    $"The tenant name {name} is used by another tenant. Please choose another name.",
                    HttpStatusCode.Conflict);

            public static Error AliasInUse(string alias, string? tenantName = null) =>
                new Error(
                    "tenant.alias.in.use",
                    "That alias is taken",
                    $"The tenant alias {alias} is used by " + (tenantName == null ? "another tenant" : $"the tenant {tenantName}") + ". Please choose another alias.",
                    HttpStatusCode.Conflict);

            public static Error CustomDomainInuse(string customDomain) =>
                new Error(
                    "custom.domain.in.use",
                    "That custom domain is in used",
                    $"The custom domain {customDomain} is used by other tenant. Please choose another portal domain.",
                    HttpStatusCode.Conflict);

            public static Error IdInUse(string id, string? tenantName = null) =>
                new Error(
                    "tenant.id.in.use",
                    "That ID is taken",
                    $"The tenant id {id} is used by " + (tenantName == null ? "another tenant" : $"the tenant {tenantName}") + ". "
                    + "Currently the id is set to the same as the initial alias, so please choose another alias.",
                    HttpStatusCode.Conflict);

            public static Error Mismatch(string objectName, string objectTenantAlias, string thisTenantAlias) =>
                new Error(
                    "tenant.object.mismatch",
                    "Tenant object mismatch",
                    $"You have tried to operate on a {objectName} in tenancy \"{objectTenantAlias}\" from tenancy \"{thisTenantAlias}\", which is not allowed.");

            public static Error Mismatch(string objectName, Guid objectTenantId, Guid thisTenantId) =>
                new Error(
                    "tenant.object.mismatch",
                    "Tenant object mismatch",
                    $"You have tried to operate on a {objectName} in tenancy \"{objectTenantId}\" from tenancy \"{thisTenantId}\", which is not allowed.");

            public static Error AliasNotFound(string alias) =>
            new Error(
                "tenant.alias.not.found",
                "Tenant not found",
                $"When trying to find a tenant with the alias \"{alias}\", nothing came up. Please take note that tenant aliases are case sensitive. " +
                $"If you need assistance with this, please don't hesitate to get in touch with customer support.",
                HttpStatusCode.NotFound,
                null,
                new JObject()
                {
                        { "alias", alias },
                });

            public static Error NotFound() =>
                new Error(
                    "tenant.not.found",
                    "Tenant not found",
                    "The provided tenant Id is either null, empty or non-existing in the record. Please ensure that you are passing the correct Id or contact customer support if you are experiencing this error in the portal.",
                    HttpStatusCode.NotFound);

            public static Error NotFound(string tenantId) =>
                new Error(
                    "tenant.with.id.not.found",
                    $"We couldn't find tenant '{tenantId}'",
                    $"When trying to find tenant '{tenantId}', nothing came up. Please ensure that you are passing the correct Id or contact customer support if you are experiencing this error in the portal.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "tenantId", tenantId },
                    });

            public static Error NotFound(Guid tenantId) =>
                new Error(
                    "tenant.with.id.not.found",
                    $"We couldn't find tenant '{tenantId}'",
                    $"When trying to find tenant '{tenantId}', nothing came up. Please ensure that you are passing the correct Id or contact customer support if you are experiencing this error in the portal.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "tenantId", tenantId },
                    });

            public static Error ProductDirectoryAlreadyExists(string oldtenantAlias, string newTenantAlias, string folderName) =>
                new Error(
                    "product.directory.already.exists",
                    "The product directory already exists",
                    $"When renaming tenant alias from '{oldtenantAlias}' to '{newTenantAlias}', "
                    + $"We have found that the product directory '{folderName}/{newTenantAlias}' already exist, "
                    + "please delete or rename the said product file directory. "
                    + "If you need assistance please get in touch with our support team.",
                    HttpStatusCode.Forbidden,
                    null,
                    new JObject
                    {
                                    { "folderName", folderName },
                                    { "oldtenantAlias", oldtenantAlias },
                                    { "newTenantAlias", newTenantAlias },
                    });

            public static Error UnableToRenameProductDirectory(string oldtenantAlias, string newTenantAlias, string folderName, string exceptionMessage) =>
                new Error(
                    "product.directory.unable.to.rename",
                    "Cannot rename product directory",
                    $"When renaming tenant alias from '{oldtenantAlias}' to '{newTenantAlias}', "
                    + $"We have found that the product directory '{folderName}/{oldtenantAlias}' cannot be renamed. "
                    + "Please make sure your not opening files inside the directory and avoid opening the directory itself, "
                    + "as that would contribute to directory being read only and unable to rename. "
                    + "If you need assistance please get in touch with our support team.",
                    HttpStatusCode.Forbidden,
                    null,
                    new JObject
                    {
                                    { "folderName", folderName },
                                    { "oldtenantAlias", oldtenantAlias },
                                    { "newTenantAlias", newTenantAlias },
                                    { "exception", exceptionMessage },
                    });
        }

        public static class Product
        {
            public static Error AliasIsNull(string alias) =>
                new Error(
                    "product.alias.cannot.be.the.word.null",
                    $"Product alias cannot be the word null",
                    $"The product alias cannot be the word \"{alias}\". Please enter a valid name for the product alias",
                    HttpStatusCode.BadRequest,
                    null,
                    new JObject()
                    {
                        { "alias", alias },
                    });

            public static Error Disabled(string productAlias) =>
                new Error(
                    "product.disabled",
                    "Product disabled",
                    $"Product \"{productAlias}\" has been disabled. Please contact customer support if you would like to enable this product again.",
                    HttpStatusCode.NotFound);

            public static Error Deleted(string productAlias) =>
                new Error(
                    "product.deleted",
                    "Product deleted",
                    $"Product \"{productAlias}\" was deleted. Please contact customer support if you would like to restore this product.",
                    HttpStatusCode.Gone);

            public static Error MisConfiguration(string message, List<string>? additionalDetails = null, JObject? data = null) =>
                new Error(
                    "product.misconfiguration",
                    "Product misconfiguration",
                    message,
                    additionalDetails: additionalDetails,
                    data: data);

            public static Error ProductDirectoryAlreadyExists(string tenantAlias, string oldProductAlias, string newProductAlias, string folderName) =>
                new Error(
                    "product.directory.already.exists",
                    "The product directory already exists",
                    $"When renaming product alias from '{oldProductAlias}' to '{newProductAlias}', "
                    + $"We have found that the product directory '{folderName}/{tenantAlias}/{newProductAlias}' already exist, "
                    + "please delete or rename the said product file directory. "
                    + "If you need assistance please get in touch with our support team.",
                    HttpStatusCode.Forbidden,
                    null,
                    new JObject
                    {
                                    { "folderName", folderName },
                                    { "tenantAlias", tenantAlias },
                                    { "oldProductAlias", oldProductAlias },
                                    { "newProductAlias", newProductAlias },
                    });

            public static Error UnableToRenameProductDirectory(string tenantAlias, string oldProductAlias, string newProductAlias, string folderName, string exceptionMessage) =>
                new Error(
                    "product.directory.unable.to.rename",
                    "Cannot rename product directory",
                    $"When renaming product alias from '{oldProductAlias}' to '{newProductAlias}', "
                    + $"We have found that the product directory '{folderName}/{tenantAlias}/{oldProductAlias}' cannot be renamed. "
                    + "Please make sure your not opening files inside the directory and avoid opening the directory itself, "
                    + "as that would contribute to directory being read only and unable to rename. "
                    + "If you need assistance please get in touch with our support team.",
                    HttpStatusCode.Forbidden,
                    null,
                    new JObject
                    {
                                    { "folderName", folderName },
                                    { "tenantAlias", tenantAlias },
                                    { "oldProductAlias", oldProductAlias },
                                    { "newProductAlias", newProductAlias },
                                    { "exception", exceptionMessage },
                    });

            public static Error ProductComponentFolderDoesNotExist(
                string productAlias,
                WebFormAppType componentType,
                JObject? errorData = null) =>
                new Error(
                    "product.synchronisation.no.folder.exists",
                    "Nothing to synchronise",
                    $"When trying to synchronise your product {productAlias}, there was no {componentType.Humanize()} "
                    + $"folder found. Please copy a template set of files for into the {componentType.Humanize()} "
                    + $"folder for this product. If you need assistance creating a new product, please get in touch "
                    + "with our support team.",
                    HttpStatusCode.NotFound,
                    null,
                    errorData);

            public static Error ProductComponentFolderDoesNotExist(
                string productAlias,
                JObject? errorData = null) =>
                new Error(
                    "product.synchronisation.no.quote.or.claim.folder.exists",
                    "Nothing to synchronise",
                    $"When trying to synchronise your product {productAlias}, there was no Quote or Claim "
                    + $"folder found. Please copy a template set of files for into the Quote or Claim "
                    + $"folder for this product. If you need assistance creating a new product, please get in touch "
                    + "with our support team.",
                    HttpStatusCode.NotFound,
                    null,
                    errorData);

            public static Error WorkbookNotFound(string tenantAlias, string productAlias, string webformType) =>
                new Error(
                    "workbook.not.found",
                    "Workbook not found",
                    $"Could not find file \"{webformType}/{tenantAlias}-{productAlias}-Workbook.xlsx\". "
                    + "If this was a result of an alias change for either of the tenant or product, "
                    + "please rename the workbook file to the proper name. If you need assistance creating a new "
                    + "product, please get in touch with our support team.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject
                    {
                        { "tenantAlias", tenantAlias },
                        { "productAlias", productAlias },
                    });

            public static Error ProductSynchronisationInProgress(string tenantAlias, string productAlias) =>
                new Error(
                    "product.synchronisation.in.progress",
                    $"Product synchronisation is in progress",
                    $"Could not synchronise the product {productAlias} at the moment as synchronisation is in progress. "
                    + "Please wait until the synchronisation is complete. "
                    + "If you need assistance with synchronisation of a "
                    + "product, please get in touch with our support team.",
                    HttpStatusCode.Conflict,
                    null,
                    new JObject
                    {
                        { "tenantAlias", tenantAlias },
                        { "productAlias", productAlias },
                    });

            public static Error NotFound(string productId) =>
                new Error(
                    "product.not.found",
                    $"Product not found",
                    $"When trying to find the product '{productId}', nothing came up. Please ensure that you are passing the correct ID "
                    + "or contact customer support if you are experiencing this error in the portal.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                                    { "productId", productId },
                    });

            public static Error AliasNotFound(string alias) =>
                new Error(
                    "product.alias.not.found",
                    $"Product not found",
                    $"When trying to find a product with the alias \"{alias}\", nothing came up. Please take note that product aliases are case sensitive, and the product must be been deployed to the designated environment." +
                    " If you need assistance with this, please don't hesitate to get in touch with customer support.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                         { "alias", alias },
                    });

            public static Error WorkbookParseFailure(string reason, IEnumerable<string>? additionDetails = null) =>
                new Error(
                    "product.component.workbook.parse.failure",
                    "There was a problem reading data from the workbook",
                    "When trying to synchronise, we ran into a problem trying to parse the data in the workbook. "
                    + (reason != null ? reason + " " : string.Empty)
                    + "Please ensure your workbook has the correct structure and is up to date.",
                    HttpStatusCode.ExpectationFailed,
                    additionDetails);

            public static Error NotEnabledForPortal(string productName, string portalName) =>
                new Error(
                    "product.not.enabled.for.portal",
                    "Product not enabled",
                    $"The product \"{productName}\" has not been enabled for the portal \"{portalName}\".",
                    HttpStatusCode.Forbidden);

            public static Error NotEnabledForOrganisation(string product, string organisation) =>
                new Error(
                    "product.not.enabled.for.organisation",
                    "Product not enabled",
                    $"The product \"{product}\" has not been enabled for organisation \"{organisation}\". " +
                    "Please check the Organisation Settings to enable the Allow New Quotes for Products. " +
                    "If you think this is bug, please contact customer support.",
                    HttpStatusCode.Forbidden,
                    null,
                    new JObject
                        {
                            { "product", product },
                            { "organisation", organisation },
                        });

            public static class Asset
            {
                public static Error NotFound(string tenantAlias, string productAlias, DeploymentEnvironment environment, string assetName) =>
                    new Error(
                        "product.asset.not.found",
                        "Missing asset",
                        $"Could not find the asset \"{assetName}\" for tenant \"{tenantAlias}\", product \"{productAlias}\" and environment \"{environment}\". "
                        + "Please ensure the asset has been uploaded, is named correctly, and assets have been "
                        + "synchronised, and if relevent, the necessary release has been created.",
                        HttpStatusCode.NotFound);
            }

            public static class File
            {
                public static Error NotFound(
                    string tenantAlias,
                    string productAlias,
                    DeploymentEnvironment environment,
                    WebFormAppType webFormAppType,
                    FileVisibility visibility,
                    string fileName) =>
                    new Error(
                        "product.file.not.found",
                        "Missing product file",
                        $"Could not find the file \"{fileName}\" for tenant \"{tenantAlias}\", "
                        + $"product \"{productAlias}\", environment \"{environment}\", "
                        + $"web form app type \"{webFormAppType.Humanize()}\", and "
                        + $"visibility \"{visibility.Humanize()}\". "
                        + "Please ensure the file has been uploaded, is named correctly, and files have been "
                        + "synchronised, and if relevent, the necessary release has been created.",
                        HttpStatusCode.NotFound);

                public static Error NotFound(
                    string tenantAlias,
                    string productAlias,
                    Guid productReleaseId,
                    WebFormAppType webFormAppType,
                    string path) =>
                    new Error(
                        "product.file.not.found",
                        "We couldn't find that product file",
                        $"Could not find the file \"{path}\" for tenant \"{tenantAlias}\", "
                        + $"product \"{productAlias}\", product release ID \"{productReleaseId}\" and "
                        + $"web form app type \"{webFormAppType.Humanize()}\"."
                        + "Please ensure the file has been uploaded, is named correctly, and files have been "
                        + "synchronised, and if relevent, the necessary release has been created.",
                        HttpStatusCode.NotFound);
            }

            public static class Component
            {
                public static Error NotFound(IProductContext productContext, WebFormAppType webFormAppType) =>
                    new Error(
                        "product.component.not.found",
                        $"A {webFormAppType} component was not found",
                        $"Could not find a {webFormAppType} component for the product {productContext.ProductId} in "
                        + $"the {productContext.Environment} environment. "
                        + "Please ensure that you have created this component for your product and deployed it to the "
                        + $"{productContext.Environment} before trying to access it.",
                        HttpStatusCode.NotFound,
                        null,
                        new JObject
                        {
                            { "tenantId", productContext.TenantId },
                            { "productId", productContext.ProductId },
                            { "environment", productContext.Environment.ToString() },
                            { "webFormAppType", webFormAppType.ToString() },
                        });
            }
        }

        public static class Portal
        {
            public static Error AliasIsNull(string alias) =>
                new Error(
                    "portal.alias.cannot.be.the.word.null",
                    $"Portal alias cannot be the word null",
                    $"The portal alias cannot be the word \"{alias}\". Please enter a valid name for the portal alias",
                    HttpStatusCode.BadRequest,
                    null,
                    new JObject()
                    {
                        { "alias", alias },
                    });

            public static Error NotFound() =>
                new Error(
                    "portal.not.found",
                    "Portal not found",
                    "The provided portal Id is either null, empty or non-existing in the record. Please ensure that you are passing the correct Id or contact customer support if you are experiencing this error in the portal.",
                    HttpStatusCode.NotFound);

            public static Error NotFound(Guid tenantId, Guid portalId) =>
                new Error(
                    "portal.not.found",
                    "Portal not found",
                    $"Could not find portal with ID \"{portalId}\" for tenant with ID \"{tenantId}\". "
                    + "Please check you have specified a correct portal alias and the portal has been created for the specified tenant.",
                    HttpStatusCode.NotFound);

            public static Error NotFound(Guid tenantId, string portalAlias) =>
                new Error(
                    "portal.not.found",
                    "Portal not found",
                    $"Could not find portal \"{portalAlias}\" for tenant with ID \"{tenantId}\". "
                    + "Please check you have specified a correct portal alias and the portal has been created for the specified tenant.",
                    HttpStatusCode.NotFound);

            public static Error NotFound(string tenantAlias, string portalIdOrAlias) =>
                new Error(
                    "portal.not.found",
                    "Portal not found",
                    $"Could not find portal \"{portalIdOrAlias}\" for tenant \"{tenantAlias}\". "
                    + "Please check you have specified a correct portal alias and the portal has been created for the specified tenant.",
                    HttpStatusCode.NotFound);

            public static Error NotFound(string tenantAlias, Guid portalId) =>
                new Error(
                    "portal.not.found",
                    "Portal not found",
                    $"Could not find portal \"{portalId}\" for tenant \"{tenantAlias}\". "
                    + "Please check you have specified a correct portal and the portal has been created for the specified tenant.",
                    HttpStatusCode.NotFound);

            public static Error UrlMismatch(string tenantAlias, Guid? portalId, string url) =>
                new Error(
                    "portal.url.mismatch",
                    "Portal URL mismatch",
                    $"The url {url} does not match any of the urls allowed for the portal \"{portalId}\" in tenant \"{tenantAlias}\". "
                    + "Please ensure the portal url is whitelisted in the deployment settings for the portal.",
                    HttpStatusCode.Unauthorized);

            public static Error CannotAccessPortalFromAnotherOrganisation(string tenantAlias, string portalAlias) =>
                new Error(
                    "cannot.access.portal.from.another.organisation",
                    "You can't access that portal",
                    $"You've attempted to access a portal from another organisation, however you're not authorised to "
                    + $"access the portal {portalAlias} within the {tenantAlias} tenancy. Please ensure you specify a "
                    + "portal from your own organisation, or one which has been configured for use by your "
                    + "organisation.",
                    HttpStatusCode.Forbidden);

            public static Error NoLocationConfigured(string portalAlias, string portalName) =>
                new Error(
                    "portal.no.location.configured",
                    "You need to configure a location for the portal",
                    $"When trying to get the deployment target for the portal \"{portalName}\", we found that "
                    + "none have been configured. You must define at least one deployment target so that links "
                    + "can be generated to allow users to activate their account, reset their password and so forth.",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    new JObject
                    {
                        { "portalAlias", portalAlias },
                    });

            public static Error OrganisationMismatch(
                string portalAlias,
                string portalOrganisationAlias,
                string specifiedOrganisationAlias) =>
                new Error(
                    "portal.organisation.mismatch",
                    "You've specified the wrong organisation for that portal",
                    $"The portal \"{portalAlias}\" is associated with the organisation \"{portalOrganisationAlias}\" "
                    + $"however you have specified the organisation \"{specifiedOrganisationAlias}\". Please ensure "
                    + "you specify the correct organisation, or leave out the organisation to have it automatically "
                    + "selected when specify a portal.",
                    HttpStatusCode.Conflict,
                    null,
                    new JObject
                    {
                        { "portalAlias", portalAlias },
                        { "portalOrganisationAlias", portalOrganisationAlias },
                        { "specifiedOrganisationAlias", specifiedOrganisationAlias },
                    });

            public static Error AlreadyDefaultForOrganisation(string portalName, string organisationName) =>
                new Error(
                    "portal.already.default.for.organisation",
                    "That portal is already the default for the organisation",
                    "The portal \"{portalName}\" is already the default portal for the organisation \"{organisationName}\"",
                    HttpStatusCode.PreconditionFailed);

            public static Error CannotUnsetAsDefaultWithoutSettingAnotherAsDefaultFirst(
                string portalName,
                string organisationName,
                string tenantName) =>
                new Error(
                    "portal.cannot.unset.as.default.when.set.as.default.for.tenant",
                    "You'll need to set another portal as the default instead",
                    $"You were trying to unset the portal \"{portalName}\" as the default for the organisation "
                    + $"\"{organisationName}\" which is the default organisation for the tenant \"{tenantName}\". "
                    + "You'll need to set another portal as the default for the organisation before you can unset "
                    + "this one, to ensure that there is a way to login and administer the tenancy.",
                    HttpStatusCode.PreconditionFailed);

            public static Error CannotSetDefaultPortalForTenantToCustomerPortal(
                string portalName,
                string organisationName,
                string tenantName) =>
                new Error(
                    "portal.cannot.set.default.portal.for.tenant.to.customer.portal",
                    "You can't set a customer portal as the tenant default",
                    "You're trying to set a customer portal to the default portal for the organisation "
                    + $"\"{organisationName}\", however this organisation is the default organisation for "
                    + "the tenant. For this reason, the default portal must be an agent portal, not a customer "
                    + "portal.",
                    HttpStatusCode.Conflict);

            public static Error CannotDeleteDefaultPortalForTenant(
                string portalName,
                string organisationName,
                string tenantName) =>
                new Error(
                    "portal.cannot.delete.default.portal.for.tenant",
                    "You can't delete the default portal for a tenant",
                    $"You're trying to delete the portal \"{portalName}\" which is the default portal for the "
                    + $"organisation \"{organisationName}\", and that organisation is the default organisation "
                    + $"for the tenant \"{tenantName}\", making this portal the default portal for the tenant. "
                    + "if you wish to delete this portal, please make sure another portal is set to be the default "
                    + "portal first.",
                    HttpStatusCode.Conflict);

            public static Error CannotDisableDefaultPortalForTenant(
                string portalName,
                string organisationName,
                string tenantName) =>
                new Error(
                    "portal.cannot.disable.default.portal.for.tenant",
                    "You can't disable the default portal for a tenant",
                    $"You're trying to disable the portal \"{portalName}\" which is the default portal for the "
                    + $"organisation \"{organisationName}\", and that organisation is the default organisation "
                    + $"for the tenant \"{tenantName}\", making this portal the default portal for the tenant. "
                    + "if you wish to disable this portal, please make sure another portal is set to be the default "
                    + "portal first.",
                    HttpStatusCode.Conflict);

            public static Error NoDefaultPortalExists(string organisationName, PortalUserType userType) =>
                new Error(
                    "portal.no.default.portal.exists",
                    "No default portal exists",
                    $"There is no default portal for user type \"{userType.Humanize()}\" in the organisation "
                    + $"{organisationName}\", or in the default organisation either. Please get in touch with "
                    + "customer support so we can configure the correct location for you to log into.",
                    HttpStatusCode.NotFound);

            public static Error CannotSetDisabledPortalAsDefault(
                string portalName,
                string organisationName) =>
                new Error(
                    "portal.cannot.set.disabled.portal.as.default",
                    "You can't make a disabled portal a default portal",
                    $"You're trying to make the portal \"{portalName}\" the default portal for the organisation "
                    + $"\"{organisationName}\", however this portal is disabled. "
                    + "Please enable the portal first, then you can set it as default.",
                    HttpStatusCode.PreconditionFailed);

            public static Error SelfRegistrationNotAllowed(
                string portalName,
                string organisationName) =>
                new Error(
                    "portal.does.not.allow.self.registration",
                    "This portal does not allow self-registration",
                    $"You're trying to self-register using the \"{portalName}\" of the \"{organisationName}\" organisation, "
                    + $"however self-registration is currently disabled on this portal. To enable self-registration, "
                    + "please enable the \"Allow Creation from Login Page\" setting in the Portal Settings Page.",
                    HttpStatusCode.PreconditionFailed);
        }

        public static class File
        {
            public static Error NotFound(string filename, string location) =>
                new Error(
                    "file.not.found",
                    "File not found",
                    $"Could not find the file \"{filename}\" in the location \"{location}\"",
                    HttpStatusCode.NotFound);

            public static Error FileNameInvalid(List<string>? additionalDetails, JObject? errorData = null) =>
              new Error(
                  "file.name.invalid",
                  "File must have a valid filename.",
                  $"You have attempted to set the filename of the file to a value that is not a valid filename",
                  HttpStatusCode.InternalServerError,
                  additionalDetails,
                  errorData);

            public static Error TemporaryFileNotFoundWhileAttemptingToApplyDataToDocumentTemplate(
                string filePath)
            {
                return new Error(
                    "temporary.file.not.found",
                    "Temporary file does not exists",
                    "The creation of temporary file which contains the data of the source template had failed. It is "
                    + "possible that the server rejected the request of the system because it doesn't have enough "
                    + "security access. Please contact the customer support to check the system's access rights.",
                    HttpStatusCode.InternalServerError,
                    new List<string>
                    {
                        { $"Temporary file : {filePath}" },
                    },
                    new JObject
                    {
                        { "temporary file", filePath },
                    });
            }
        }

        public static class Release
        {
            public static Error DefaultReleaseNotFound(string tenantAlias, string productAlias, DeploymentEnvironment environment, string? productName = null) =>
                new Error(
                    "default.release.not.found",
                    "Release not found",
                    $"Could not find a default product release for tenant \"{tenantAlias}\", product \"{productAlias}\" in environment \"{environment}\". "
                    + (environment == DeploymentEnvironment.Development ?
                        "Please ensure you have synchronised the product assets." :
                        $"Please ensure you have set a default product release for the \"{environment}\" environment."),
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "tenantAlias", tenantAlias },
                        { "productAlias", tenantAlias },
                        { "productName", productName },
                        { "environment", environment.ToString() },
                    });

            public static Error NotFound(string tenantAlias, string productAlias, Guid releaseId) =>
                new Error(
                    "release.not.found",
                    "Release not found",
                    $"Could not find a release for tenant \"{tenantAlias}\"and product \"{productAlias}\" "
                        + $"with the ID \"{releaseId.ToString()}\".",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "tenantAlias", tenantAlias },
                        { "productAlias", tenantAlias },
                        { "releaseId", releaseId.ToString() },
                    });

            public static Error ReleaseNumberNotFound(string tenantAlias, string productAlias, string releaseNumber) =>
                new Error(
                    "release.number.not.found",
                    $"Release number not found",
                    $"Could not find a product release for tenant \"{tenantAlias}\" and product \"{productAlias}\" with product release number \"{releaseNumber}\". " +
                    $"Please ensure you have set a valid product release number for the productRelease argument, or omit the productRelease and a product release will automatically be selected based on the product settings.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "releaseNumber", releaseNumber },
                    });

            public static Error NumberInvalid(string releaseNumber) =>
                new Error(
                    "release.number.invalid",
                    $"That's not a valid release number",
                    $"The release number \"{releaseNumber}\" is not a valid release number. "
                    + "Release numbers have the format \"major.minor?.patch?\". For example, valid release numbers could be "
                    + "\"3\", \"3.1\", or \"3.1.4\". Please check the release you wish to use and copy the exact release number.",
                    HttpStatusCode.BadRequest,
                    null,
                    new JObject()
                    {
                        { "releaseNumber", releaseNumber },
                    });

            public static Error NumberInvalid(string tenantAlias, string productAlias, string releaseNumber) =>
                new Error(
                    "release.number.invalid",
                    $"That's not a valid release number",
                    $"Could not find a product release for tenant \"{tenantAlias}\" and product \"{productAlias}\", "
                    + $"because the release number \"{releaseNumber}\" is not a valid release number. "
                    + "Release numbers have the format \"major.minor?.patch?\". For example, valid release numbers could be "
                    + "\"3\", \"3.1\", or \"3.1.4\". Please check the release you wish to use and copy the exact release number.",
                    HttpStatusCode.BadRequest,
                    null,
                    new JObject()
                    {
                        { "releaseNumber", releaseNumber },
                    });

            public static Error ProductReleaseCannotBeSpecified(string quoteType) =>
                new Error(
                    "product.release.cannot.be.specified",
                    $"Product release cannot be specified",
                    $"A product release cannot be specified when creating quotes on the production environment. The product release must be determined based on quote type and the applicable product settings.",
                    HttpStatusCode.Forbidden,
                    null,
                    new JObject()
                    {
                        { "quoteType", quoteType },
                    });

            public static Error CannotGetBecauseInitialising(string tenantAlias, string productAlias, DeploymentEnvironment environment) =>
                new Error(
                    "release.initialising.cannot.get",
                    "Release initialising",
                    $"Could not get the release for tenant \"{tenantAlias}\", product \"{productAlias}\" in environment \"{environment}\" because it is currently initialising. "
                    + "Please try again shortly.",
                    HttpStatusCode.UnprocessableEntity);

            public static Error InitialisationFailed(
                string tenantAlias,
                string productAlias,
                DeploymentEnvironment environment,
                IEnumerable<string>? additionalDetails = null) =>
                new Error(
                    "release.initialisation.failed",
                    "Release initialisation failed",
                    $"Could not get the release for tenant \"{tenantAlias}\", product \"{productAlias}\" in environment \"{environment}\" "
                    + "because initialisation failed whilst synchronising product assets. Please try again.",
                    HttpStatusCode.UnprocessableEntity,
                    additionalDetails);

            public static Error CannotCreateBecauseProductInitialising(string productAlias) =>
                new Error(
                    "release.cannot.create.product.initialising",
                    "Product still initialising",
                    $"You can't quite create a release for product \"{productAlias}\" just yet because the product is still initialising. Shouldn't be too much longer.",
                    HttpStatusCode.PreconditionFailed);

            public static Error AssetsNotSynchronisedWhenCreatingRelease(string tenantAlias, string productAlias) =>
                new Error(
                    "create.release.assets.not.synchronised",
                    "Assets not synchronised",
                    $"The creation of a new release for the product \"{productAlias}\" failed because the assets for this product "
                    + "have never been synchronised. Please synchronise product assets before creating a release.",
                    HttpStatusCode.PreconditionFailed);

            public static Error AssetsNotSynchronised(string tenantAlias, string productAlias) =>
                new Error(
                    "release.assets.not.synchronised",
                    "Assets not synchronised",
                    $"We were unable to obtain details for the development assets of the product \"{productAlias}\" "
                    + "because assets have never been synchronised.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "tenantAlias", tenantAlias },
                        { "productAlias", tenantAlias },
                    });
        }

        public static class Deployment
        {
            public static Error NotFound(string tenantAlias, string productAlias, DeploymentEnvironment environment) =>
                new Error(
                    "deployment.not.found",
                    "Deployment not found",
                    $"Could not find deployment for tenant \"{tenantAlias}\", product \"{productAlias}\" in environment \"{environment}\". "
                    + (environment == DeploymentEnvironment.Development ?
                        "Please ensure you have synchronised the product assets." :
                        $"Please ensure you have created a release and deployed it to the {environment} environment."),
                    HttpStatusCode.NotFound);
        }

        public static class Forms
        {
            public static Error TestModeEnvironmentMismatch(DeploymentEnvironment environment) =>
                new Error(
                    "test.mode.environment.mismatch",
                    "Test mode not available for environment",
                    "You have specified for your form fills to be done in test mode where data is recorded as test data, "
                    + $"however this mode is only available for use in the Production environment and you have specified the {environment} environment. "
                    + "Please specify the production environment, or remove test mode from your request.",
                    HttpStatusCode.Conflict);
        }

        public static class Integrations
        {
            public static Error DuplicateIds(IEnumerable<string> duplicateIds, string integrationsType) =>
                new Error(
                    "integrations.duplicate.ids",
                    $"Duplicate ids in {integrationsType} integrations configuration",
                    $"There were duplicate ids found in the {integrationsType} integrations configuration. Please review the configuration and ensure each id is unique.",
                    HttpStatusCode.Conflict,
                    duplicateIds);

            public static Error IllegalProviderValue(string providerValue) =>
                new Error(
                    "integration.configuration.illegal.provider.value",
                    $"Cannot parse {providerValue}",
                    $"'{providerValue}' is an illegal value and cannot be parsed. Please review the configuration and ensure the provider is valid.");

            public static Error NoIntegrationsConfiguration(string productAlias) => new Error(
                "integrations.configuration.not.found",
                "No integrations configuration found",
                $"No integrations configuration found for product '{productAlias}.",
                HttpStatusCode.NotFound);
        }

        public static class Report
        {
            public static Error MimeTypeNotSupported(string mimeType) =>
                new Error(
                    "report.mime.type.not.supported",
                    "Mime type not supported",
                    $"The mime type {mimeType} is not yet supported by the system. We are "
                    + "unable to generate a report in this format. Please get in touch with "
                    + "customer support if you would like to request this feature.",
                    HttpStatusCode.UnprocessableEntity);

            public static Error NotFound(Guid reportId)
                => new Error(
                    "report.not.found",
                    $"We could not find the report for id '{reportId}'",
                    $"When trying to find a report with an id of '{reportId}', nothing came up. Please check you've entered the correct details. If you think this is bug, please contact customer support.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "reportId", reportId },
                    });

            public static Error DetailsNotFound(Guid reportDetailsId)
                => new Error(
                    "report.details.not.found",
                    $"We could not find the report details for id '{reportDetailsId}'",
                    $"When trying to find a report details with an id of '{reportDetailsId}', nothing came up. Please check you've entered the correct details. If you think this is bug, please contact customer support.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "reportDetailsId", reportDetailsId },
                    });

            public static Error FileNotFound(Guid reportFileId)
                => new Error(
                    "report.file.not.found",
                    $"We could not find report file for id '{reportFileId}'",
                    $"When trying to find a report file with an id of '{reportFileId}', nothing came up. Please check you've entered the correct details. If you think this is bug, please contact customer support.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "reportFileId", reportFileId },
                    });
        }

        public static class NumberPool
        {
            public static Error NoneAvailable(string tenantId, string productId, string poolName)
            {
                return new Error(
                    "number.pool.none.available",
                    "We've run out of numbers",
                    $"When trying to get a {poolName} number, we found that there were none available. "
                    + "New numbers need to be loaded into the system. Please contact customer support.",
                    HttpStatusCode.Conflict,
                    new List<string>
                    {
                        { $"Tenant Id : {tenantId}" },
                        { $"Product Id : {productId}" },
                    });
            }
        }

        public static class Accounting
        {
            public static Error AccountTransactionDoesNotExist(Guid id, string typeName, List<string> additionalDetails, JObject errorData) =>
                new Error(
                    $"accounting.{typeName.ToLower()}.does.not.exist",
                    $"{typeName} Does Not Exist",
                    $"We tried to retrieve the {typeName.ToLower()} with the Id: {id}, but it was not found. " +
                    $"If you think this is bug, we apologise for the inconvenience, and please get in touch with customer support.",
                    HttpStatusCode.NotFound,
                    additionalDetails,
                    errorData);

            public static Error PaymentCannotBeNegative(decimal amount, List<string> additionalDetails, JObject errorData) =>
                new Error(
                    $"accounting.payment.amount.cannot.be.negative",
                    $"Payment Amount Cannot Be Negative in Value",
                    $"We were trying to post a payment of {amount}, but the amount cannot be accepted because it is less than zero. " +
                    $"Please provide a value more than zero, or make a refund instead. If you think this is bug, we apologise for the inconvenience, and please get in touch with customer support.",
                    HttpStatusCode.NotAcceptable,
                    additionalDetails,
                    errorData);

            public static Error RefundCannotBeNegative(decimal amount, List<string> additionalDetails, JObject errorData) =>
                new Error(
                    $"accounting.refund.amount.cannot.be.negative",
                    $"Refund Amount Cannot Be Negative in Value",
                    $"We were trying to post a refund of {amount}, but the amount cannot be accepted because it is less than zero. " +
                    $"Please provide a value more than zero, or make a payment instead. If you think this is bug, we apologise for the inconvenience, and please get in touch with customer support.",
                    HttpStatusCode.NotAcceptable,
                    additionalDetails,
                    errorData);

            public static Error PaymentCannotBeZero(List<string> additionalDetails, JObject errorData) =>
                new Error(
                    $"accounting.payment.amount.cannot.be.zero",
                    $"Payment Amount Cannot Be Zero in Value",
                    $"We were trying to post a payment but the amount is zero. Please enter a value more than zero. " +
                    $"If you think this is bug, we apologise for the inconvenience, and please get in touch with customer support.",
                    HttpStatusCode.BadRequest,
                    additionalDetails,
                    errorData);

            public static Error RefundCannotBeZero(List<string> additionalDetails, JObject errorData) =>
                new Error(
                    $"accounting.refund.amount.cannot.be.zero",
                    $"Refund Amount Cannot Be Zero in Value",
                    $"We were trying to post a refund but the amount is zero. Please enter a value more than zero. " +
                    $"If you think this is bug, we apologise for the inconvenience, and please get in touch with customer support.",
                    HttpStatusCode.NotAcceptable,
                    additionalDetails,
                    errorData);

            public static Error TransactionPartiesNotFound(Guid id, List<string> additionalDetails, JObject errorData) =>
                new Error(
                    "accounting.transaction.parties.not.found",
                    "Transaction Parties Not Found",
                    $"We were trying to the assign participants of an accounting transaction, but the parties involved in the accounting transaction: {id} were not found. " +
                    $"If you think this is bug, we apologise for the inconvenience, and please get in touch with customer support.",
                    HttpStatusCode.NotFound,
                    additionalDetails,
                    errorData);

            public static Error TransactionDateTimeCannotBeInFuture(Instant transactionTime, string typeName, List<string> additionalDetails, JObject errorData) =>
                new Error(
                    "accounting.transaction.datetime.cannot.be.in.future.",
                    "Transaction Time Cannot Be In Future",
                    $"We were trying to post a {typeName}, but the transaction time: {transactionTime} was not accepted because it is in the future." +
                    $"Please provide a date and time in the past. If you think this is bug, we apologise for the inconvenience, and please get in touch with customer support.",
                    HttpStatusCode.NotAcceptable,
                    additionalDetails,
                    errorData);
        }

        public static class Account
        {
            public static Error StaleAuthenticationData(string? reason = null) => new Error(
                "account.stale.authentication.data",
                "Stale authentication data",
                "Your authentication data has become stale and you need to sign in again."
                + (reason != null ? $" {reason}" : string.Empty),
                HttpStatusCode.Unauthorized);

            public static Error NotLoggedInAndTenantNotSpecified() => new Error(
                "not.logged.in.and.tenant.not.specified",
                "You need to specify the tenant",
                "An attempt was made to access an API endpoint that requires either a logged in user or the tenant to "
                + "be specified by way of a query string parameter (e.g.: ?tenant=abc-insurance). Please ensure you "
                + "access this endpoint with the tenant parameter, or by signing-in first.");
        }

        public static class StartupJob
        {
            public static Error MethodNameNotFound(string methodName) => new Error(
                "startupjob.method.name.not.found",
                $"We couldn't find the method '{methodName}'.",
                $"When trying to execute '{methodName}', it does not exists in the StartupJob methods. Please check the method name saved in the database.",
                HttpStatusCode.NotFound,
                null,
                new JObject()
                {
                    { "methodName", methodName },
                });

            public static Error StartupJobIsAlreadyComplete(string methodAlias) => new Error(
                "startupjob.is.already.completed",
                $"{methodAlias} has already been completed.",
                $"When trying to execute {methodAlias}, it has been confirmed that the startup job has been marked completed." +
                "Please check the name of the startup job the database.",
                HttpStatusCode.BadRequest,
                null,
                new JObject()
                {
                    { "methodName", methodAlias },
                });

            public static Error MethodNameIsRequired() => new Error(
                "startupjob.method.name.is.required",
                "The name of the method to run is required.",
                "Please include the name of the startup job to execute.",
                HttpStatusCode.PreconditionRequired);

            public static Error StartupJobHasAlreadyStarted(string startupJobAlias, string hangfireJobId) => new Error(
                "startupjob.has.already.started",
                $"{startupJobAlias} has already start.",
                $"When trying to execute the startup job \"{startupJobAlias}\", it was found to have already started. "
                + $"The job ran as a hangfire job with ID {hangfireJobId}. Please check hangfire to know it's status. "
                + "If you want to force it to run again, you can pass the \"force\" parameter.",
                HttpStatusCode.BadRequest,
                null,
                new JObject()
                {
                    { "startupJobAlias", startupJobAlias },
                });

            public static Error PrecedingJobsNotComplete(string startupJobAlias, IEnumerable<string> precedingJobAliases, IEnumerable<string> notCompletedJobAliases) => new Error(
                "preceding.startup.jobs.not.completed",
                "There are preceding startup jobs not completed",
                $"When trying to run the startup job {startupJobAlias}, it was configured to require certain "
                + "startup jobs to be completed before it could run, and one or more of those startup jobs was "
                + "found to have not completed.",
                HttpStatusCode.BadRequest,
                new List<string>
                {
                    "Preceding Job Aliases: " + string.Join(", ", precedingJobAliases),
                    "Not Completed Job Aliases: " + string.Join(", ", notCompletedJobAliases),
                },
                new JObject
                {
                    { "precedingJobAliases", new JArray(precedingJobAliases) },
                    { "notCompletedJobAliases", new JArray(notCompletedJobAliases) },
                });

            public static Error StartupJobNotFound(string startupJobAlias) => new Error(
            "startupjob.not.found",
                $"We couldn't find the startup job '{startupJobAlias}'.",
                $"When trying to get the status of the startup job with an alias \"{startupJobAlias}\", the attempt failed because the" +
                " specified alias could not be matched against one of the startup jobs in the system." +
                " We would appreciate you reporting this error so that it can be addressed." +
                " Please get in touch with customer support.",
                HttpStatusCode.NotFound,
                null,
                new JObject()
                {
                    { "startupJobAlias", startupJobAlias },
                });
        }

        public static class ApplicationDocumentField
        {
            public static Error TranslationOfFieldIntoDataFailure(JObject data, string template) =>
                new Error(
                    "application.document.translation.field.into.error",
                    "A field of applicaton document has error",
                    "There was an unexpected error when trying to translate a field into a data which is "
                    + $"defined in the  source template '{template}'. Please check this template for possible error, "
                    + "correct it if you found one then product synch it. If the problem still persists, please do not "
                    + "hesitate to contact the customer support.",
                    HttpStatusCode.BadRequest,
                    null,
                    data);
        }

        public static class AbnLookup
        {
            public static Error AbnNotFound(string abn) =>
                new Error(
                    "abr.abn.registration.not.found",
                    "ABN registration not found",
                    $"The ABN provided as part of your request did not match an ABN registration in the ABR dataset.",
                    HttpStatusCode.NotFound,
                    new[] { $"ABN provided: {abn}" },
                    null);

            public static Error InvalidAbn(string abn) =>
                new Error(
                    "abr.invalid.abn",
                    "Not a valid ABN",
                    $"The ABN provided as part of your request is not a valid ABN. " +
                    "A valid Australian ABN must contain 11 numerical digits starting with a digit other than zero.",
                    HttpStatusCode.BadRequest,
                    new[] { $"ABN provided: {abn}" },
                    null);

            public static Error InvalidSearch(string abn, string errorMessage) =>
                new Error(
                    "abr.invalid.search",
                    "Invalid search",
                    errorMessage,
                    HttpStatusCode.BadRequest,
                    new[] { $"ABN provided: {abn}" },
                    null);

            public static Error NoNametypesIncludedInSearch() =>
                new Error(
                    "abr.no.nametypes.included.in.search",
                    "No name types included in search",
                    "You must include at least one name type in the search, or no matches can be generated. " +
                    "Please set at least one of the following parameters to true to resolve this problem: " +
                    "includeEntityNames, includeBusinessNames, includeTradingNames",
                    HttpStatusCode.BadRequest,
                    null,
                    null);
        }

        public static class DataLocators
        {
            public static Error ParseFailure(QuoteDatumLocation location, Type type, string reason) =>
                new Error(
                    "data.locator.parse.failure",
                    "Data was not in the correct format",
                    $"When trying to locate a datum within the {location.Object.Humanize()} at that path "
                    + $"\"{location.Path}\", we found data at this location, however when it was being parsed "
                    + $"as a {type} we ran into a problem because the data was not in the correct format. "
                    + (!string.IsNullOrEmpty(reason) ? reason : string.Empty)
                    + "This is a product configuration issue, which a product developer needs to fix. "
                    + "We would appreciate you reporting this error so that it can be addressed. Please get in touch "
                    + "with customer support.",
                    HttpStatusCode.NotAcceptable,
                    null,
                    new JObject
                    {
                        { "object", location.Object.Humanize() },
                        { "path", location.Path },
                        { "type", type.ToString() },
                    });
        }

        public static class DataImports
        {
            public static Error FileSizeLimitExceeded() =>
                new Error(
                    "import.file.size.exceeded",
                    "File size uploaded exceeded limit of 5MB",
                    "The file you uploaded for data imports has exceeded the maximum file size accepted. Please choose another file or " +
                    "update file to conform with size limit requirement",
                    HttpStatusCode.RequestEntityTooLarge,
                    null,
                    null);

            public static Error FileTypeUnsupported(string fileName, string importType) =>
                new Error(
                    "import.file.type.unsupported",
                    "File type is not supported for data importation",
                    "The file you uploaded for data imports is currently not supported. Please "
                    + $"use CSV files only. File name: {fileName}",
                    HttpStatusCode.UnsupportedMediaType,
                    null,
                    new JObject
                    {
                        { "importType", importType },
                        { "fileName", fileName },
                    });

            public static Error InvalidDataStructure() =>
                new Error(
                    "import.file.has.invalid.content",
                    "File content has invalid data",
                    "The content of the file for data import has incorrect or invalid structured data. Please check the contents of " +
                    "the file and try again.",
                    HttpStatusCode.NotAcceptable,
                    null,
                    null);
        }

        public static class DkimSetting
        {
            public static Error InvalidDomainName(Guid tenantId, string tenantName, Guid organisationId, string organisationName, string errorDetail)
            {
                var additionalDetails = new List<string>
                {
                    { $"Error Details: {errorDetail}" },
                    { $"Tenant ID: {tenantId}" },
                    { $"Organisation ID: {organisationId}" },
                };

                var errorData = new JObject
                {
                    { "tenantId", tenantId },
                    { "organisationId", organisationId },
                };

                return new Error(
                    "invalid.dkim.settings.domain.name",
                    $"Invalid DKIM settings domain name",
                    $"When trying to sign an email with DKIM under tenant \"{tenantName}\" and organisation \"{organisationName}\", "
                    + $"we found that the domain name of your DKIM settings is invalid."
                    + $" Please review your DKIM settings to ensure that the domain name is valid."
                    + " If you need assistance, please don't hesitate to contact customer suppport.",
                    HttpStatusCode.BadRequest,
                    additionalDetails,
                    errorData);
            }

            public static Error InvalidDnsSelector(Guid tenantId, string tenantName, Guid organisationId, string organisationName, string errorDetail)
            {
                var additionalDetails = new List<string>
                {
                    { $"Error Details: {errorDetail}" },
                    { $"Tenant ID: {tenantId}" },
                    { $"Organisation ID: {organisationId}" },
                };

                var errorData = new JObject
                {
                    { "tenantId", tenantId },
                    { "organisationId", organisationId },
                };

                return new Error(
                    "invalid.dkim.settings.dns.selector",
                    $"Invalid DKIM settings DNS selector",
                    $"When trying to sign an email with DKIM under tenant \"{tenantName}\" and organisation \"{organisationName}\", "
                    + $"we found that the DNS selector of your DKIM settings is invalid."
                    + $" Please review your DKIM settings to ensure that the DNS selector is valid."
                    + " If you need assistance, please don't hesitate to contact customer suppport.",
                    HttpStatusCode.BadRequest,
                    additionalDetails,
                    errorData);
            }

            public static Error InvalidPrivateKey(Guid tenantId, string tenantName, Guid organisationId, string organisationName, string errorDetail)
            {
                var additionalDetails = new List<string>
                {
                    { $"Error Details: {errorDetail}" },
                    { $"Tenant ID: {tenantId}" },
                    { $"Organisation ID: {organisationId}" },
                };

                var errorData = new JObject
                {
                    { "tenantId", tenantId },
                    { "organisationId", organisationId },
                };

                return new Error(
                    "invalid.dkim.settings.private.key",
                    $"Invalid DKIM settings private Key",
                    $"When trying to sign an email with DKIM under tenant \"{tenantName}\" and organisation \"{organisationName}\", "
                    + $"we found that the private key of your DKIM settings is invalid."
                    + $" Please review your DKIM settings to ensure that the private key is valid."
                    + " If you need assistance, please don't hesitate to contact customer suppport.",
                    HttpStatusCode.BadRequest,
                    additionalDetails,
                    errorData);
            }

            public static Error InvalidDKIMSettings(Guid tenantId, string tenantName, Guid organisationId, string organisationName, string errorDetail)
            {
                var additionalDetails = new List<string>
                {
                    { $"Error Details: {errorDetail}" },
                    { $"Tenant ID: {tenantId}" },
                    { $"Organisation ID: {organisationId}" },
                };

                var errorData = new JObject
                {
                    { "tenantId", tenantId },
                    { "organisationId", organisationId },
                };

                return new Error(
                    "invalid.dkim.settings",
                    $"Invalid DKIM settings",
                    $"When trying to sign an email with DKIM under tenant \"{tenantName}\" and organisation \"{organisationName}\", "
                    + $"we found that the DKIM settings under this organisation are invalid."
                    + $" Please review your DKIM settings to ensure that the domain name, private key and DNS selector are valid."
                    + " If you need assistance, please don't hesitate to contact customer suppport.",
                    HttpStatusCode.BadRequest,
                    additionalDetails,
                    errorData);
            }

            public static Error PublicKeyNotFound(Guid tenantId, string tenantName, Guid organisationId, string organisationName, string domainName, string selector)
            {
                var additionalDetails = new List<string>
                {
                    { $"Tenant ID: {tenantId}" },
                    { $"Organisation ID: {organisationId}" },
                    { $"Selector: {selector}" },
                };

                var errorData = new JObject
                {
                    { "tenantId", tenantId },
                    { "organisationId", organisationId },
                    { "selector", selector },
                };

                return new Error(
                    "public.key.not.found.",
                    $"Public key not found",
                    $"When trying to find the public key for signing an email with DKIM under tenant \"{tenantName}\" and organisation \"{organisationName}\" with domain name \"{domainName}\", "
                    + $"We found that the public key was not found."
                    + $" Please review your DKIM settings to ensure that domain name, private key and DNS selector are correct."
                    + " If you need assistance, please don't hesitate to contact customer suppport.",
                    HttpStatusCode.NotFound,
                    additionalDetails,
                    errorData);
            }
        }

        public static class AdditionalProperties
        {
            public static Error TenantNotFound(Guid tenantId, string entityType, string propertyName)
            {
                var details = new List<string>
                {
                    { $"Tenant id: {tenantId}" },
                    { $"Property name: {propertyName}" },
                    { $"Entity type: {entityType}" },
                };
                var errorData = new JObject
                {
                    { "tenantId", tenantId },
                    { "propertyName", propertyName },
                    { "entityType", entityType },
                };

                return new Error(
                    "additional.property.tenant.not.found",
                    $"Unable to find associated tenant",
                    $"When trying to save an additional property name '{propertyName}' under a tenant '{tenantId}', "
                    + $"we found that the tenant doesn't exists during validation. It is possible that the request "
                    + "is providing an incorrect tenant id. We apologise for the inconveniece. Please contact your "
                    + "customer support for assistance to check why the web form is sending an invalid data in the "
                    + "request.",
                    HttpStatusCode.NotFound,
                    details,
                    errorData);
            }

            public static Error OrganisationNotFound(Guid tenantId, Guid organisationId, string entityType, string propertyName)
            {
                var details = new List<string>
                {
                    { $"Organisation id: {organisationId}" },
                    { $"Tenant id: {tenantId}" },
                    { $"Property name: {propertyName}" },
                    { $"Entity type: {entityType}" },
                };
                var errorData = new JObject
                {
                    { "organisationId", organisationId },
                    { "tenantId", organisationId },
                    { "propertyName", propertyName },
                    { "entityType", entityType },
                };

                return new Error(
                    "additional.property.organisation.not.found",
                    $"Unable to find associated organisation",
                    $"When trying to save an additional property name '{propertyName}' under an organisation '{organisationId}' "
                    + $"associated with tenant '{tenantId}', we found that the organisation doesn't exists during "
                    + "validation. It is possible that the request is providing an incorrect organisation id. We "
                    + "apologise for the inconveniece. Please contact your customer support for assistance to check "
                    + "why the web form is sending an invalid data in the request.",
                    HttpStatusCode.NotFound,
                    details,
                    errorData);
            }

            public static Error DefinitionIdNotFound(Guid id, string actionDescription)
            {
                var details = new List<string>
                {
                    { $"Id:{id}" },
                };

                var errorData = new JObject
                {
                    { "id", id },
                };

                return new Error(
                    "additionalproperty.id.not.found",
                    "We couldn't find that additional property definition",
                    $"When trying to {actionDescription}, we found that the id received by the server from "
                    + "the request doesn't exist in the system which is why it failed during the pre or post "
                    + "validation. It is possible that the request is passing an incorrect id. We apologise for the "
                    + "inconvenience. Please contact your customer support to check why the web form is sending an "
                    + "invalid data in the request.",
                    HttpStatusCode.NotFound,
                    details,
                    errorData);
            }

            public static Error DefinitionNotFoundOnEntity(
                string propertyAlias,
                AdditionalPropertyEntityType entityType,
                string actionDescription,
                JObject errorData,
                List<string> additionalDetails) => new Error(
                    "additional.property.definition.not.found.on.entity",
                    "We couldn't find that additional property definition",
                    $"When trying to {actionDescription}, we found that there was no additional property definition "
                    + $"with the alias \"{propertyAlias}\" on the entity \"{entityType.Humanize()}\". "
                    + "If this is unexpected, please get in touch with support so we can assist you.",
                    HttpStatusCode.NotFound,
                    additionalDetails,
                    errorData);

            public static Error AdditionalPropertyDefinitionFailedToBePersisted(
                string name, string defaultValue, Guid id)
            {
                var details = new List<string>
                {
                    { $"Id: {id}" },
                    { $"Name: {name}" },
                    { $"Default value: {defaultValue}" },
                };

                return new Error(
                      "additionalproperty.creation.failed",
                      "Additional property definition failed to create",
                      $"The additional property definition with name {name} and default value '{defaultValue}' failed "
                      + "to save. It is possible that there is a database issue. We apologise for the "
                      + "inconvenience. Please contact your customer support.",
                      HttpStatusCode.NotFound,
                      details);
            }

            public static Error NameInUse(string entityType, string propertyName, string context)
            {
                var details = new List<string>
                {
                    { $"Property name: {propertyName}" },
                    { $"Entity type: {entityType}" },
                    { $"Context type: {context}" },
                };

                var errorData = new JObject
                {
                    { "propertyName", propertyName },
                    { "entityType", entityType },
                    { "context", context },
                };

                return new Error(
                    "additionalproperty.name.in.use",
                    "That property name is taken",
                    $"When trying to save a property with name {propertyName}, we found that either it is already in "
                    + $"used by the tenant it is associated with or by other context within the tenancy. Please be "
                    + "reminded that duplicate property name under same entity and context is not allowed. "
                    + "Please choose another name.",
                    HttpStatusCode.Conflict,
                    details,
                    errorData);
            }

            public static Error AliasInUse(string alias, string entityType, string context)
            {
                var details = new List<string>
                {
                    { $"Alias: {alias}" },
                    { $"Entity type: {entityType}" },
                    { $"Context type: {context}" },
                };

                var errorData = new JObject
                {
                    { "alias", alias },
                    { "entityType", entityType },
                    { "context", context },
                };

                return new Error(
                    "additionalproperty.alias.in.use",
                    "That alias is taken",
                    $"When trying to save an alias with name {alias}, we found that either it is already in used"
                    + $"by the tenant it is associated with or by other context within the tenancy. Please be "
                    + "reminded that duplicate property alias under same entity and context is not allowed. "
                    + "Please choose another alias.",
                    HttpStatusCode.Conflict,
                    details,
                    errorData);
            }

            public static Error ProductNotFound(Guid tenantId, Guid productId, string entityType, string name)
            {
                var details = new List<string>
                {
                    { $"Product Id: {productId}" },
                    { $"Tenant Id: {tenantId}" },
                    { $"Property name: {name}" },
                    { $"Entity type: {entityType}" },
                };

                var errorData = new JObject
                {
                    { "productId", productId },
                    { "tenantId", tenantId },
                    { "name", name },
                    { "entityType", entityType },
                };

                return new Error(
                    "additionalproperty.product.not.found",
                    "Product id is missing",
                    $"When trying to save an additional property '{name}' under a product '{productId}' associated "
                    + $"with tenant '{tenantId}', we found that the product doesn't exists during validation. It is "
                    + "possible that the request is providing in incorrect product id.  We apologise for the "
                    + "inconvenience. Please contact your customer support for assistance to check why the web form "
                    + "is sending an invalid data in the request.",
                    HttpStatusCode.NotFound,
                    details,
                    errorData);
            }

            public static Error InvalidContext(AdditionalPropertyDefinitionContextType context)
            {
                var contextDescription = context.Humanize();
                var details = new List<string>
                {
                    { $"Context type: {contextDescription}" },
                };

                var errorData = new JObject
                {
                    { "contextType", contextDescription },
                };
                return new Error(
                    "additionalproperty.invalid.context",
                    "Context is not valid",
                    $" When trying to save or delete an additional property, we found that the context '{context}' "
                    + "received by the server from the request is valid but with missing implementation, which is why "
                    + "it failed during the validation attempt. We apologise for the inconvenience. Please contact "
                    + "your customer support for assistance.",
                    HttpStatusCode.InternalServerError,
                    details,
                    errorData);
            }

            public static Error PropertyTypeNotFound(string propertyType)
            {
                var supportedPropertyTypes = Enum.GetValues(typeof(AdditionalPropertyDefinitionType));
                var supportedPropertyTypeDescription = string.Join(",", supportedPropertyTypes);
                return new Error(
                    "property.type.not.implemented",
                    "Property Type is not yet implemented",
                    $"The property type '{propertyType}' you selected is not yet implemented. Please check the detail "
                    + "for the complete list of valid property type. We apologise for the inconvenience. Please "
                    + "contact customer support for further assistance.",
                    HttpStatusCode.NotImplemented,
                    new List<string>()
                    {
                        $"Supported property types {(supportedPropertyTypes.Length > 0 ? "are" : "is")} "
                        + $"{supportedPropertyTypeDescription}",
                    },
                    new JObject
                    {
                        { "supportedPropertyTypes", supportedPropertyTypeDescription },
                    });
            }

            public static Error EntityTypeNotSupportedForUpdatingAdditionalPropertyValueToEntityAggregate(
                string entityType)
            {
                var entityTypeDescription = entityType.Humanize();
                return new Error(
                    "entity.type.not.supported",
                    "Entity type is not yet supported",
                    $"Updating the value of an additional property for the entity "
                    + $"{entityTypeDescription} is currently not supported. We apologise for the "
                    + "inconvenience. Please contact customer support for assistance.",
                    HttpStatusCode.NotImplemented,
                    new List<string>()
                    {
                        $"Entity type: {entityTypeDescription}",
                    },
                    new JObject
                    {
                        { "entityType", entityTypeDescription },
                    });
            }

            public static Error DerivedEntityTypeNotProvided(
                AdditionalPropertyEntityType entityType,
                IEnumerable<AdditionalPropertyEntityType> derivedEntityTypes)
            {
                var derivedTypeStrings = derivedEntityTypes.Select(et => et.Humanize());
                var joinedDerivedTypesString = derivedTypeStrings.JoinCommaSeparatedWithFinalOr();
                return new Error(
                        "additional.property.derived.entity.type.not.provided",
                        "You need to provide the most derived entity type",
                        "When retrieving additional property values, you asked us to fetch them for a "
                        + $"{entityType.Humanize()}, however we need to know if it's a {joinedDerivedTypesString} "
                        + "so that we can show the additional properties relevant to that type.",
                        HttpStatusCode.BadRequest);
            }

            public static Error AdditionalPropertyHasInvalidType(string alias, Type type, JObject? data = null) =>
                 new Error(
                     "additional.property.value.has.invalid.type",
                     "An additional property value has an invalid type",
                     $"The value specified for the additional property with alias \"{alias}\" is of an invalid type. " +
                     $"The specified value is of type \"{(type.ToString().StartsWith("System.") ? type.ToString().Remove(0, 7).Replace("64", string.Empty) : type.ToString()).ToLower()}\" whereas the required type is \"string\".",
                     HttpStatusCode.BadRequest,
                     data: data);

            public static Error AdditionalPropertyNotFound(AdditionalPropertyEntityType entityType, string alias, JObject? data = null) =>
             new Error(
                 "additional.property.alias.invalid",
                 "Additional property with specified alias not found",
                 $"The alias \"{alias}\" could not be resolved to an additional property defined for {entityType.Humanize().ToLower()} entities in this context. " +
                 "Please ensure that all specified additional properties have a name that matches the alias of an additional property defined for the entity type in question in the applicable context.",
                 HttpStatusCode.BadRequest,
                 data: data);

            public static Error AdditionalPropertyIsRequired(string alias, JObject? data = null) =>
             new Error(
                 "required.additional.property.missing",
                 "A required additional property is missing",
                 $"The required additional property with alias \"{alias}\" has not been included. " +
                 "Please ensure that you include a value for all required additional properties.",
                 HttpStatusCode.BadRequest,
                 data: data);

            public static Error RequiredAdditionalPropertyIsEmpty(string alias, JObject? data = null) =>
             new Error(
                 "required.additional.property.value.missing",
                 "A required additional property value is missing",
                 $"The value provided for the required additional property with alias \"{alias}\" is null or empty string. " +
                 $"Please ensure that you provide a valid value for all required additional properties. Please note that an empty string is not considered a valid value.",
                 HttpStatusCode.BadRequest,
                 data: data);

            public static Error UniqueAdditionalPropertyValueAlreadyUsed(AdditionalPropertyEntityType entityType, string alias, string value, JObject? data = null) =>
             new Error(
                 "additional.property.value.not.unique",
                 "A unique additional property value is already in use",
                 $"The additional property with alias \"{alias}\" requires a value that is unique, however the provided value \"{value}\" is already used by another {entityType.Humanize().ToLower()} entity in the same context. " +
                 $"Please ensure that you provide a unique value for all additional properties that required unique values within their context.",
                 HttpStatusCode.BadRequest,
                 data: data);

            public static Error InvalidJsonObject(
                string fieldName,
                string sourceJson,
                string message,
                int lineNumber,
                int linePosition,
                string path) =>
                new Error(
                    "additional.property.invalid.json.object",
                    $"The {fieldName} field must contain a valid JSON object",
                    $"The {fieldName} field must contain a valid JSON object. "
                    + message + ". "
                    + "Please run it through a JSON Validator such as https://jsonlint.com/ then fix the errors before trying again.",
                    HttpStatusCode.BadRequest,
                    new string[]
                    {
                        $"Path: {path}",
                        $"Line number: {lineNumber}",
                        $"Position: {linePosition}",
                    },
                    new JObject()
                    {
                        { "jsonPath", path },
                        { "lineNumber", lineNumber },
                        { "linePosition", linePosition },
                        { "sourceJson", sourceJson },
                    });

            public static Error NotAValidJsonSchema(
                string alias,
                Guid tenantId,
                AdditionalPropertyEntityType entityType,
                AdditionalPropertyDefinitionContextType contextType)
            {
                return new Error(
                    "additional.property.definition.not.a.valid.JSON.schema",
                    "The defined schema is not a valid JSON schema",
                    $"When trying to save the additional property '{alias}' under entity '{entityType.Humanize().ToLower()}' associated "
                    + $"with tenant '{tenantId}' of context {contextType}, we found that the defined schema is not a valid JSON schema.",
                    HttpStatusCode.BadRequest);
            }

            public static Error SchemaValidationFailure(
                string jsonSchemaName,
                IEnumerable<string> additionalDetails,
                string? fieldName)
            {
                fieldName = fieldName ?? "JSON object";
                return new Error(
                    "additional.property.json.object.schema.assertion.failed",
                    $"{fieldName} does not pass schema assertion",
                    $"Validation of the JSON object against the '{jsonSchemaName} schema failed.",
                    HttpStatusCode.BadRequest,
                    additionalDetails);
            }

            public static Error PropertyTypeNotYetSupported(string propertyType) =>
                new Error(
                    "additional.property.type.not.yet.supported",
                    $"The additional property type {propertyType} is not yet supported",
                    $"When trying to create a new additional property definition, the property type {propertyType} was provided," +
                    $"however this is not yet a supported additional property type.",
                    HttpStatusCode.BadRequest);
        }

        public static class LuceneIndex
        {
            public static Error SearchIndexCorrupted(
                    string tenantAlias, DeploymentEnvironment environment, string entity, string message) =>
                    new Error(
                        $"{entity}.search.index.corrupted",
                        $"The {entity} search index needs to be regenerated",
                        $"The {entity} search index cannot be used in the current state, and needs to be regenerated. "
                        + message.WithDot().WithSpace()
                        + "Please get in touch with customer support so they can regenerate the search index.",
                        HttpStatusCode.InternalServerError,
                        null,
                        new JObject
                        {
                        { "tenantAlias", tenantAlias },
                        { "environment", environment.Humanize() },
                        });
        }

        public static class DataTableDefinition
        {
            public static Error NameInUse(string name) =>
                new Error(
                    "data.table.definition.name.in.use",
                    "Table name already in use",
                    $"This entity already has a data table with the name \"{name}\". " +
                    "Names must be unique among data tables created for the same entity. " +
                    "Please choose a different name for this data table.",
                    HttpStatusCode.Conflict);

            public static Error AliasInUse(string alias) =>
                new Error(
                    "data.table.definition.alias.in.use",
                    "Table alias already in use",
                    $"This entity already has a data table with the alias \"{alias}\". " +
                    "Aliases must be unique among data tables created for the same entity. " +
                    "Please choose a different alias for this data table.",
                    HttpStatusCode.Conflict);

            public static Error CsvDataTooLarge() =>
                new Error(
                    "datatable.definition.csv.data.too.large",
                    "CSV data too large",
                    $"Total size must not exceed 20 MB. If you wish to store more than 20 MB, please disable caching.",
                    HttpStatusCode.BadRequest);

            public static Error AliasNotFound(string alias, JObject errorData) =>
                new Error(
                    "data.table.alias.not.found",
                    "A data table with the specified alias was not found",
                    $"A data table with the alias \"{alias}\" could not be found on the specified entity. Please ensure that you have specified the correct data table alias.",
                    HttpStatusCode.NotFound,
                    null,
                    errorData);

            public static Error NotSupportedEntity(string entityType, JObject errorData) =>
               new Error(
                   "entity.type.does.not.support.data.tables",
                   "This entity type does not support data tables",
                   $"The specified entity is of a type \"{entityType.ToLower()}\" that does not support data tables. Please ensure that the specified entity of a type that supports data tables.",
                   HttpStatusCode.BadRequest,
                   null,
                   errorData);

            public static Error NotFound(Guid dataTableDefinitionId)
               => new Error(
                   "data.table.definition.details.not.found",
                   $"We could not find the data table details for id \"{dataTableDefinitionId}\"",
                   $"When trying to find a data table details with an id of \"{dataTableDefinitionId}\", nothing came up. Please check you've entered the correct details. If you think this is bug, please contact customer support.",
                   HttpStatusCode.NotFound,
                   null,
                   new JObject()
                   {
                        { "dataTableDefinitionId", dataTableDefinitionId },
                   });

            public static Error NameIsEmpty() =>
                new Error(
                    "data.table.definition.name.is.empty",
                    "The data table definition name is empty",
                    $"The data table definition name is empty. Please add a name.",
                    HttpStatusCode.BadRequest);

            public static Error AliasIsEmpty() =>
                new Error(
                    "data.table.definition.alias.is.empty",
                    "The data table definition alias is empty",
                    $"The data table definition alias is empty. Please add an alias.",
                    HttpStatusCode.BadRequest);

            public static Error TableSchemaIsEmpty() =>
                new Error(
                    "data.table.definition.table.schema.is.empty",
                    "The data table definition JSON configuration is empty",
                    $"The data table definition JSON configuration is empty. Please add a JSON configuration.",
                    HttpStatusCode.BadRequest);

            public static Error AlreadyDeleted() =>
                new Error(
                    "data.table.definition.was.already.deleted",
                    "The data table definition was already deleted",
                    $"The data table definition was already deleted. Please confirm action.",
                    HttpStatusCode.BadRequest);

            public static Error InvalidDatabaseTableName(string databaseTableName) =>
                new Error(
                    "data.table.definition.database.table.name.is.invalid",
                    $"The data table definition database table name \"{databaseTableName}\" is invalid",
                    $"The data table definition database table name \"{databaseTableName}\" is invalid. Please add valid database table name.",
                    HttpStatusCode.BadRequest);

            public static Error OutOfIndex(string baseDatabaseTableName) =>
                new Error(
                    "data.table.definition.out.of.index",
                    $"The data table definition with base database table name of \"{baseDatabaseTableName}\" run out of index number",
                    $"0001 - 9999 indexs are fully utilized for data table definition with base database table name of \"{baseDatabaseTableName}\". "
                    + "Please contact customer support.",
                    HttpStatusCode.BadRequest);

            public static Error JsonIndexKeyColumnNotFound(bool isClustered, string indexAlias)
            {
                string indexType = isClustered ? "clustered" : "unclustered";

                return new Error(
                    "data.table.definition.json.index.key.column.not.found",
                    $"Key column was not found for index \"{indexAlias}\".",
                    $"When validating the JSON configuration {indexType} indexes, " +
                    $"the key column for {indexType} index was not found. " +
                    $"Please ensure to define at least one key column for this index.",
                    HttpStatusCode.BadRequest);
            }

            public static Error IndexColumnAliasNotFound(bool isClustered, bool isKeyColumn, List<string> notExistingColumnAliases)
            {
                string indexType = isClustered ? "clustered" : "unclustered";
                string indexColumnType = isKeyColumn ? "key" : "non-key";
                var quotedColumnAliases = notExistingColumnAliases.Select(x => $"\"{x.Camelize()}\"").ToList();
                string readbleColumnAliases = notExistingColumnAliases.Count > 1
                  ? string.Join(", ", quotedColumnAliases
                  .Take(quotedColumnAliases.Count - 1)) + " and " + quotedColumnAliases.Last()
                  : quotedColumnAliases.FirstOrDefault() ?? string.Empty;
                string aliasNoun = notExistingColumnAliases.Count > 1 ? "aliases" : "alias";
                string columnNoun = notExistingColumnAliases.Count > 1 ? "columns" : "column";

                return new Error(
                    "data.table.definition.index.column.alias.not.found",
                    $"An index referred to a column alias that could not be found in the list of defined columns",
                    $"When trying to save the configuration for a data table, the attempt failed because " +
                    $"the {indexType} index {indexColumnType} {columnNoun} {aliasNoun} " +
                    $"{readbleColumnAliases} could not be found in the list of defined columns. " +
                    $"To resolve this issue, please ensure that all key columns that are part of index " +
                    $"definitions use a column alias that can be matched against a column alias in the list " +
                    $"of defined columns. If you need further assistance please contact customer support.",
                    HttpStatusCode.BadRequest);
            }

            public static Error IndexInvalidKeyColumnLargeObjectType(
                bool isClustered,
                string indexAlias,
                string keyColumnAlias,
                string columnDataType,
                string sqlDataTypeAndSize)
            {
                string indexType = isClustered ? "clustered" : "unclustered";
                return new Error(
                    "data.table.definition.index.column.data.type.invalid",
                    $"An index key column had an unsupported data type",
                    $"When trying to save the configuration for a data table, " +
                    $"the attempt failed because the {indexType} index with alias \"{indexAlias}\" " +
                    $"contained a key column with reference to the \"{keyColumnAlias}\" database column. " +
                    $"The \"{keyColumnAlias}\" column definition uses the \"{columnDataType}\" " +
                    $"data type which results in the SQL column data type \"{sqlDataTypeAndSize}\". " +
                    $"This SQL column data type is not supported for creating {indexType} database indexes due to its size. " +
                    $"If you need further assistance please contact customer support.",
                    HttpStatusCode.BadRequest);
            }

            public static Error IndexInvalidUniqueColumnLargeObjectType(
                string columnAlias,
                string columnDataType,
                string sqlDataTypeAndSize)
            {
                return new Error(
                    "data.table.definition.unique.column.data.type.invalid",
                    $"A unique column has an unsupported data type",
                    $"When trying to save the configuration for a data table, the attempt failed because the column \"{columnAlias}\" " +
                    $"uses the \"{columnDataType}\" data type " +
                    $"which results in an SQL column data type \"{sqlDataTypeAndSize}\". " +
                    $"This SQL column data type is not supported for creating unique column constraint due to its size. " +
                    $"If you need further assistance please contact customer support.",
                    HttpStatusCode.BadRequest);
            }

            public static Error ColumnNamesNotUnique(List<string> duplicateColumnNames)
            {
                var quotedColumnNames = duplicateColumnNames.Select(x => $"\"{x}\"").ToList();
                var readbleColumnNames = string.Join(", ", quotedColumnNames);
                return new Error(
                    "data.table.definition.column.name.not.unique",
                    "The data table definition contained a column with a non-unique name",
                    "When trying to save the configuration for a data table, " +
                    $"the attempt failed because one or more of the defined columns has the same name " +
                    $"({readbleColumnNames}) as another column. " +
                    $"To resolve this issue, ensure that all columns in the data table " +
                    $"definition have unique name values. If you need further assistance " +
                    $"please contact customer support.",
                    HttpStatusCode.BadRequest);
            }

            public static Error ColumnAliasesNotUnique(List<string> duplicateColumnAliases)
            {
                var quotedColumnAliases = duplicateColumnAliases.Select(x => $"\"{x}\"").ToList();
                var readbleColumnAliases = string.Join(", ", quotedColumnAliases);
                return new Error(
                    "data.table.definition.column.alias.not.unique",
                    "The data table definition contained a column with a non-unique alias",
                    "When trying to save the configuration for a data table, " +
                    $"the attempt failed because one or more of the defined columns has the same alias " +
                    $"({readbleColumnAliases}) as another column. " +
                    $"To resolve this issue, ensure that all columns in the data table " +
                    $"definition have unique alias values. If you need further assistance " +
                    $"please contact customer support.",
                    HttpStatusCode.BadRequest);
            }

            public static Error CsvColumnHeaderNameNotFoundInJson(List<string> columnHeader)
            {
                var quotedColumnHeader = columnHeader.Select(x => $"\"{x}\"").ToList();
                string readbleColumnHeaders = columnHeader.Count > 1
                  ? string.Join(", ", quotedColumnHeader
                  .Take(quotedColumnHeader.Count - 1)) + " and " + quotedColumnHeader.Last()
                  : quotedColumnHeader.FirstOrDefault() ?? string.Empty;
                string verb = columnHeader.Count > 1 ? "were" : "was";
                string columnNoun = columnHeader.Count > 1 ? "columns" : "column";
                return new Error(
                    "data.table.csv.data.column.alias.not.found",
                    "The CSV data contained an undefined column header",
                    $"When trying to import CSV data into a data table, the attempt failed because the CSV " +
                    $"{columnNoun} {readbleColumnHeaders} {verb} not defined in the data table configuration. " +
                    $"To resolve this issue, please ensure that each CSV column header that is part " +
                    $"of the CSV data payload matches the alias of a column defined in the data table configuration. " +
                    $"If you need further assistance please contact customer support.",
                    HttpStatusCode.BadRequest);
            }

            public static Error CsvDataRequiredColumnNotFound(List<string> columnAliases)
            {
                var quotedColumnAliases = columnAliases.Select(x => $"\"{x}\"").ToList();
                string readbleColumnNames = columnAliases.Count > 1
                  ? string.Join(", ", quotedColumnAliases
                  .Take(quotedColumnAliases.Count - 1)) + " and " + quotedColumnAliases.Last()
                  : quotedColumnAliases.FirstOrDefault() ?? string.Empty;
                string verb = columnAliases.Count > 1 ? "were" : "was";
                string columnNoun = columnAliases.Count > 1 ? "columns" : "column";
                return new Error(
                    "data.table.csv.data.required.column.not.found",
                    "A required column is missing",
                    $"When trying to import CSV data into a data table, the attempt failed " +
                    $"because the required CSV {columnNoun} {readbleColumnNames} {verb} missing " +
                    $"from the CSV data.To resolve this issue, please ensure that all required " +
                    $"columns exists in the CSV data. If you need further assistance please contact customer support.",
                    HttpStatusCode.BadRequest);
            }

            public static Error CsvDataColumnValueNotUnique(string details) => new Error(
                    "data.table.csv.data.column.value.not.unique",
                    "The CSV data contained a non-unique value for a column that requires a unique value",
                    "When trying to import CSV data into a data table, the attempt failed because a row " +
                    "was detected with a non-unique value for a column that is configured to require a unique value " +
                    "for every record. To resolve this issue, either ensure that each row in the CSV data " +
                    "contains a unique value for all columns configured to require a unique value, " +
                    "or change the data table configuration to remove the unique value constraint " +
                    "on the affected column. If you need further assistance please contact customer support.",
                    HttpStatusCode.BadRequest);

            public static Error CsvDataColumnValueNotFound() => new Error(
                    "data.table.csv.data.column.value.not.found",
                    "The CSV data contained a row with a missing column value",
                    "When trying to import CSV data into a data table, the attempt failed " +
                    "because one of the data rows contained a column with a missing value. " +
                    "To resolve this issue, please ensure that all rows contain values for each of the defined columns. " +
                    "If a column needs to contain an empty value, a delimiter must be used to explicitly denote that empty value. " +
                    "If you need further assistance please contact customer support.",
                    HttpStatusCode.BadRequest);
        }

        public static class FilterListItemList
        {
            public static Error AliasIsRequired(JObject? errorData = null) =>
                new Error(
                    "filter.list.item.list.alias.is.required",
                    "The filter list item list requires an alias",
                    $"The filter list item list requires an alias when using data table query list. "
                    + "Please ensure that you have specified an alias.",
                    HttpStatusCode.NotFound,
                    null,
                    errorData);
        }

        public static class Schema
        {
            public static Error NotPublished(string filename) =>
                new Error(
                    "schema.file.not.published",
                    "Schema Not Published",
                    $"No such schema file \"{filename}\" has been published. Please check you've specified correct filename.",
                    HttpStatusCode.NotFound);
        }

        public static class Saml
        {
            public static Error SamlErrorStatus(string message, string code) =>
                new Error(
                    "saml.exception.thrown",
                    "A SAML exception was thrown",
                    $"A SAML error occured with the message \"{message}\" with error code \"{code}\". "
                    + "Please get in touch with a systems administrator or ubind support to find out what caused the issue.",
                    HttpStatusCode.InternalServerError);
        }

        public static class RedirectUrl
        {
            public static Error NotFound(string url) =>
                new Error(
                    "destination.url.not.found",
                    "Destination URL not found",
                    $"When trying to perform a redirect in relation to a tiny URL," +
                    $" the attempt failed because the tiny URL token could not be resolved against a destination URL." +
                    $" To resolve this issue, please ensure that the token used as part of the tiny URL corresponds to a valid destination URL." +
                    $" If you require further assistance please contact technical support.",
                    HttpStatusCode.NotFound,
                    new List<string> { $"Tiny URL: {url}" });
        }
    }
}
