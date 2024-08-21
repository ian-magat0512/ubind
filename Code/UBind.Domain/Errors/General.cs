// <copyright file="General.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

#pragma warning disable SA1600
#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Humanizer;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Extensions;
    using UBind.Domain.Permissions;

    /// <summary>
    /// Allows enumeration of all application errors as outlined here: https://enterprisecraftsmanship.com/posts/advanced-error-handling-techniques/.
    /// </summary>
    public static partial class Errors
    {
        /// <summary>
        /// General category for Error - place general errors here, or create a relevant category.
        /// </summary>
        public static class General
        {
            public static Error NotFound(string entityName, dynamic matchingValue, string propertyName = "ID") =>
                new Error(
                    "record.not.found",
                    $"We couldn't find that {entityName}",
                    $"When trying to find a {entityName} with the property \"{propertyName}\" matching the value \"{matchingValue}\", nothing came up. Please check that you've entered the correct details. If you think this is a bug, please contact customer support.",
                    HttpStatusCode.NotFound);

            public static Error NotFoundOrCouldHaveBeenDeleted(string entityName, dynamic matchingValue, string propertyName = "ID") =>
                new Error(
                    "record.not.found.or.deleted",
                    $"We couldn't find that {entityName}",
                    $"The {entityName} with {propertyName} '{matchingValue}' was not found. If this {entityName} did exist, it may have been marked as deleted. Please check that you've entered the correct details. If you think this is a bug, please contact customer support.",
                    HttpStatusCode.NotFound);

            public static Error NotAuthenticated(string? action = null, string? entityName = null, dynamic? id = null) =>
                new Error(
                    "user.not.authenticated",
                    "You need to be logged in to do that",
                    $"You need to be logged in to {action ?? "perform that action"}"
                    + (entityName != null ? $" on {entityName}" : string.Empty)
                    + (id != null ? $" with ID \"{id}\"" : string.Empty) + ". "
                    + "Please ensure your account is logged in and you have the necessary permissions before trying again.",
                    HttpStatusCode.Unauthorized);

            public static Error NotAuthenticated() =>
                new Error(
                    "user.not.authenticated",
                    "You need to sign in",
                    $"You need to be signed in to access this resource.",
                    HttpStatusCode.Unauthorized);

            public static Error NotAuthorized(string action, string? entityName = null, dynamic? id = null) =>
                new Error(
                    "user.not.authorised",
                    "You're not allowed to do that",
                    $"You are not authorised to {action}" + (entityName != null ? $" on {entityName}" : string.Empty)
                    + (id != null && id != default(Guid) ? $" with ID \"{id}\"" : string.Empty) + ". "
                    + "If you feel you should have access to do that, please get in touch with an administrator to request a higher level of access.",
                    HttpStatusCode.Forbidden);

            public static Error NotAuthorized() =>
                new Error(
                    "user.not.authorised",
                    "You don't have access",
                    $"You are not allowed to access this resource. "
                    + "If you feel you should have access, please get in touch with an administrator to request a higher level of access.",
                    HttpStatusCode.Forbidden);

            public static Error PermissionRequiredToAccessResource(
                Permission permission,
                string resourceTypeName,
                string? resourceIdentifier = null,
                string? additionalConditionMessage = null) =>
                new Error(
                    "permission.required.to.access.resource",
                    $"You're missing the required permission",
                    $"In order to access the {resourceTypeName}{(string.IsNullOrEmpty(resourceIdentifier) ? string.Empty : $" \"{resourceIdentifier}\"")}, you need the "
                    + $"\"{permission.Humanize()}\" permission{(string.IsNullOrEmpty(additionalConditionMessage) ? "." : " and " + additionalConditionMessage + ".")} "
                    + "If you feel you should have access, please get in touch with an administrator to request a "
                    + "higher level of access.",
                    HttpStatusCode.Forbidden);

            public static Error PermissionRequiredToModifyResource(
                Permission permission,
                string resourceTypeName,
                string? resourceIdentifier = null,
                string? additionalConditionMessage = null) =>
                new Error(
                    "permission.required.to.modify.resource",
                    $"You're missing the required permission",
                    $"In order to modify the {resourceTypeName}{(string.IsNullOrEmpty(resourceIdentifier) ? string.Empty : $" with Id \"{resourceIdentifier}\"")}, you need the "
                    + $"\"{permission.Humanize()}\" permission{(string.IsNullOrEmpty(additionalConditionMessage) ? "." : " and " + additionalConditionMessage + ".")} "
                    + "If you feel you should have access, please get in touch with an administrator to request a "
                    + "higher level of access.",
                    HttpStatusCode.Forbidden);

            public static Error PermissionRequiredToCeateResource(
                Permission permission,
                string resourceTypeName,
                string? additionalConditionMessage = null) =>
                new Error(
                    "permission.required.to.create.resource",
                    $"You're missing the required permission",
                    $"In order to create a {resourceTypeName}, you need the "
                    + $"\"{permission.Humanize()}\" permission{(string.IsNullOrEmpty(additionalConditionMessage) ? "." : " and " + additionalConditionMessage + ".")} "
                    + "If you feel you should have access, please get in touch with an administrator to request a "
                    + "higher level of access.",
                    HttpStatusCode.Forbidden);

            public static Error PermissionRequiredToPerformOperation(
                Permission permission,
                string operationDescription) =>
                new Error(
                    "permission.required.to.perform.operation",
                    $"You're not allowed to do that",
                    $"In order to {operationDescription}, you need the \"{permission.Humanize()}\" permission. "
                    + "If you feel you should be allowed to do that, please get in touch with an administrator to "
                    + "request a higher level of access.",
                    HttpStatusCode.Forbidden);

            public static Error AccessDeniedToResource(string resourceTypeName, string resourceIdentifier) =>
                new Error(
                    "access.denied.to.resource",
                    $"You don't have access to that {resourceTypeName}",
                    $"You tried to access the {resourceTypeName} \"{resourceIdentifier}\", "
                    + $"but it is not your {resourceTypeName}, you are not an assigned agent or "
                    + $"you don't have the required permission."
                    + $" If you believe you should have access to this {resourceTypeName}, "
                    + "please get in touch with your administrator, or contact customer support.",
                    HttpStatusCode.Forbidden);

            public static Error AccessDeniedToEnvironment(DeploymentEnvironment environment) =>
                new Error(
                    "access.denied.to.data.environment",
                    $"You don't have access to that data environment",
                    $"You tried to access data from the {environment} environment, however "
                    + $"you don't have the required permission."
                    + $"If you believe you should have access to this environment, "
                    + "please get in touch with your administrator, or contact customer support.",
                    HttpStatusCode.Forbidden);

            public static Error Forbidden(string? action = null, string? reason = null) =>
                new Error(
                    "action.forbidden",
                    "Action forbidden",
                    "You cannot " + (action != null ? action : "perform that action")
                    + (reason != null ? ", because " + reason : string.Empty)
                    + ". If you believe this is a mistake, or you would like assistance, please contact customer support.",
                    HttpStatusCode.Forbidden);

            public static Error BadRequest(string reason) =>
                new Error(
                    "bad.request",
                    "Bad request",
                    $"Your request was invalid, malformed, or contained missing data: {reason}. "
                    + "If you believe this is a mistake, or you would like assistance, please contact customer support.",
                    HttpStatusCode.BadRequest);

            public static Error Unexpected(string? description = null, JObject? data = null) =>
                new Error(
                    "error.unexpected",
                    "Something went wrong",
                    $"Something went wrong, and unfortunately this an unexpected situation. We apologise for the inconvenience. "
                    + "We would appreciate it if you would contact customer support and provide a description of the steps you took to uncover this issue. "
                    + "If you can provide a screenshot or video recording, and details about your device or browser that would help us to resolve this quickly.",
                    HttpStatusCode.InternalServerError,
                    description != null ? new string[] { description } : null,
                    data);

            public static Error UnexpectedEnumValue(Enum value, Type type, JObject? data = null) =>
            new Error(
                "error.unexpected.enum.value",
                "Unexpected Enum Value",
                $"The enum value {value.Humanize()} of {type.Name} is not one of the possible valid values for this situation. "
                + "We apologise for the inconvenience. "
                + "We would appreciate it if you would contact customer support and provide a description of the steps you took to uncover this issue. "
                + "If you can provide a screenshot or video recording, and details about your device or browser that would help us to resolve this quickly.",
                HttpStatusCode.InternalServerError,
                data: data);

            public static Error InvalidGuid(string sourceValue) =>
                new Error(
                    "error.invalid.guid.value",
                    "Invalid GUID value",
                    $"We were unable to parse the value \"{sourceValue.LimitLengthWithEllipsis(50)}\" into a GUID. "
                    + $"The format was incorrect. A GUID should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).",
                    HttpStatusCode.BadRequest);

            public static Error InvalidEmailAddress(string emailAddress, string action) =>
            new Error(
                "invalid.email.address",
                "Invalid email address",
                $"When trying to {action}, we found that email address \"{emailAddress}\" is invalid."
                + $"Please check you've entered the correct email address. If you think this is bug, please contact customer support.",
                HttpStatusCode.BadRequest);

            public static Error UrlInvalid(string url) =>
            new Error(
                "invalid.url",
                "Invalid URL",
                $"The URL \"{url}\" is not valid."
                + $"Please check that you've entered a proper URL with a scheme, host and optionally port, path and "
                + "query string. For example: \"https://mywebsite.com/somewhere\".",
                HttpStatusCode.BadRequest);

            public static Error DuplicatePropertyValue(string entityName, string propertyName, string value) =>
                new Error(
                    "error.duplicate.property.value",
                    "It needs to be unique",
                    $"You entered a duplicate value \"{value}\" for the property \"{propertyName}\" of a {entityName}. "
                    + "It needs to be unique so please choose another value.",
                    HttpStatusCode.Conflict);

            public static Error ModelValidationFailed(string? reason = null, IEnumerable<string>? additionalDetails = null) =>
                new Error(
                    "model.validation.failed",
                    "Model validation failed",
                    "A validation error occurred when checking the data. When this happens it means the set of data sent "
                    + "to the server either had some data missing, or was in an invalid format."
                    + (reason != null ? " " + reason : string.Empty),
                    HttpStatusCode.BadRequest,
                    additionalDetails);

            public static Error FeatureDisabled(string featureName) =>
                new Error(
                    "feature.disabled",
                    "Feature disabled",
                    $"You have attempted to access a page that requires the {featureName} function to be enabled. To enable this feature, please contact an administrator for assistance.",
                    HttpStatusCode.PreconditionFailed);

            public static Error FeatureUnderConstruction(string featureName) =>
                new Error(
                    "feature.under.construction",
                    "Feature Under Construction",
                    $"You have attempted to access the {featureName} feature which is still under construction. This will be fully operational once development work is complete. Please bear with us for the moment.",
                    HttpStatusCode.PreconditionFailed);

            public static Error OperationCancelled() =>
                new Error(
                    "operation.cancelled",
                    "The operation was canceled",
                    $"The operation was cancelled. This can happen because a new request was submitted, the browser "
                    + "cancelled the request before the server had a chance to respond.",
                    (HttpStatusCode)499); // 499 - Client Closed Request. This is not yet a standard status code, this is used by NGINX in similar scenario.

            /// <summary>
            /// The error message when a 3rd party exception occures.
            /// </summary>
            /// <param name="additionalDetails">This will add more information about the error
            /// since we dont know in real time what caused the error.</param>
            public static Error ApiAccessException(string apiUrl, IEnumerable<string> additionalDetails) =>
                new Error(
                    "api.access.exception",
                    $"A 3rd party API caused an error",
                    $"A 3rd party API ({apiUrl}) is being accessed and caused an error, unfortunately this an unexpected situation, please contact an administrator for assistance.",
                    HttpStatusCode.Forbidden,
                    additionalDetails);

            public static Error UnknownTimeZoneId(string timeZoneId) =>
                new Error(
                    "unknown.time.zone.id",
                    "We're not familiar with that time zone",
                    $"The time zone ID \"${timeZoneId}\" is not a valid or known time zone ID within the TZDB database. "
                    + "Please ensure that TZDB time zones are used, and not one from another time zone database."
                    + "We apologise for the inconvenience. Please get in touch with customer support so we can help you "
                    + "resolve this issue.",
                    HttpStatusCode.NotFound);

            public static Error RequestTimedOut() =>
                new Error(
                    "request.timed.out",
                    "The operation timed out",
                    "The operation timed out. This can happen if the request is working on a large dataset, or triggering a third-party API that took too long to respond. "
                    + "If you are requesting a large dataset, please consider using paging or reducing your page size.",
                    HttpStatusCode.RequestTimeout);

            public static Error FileNameNotFoundForSchemaType(string schemaName) =>
                new Error(
                    "file.name.not.found.for.schema.type",
                    "File name not found for schema type",
                    $"File name was not found for the schema type \"{schemaName}\". "
                    + "We apologise for the inconvenience. Please get in touch with customer support so we can help you "
                    + "resolve this issue.",
                    HttpStatusCode.InternalServerError);

            public static class Factory
            {
                public static Error UnsupportedType(string typeId, string factoryName) =>
                    new Error(
                        "factory.unsupported.type",
                        "Cannot create an instance of this type",
                        $"The type \"{typeId}\" is not supported by the factory \"{factoryName}\". "
                        + "We apologise for the inconvenience. Please get in touch with customer support so we can help you "
                        + "resolve this issue.",
                        HttpStatusCode.InternalServerError);
            }
        }
    }
}
