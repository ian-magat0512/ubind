// <copyright file="Automation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

#pragma warning disable SA1600
#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace UBind.Domain;

using System;
using System.Collections.Generic;
using System.Net;
using CSharpFunctionalExtensions;
using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Extensions;
using UBind.Domain.Helpers;
using UBind.Domain.ReadWriteModel;

/// <summary>
/// Allows enumeration of all application errors as outlined here: https://enterprisecraftsmanship.com/posts/advanced-error-handling-techniques/.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Automation errors.
    /// </summary>
    public static class Automation
    {
        public static Error EnvironmentNotFound(string environment) =>
            new Error(
                "automation.environment.not.found",
                $"Environment not found",
                $"When trying to find environment \"{environment}\", nothing came up. Please ensure that you are passing the correct environment.",
                HttpStatusCode.NotFound,
                null,
                new JObject()
                {
                    { "environment", environment },
                });

        public static Error PeridicTriggerCannotBeScheduled(string tenantAlias, string productAlias, DeploymentEnvironment environment, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.periodic.scheduler.cannot.be.scheduled",
                "Automation periodic scheduler encountered an error",
                GenerateAutomationErrorMessage(
                    string.Join(
                        Environment.NewLine,
                        value: new string[]
                        {
                            $"An error was encountered when trying to schedule the automation for the given tenant {tenantAlias} and product {productAlias} under environment {environment}.",
                            "This issue may have been caused by a non-backward compatible change made to the automation codebase.",
                        }),
                    "Please contact customer support and provide the failing configuration."),
                HttpStatusCode.ExpectationFailed,
                data);

        public static Error InvalidAutomationConfiguration(JObject data, List<string>? additionalDetails = null) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.configuration.could.not.be.parsed",
                "Automation configuration could not be parsed",
                GenerateAutomationErrorMessage(
                    string.Join(
                        Environment.NewLine,
                        value: new string[]
                        {
                            "An error was encountered while attempting to parse the automation configuration associated with the deployed product release.",
                            "This issue may have been caused by a non-backward compatible change made to the automation codebase, and may require an update to the automation configuration for this product.",
                        }),
                    "Please check the automation configuration."),
                HttpStatusCode.PreconditionFailed,
                data,
                additionalDetails: additionalDetails);

        public static Error AutomationConfigurationNotFound(
            string tenantAlias,
            string productAlias,
            DeploymentEnvironment environment,
            Guid productReleaseId,
            JObject? data = null,
            List<string>? additionalDetails = null) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.configuration.not.found",
                "Automation configuration not found",
                $"An automation configuration could not be found for tenannt {tenantAlias}, product {productAlias} "
                + $"and product release {productReleaseId}, in environment {environment}.",
                HttpStatusCode.PreconditionFailed,
                data,
                additionalDetails: additionalDetails);

        public static Error AutomationConfigurationNotSupported(string message, JObject data, List<string>? additionalDetails = null) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.configuration.not.supported",
                $"Unsupported configuration.",
                GenerateAutomationErrorMessage(message),
                HttpStatusCode.InternalServerError,
                data,
                additionalDetails);

        public static Error AutomationConfigurationUnavailable(string tenantAlias, string productAlias, DeploymentEnvironment environment, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.configuration.unavailable",
                "Automation Configuration is not available",
                $"There is no automation configuration for the given tenant {tenantAlias} and product {productAlias} under the environment {environment}. Please check to confirm that a release is available for the given environment, or"
                + " that an automation configuration was included as part of the release.",
                HttpStatusCode.NotFound,
                data);

        public static Error AutomationDataDeserializationError(
            JObject? data = null,
            List<string>? additionalDetails = null) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.data.deserialization.error",
                "Error deserializing automation data",
                "An error occurred while deserializing automation data.",
                HttpStatusCode.BadRequest,
                data,
                additionalDetails: additionalDetails);

        public static Error HasDuplicateAutomationAlias(string duplicateAlias) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.configuration.should.have.distinct.automation.alias",
                "You've used an automation alias more than once",
                $"In your automations configuration you've used the automation alias \"{duplicateAlias}\" more "
                + "than once. Please ensure automation aliases are unique withing an automations configuration.",
                HttpStatusCode.Conflict,
                new JObject { { "duplicateAlias", duplicateAlias } });

        public static Error HasDuplicateTrggerAlias(string automationAlias, string duplicateTriggerAlias) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.configuration.should.have.distinct.automation.trigger.alias",
                "You've used a trigger alias more than once",
                $"In your automations configuration, for the automation \"{automationAlias}\" you've used the "
                + $"trigger alias \"{duplicateTriggerAlias}\" more "
                + "than once. Please ensure trigger aliases are unique within an automation.",
                HttpStatusCode.Conflict,
                new JObject { { "duplicateTriggerAlias", duplicateTriggerAlias } });

        public static Error HasDuplicateActionAlias(string automationAlias, string duplicateActionAlias) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.configuration.should.have.distinct.automation.action.alias",
                "You've used an action alias more than once",
                $"In your automations configuration, for the automation \"{automationAlias}\" you've used the "
                + $"action alias \"{duplicateActionAlias}\" more "
                + "than once. Please ensure action aliases are unique within an automation.",
                HttpStatusCode.Conflict,
                new JObject { { "duplicateActionAlias", duplicateActionAlias } });

        public static Error AutomationNotFound(string tenant, string product, DeploymentEnvironment environment, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.configuration.not.found",
                "The requested automation cannot be found",
                GenerateAutomationErrorMessage(
                    $"The requested automation for {tenant} {product} under {environment} cannot be matched with the given trigger request.",
                    "See accompanying data for more information."),
                HttpStatusCode.NotFound,
                data);

        public static Error AutomationTriggerNotFound(string tenant, string product, DeploymentEnvironment environment, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.configuration.trigger",
                "The requested automation trigger cannot be found",
                GenerateAutomationErrorMessage(
                    $"The requested automation trigger for {tenant} {product} under {environment} cannot be matched with the given trigger request. Possibly caused by a change in configuration.",
                    "See accompanying data for information."),
                HttpStatusCode.NotFound,
                data);

        public static Error AutomationActionNotFound(string tenant, string product, DeploymentEnvironment environment, string automationAlias, string actionAlias, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.configuration.action.not.found",
                "The action to be executed cannot be retrieved from the automation",
                GenerateAutomationErrorMessage(
                    $"The requested automation action with ID: {actionAlias} for {tenant} {product} under {environment} from automation {automationAlias} cannot be found. This can be caused by a change" +
                    " in the configuration during the processing of the automation.",
                    "See accompanying data for information."),
                HttpStatusCode.NotFound,
                data);

        public static Error InvalidValueTypeObtained(string expectedType, string providerName, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.providers.invalid.value.type.obtained",
                $"The value obtained was not of expected type {expectedType}",
                GenerateAutomationErrorMessage(
                    $"The value obtained from a given provider for {providerName} did not resolve to the expected type."),
                HttpStatusCode.PreconditionFailed,
                data);

        public static Error ProviderParameterMissing(
            string parameterName,
            string providerType,
            JObject? data = null,
            IEnumerable<string>? details = null,
            string? reasonWhyParamerWasRequiredIfApplicable = null) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "required.provider.parameter.missing",
                "A required provider parameter was missing",
                $"When trying to resolve a value for a \"{providerType}\" provider, the attempt failed because the required parameter \"{parameterName}\" was missing. "
                + $"{reasonWhyParamerWasRequiredIfApplicable}To resolve the issue, please ensure that all required provider parameters are included and have valid values. "
                + $"If you require further assistance please contact technical support.",
                HttpStatusCode.PreconditionFailed,
                data,
                details);

        public static Error PathQueryValueNotFound(string providerName, JObject data) =>
          AutomationError.GenerateErrorWithAdditionalDetailsFromData(
              "path.query.value.not.found",
              $"The path query was unable to locate a value using the specified query path",
              $"When resolving a value for a \"{providerName}\" provider, the associated path query failed "
              + $"because a value could not be found in the location specified by the query path. "
              + $"To resolve the issue, please ensure that the query path will result in a valid value, "
              + $"or alternatively you can specify a default return value by adding a \"valueIfNotFound\" or \"defaultValue\" property to "
              + $"the \"{providerName}\" provider. If you require further assistance please contact technical support.",
              HttpStatusCode.PreconditionFailed,
              data);

        public static Error PathQueryValueIfNull(string providerName, JObject data) =>
          AutomationError.GenerateErrorWithAdditionalDetailsFromData(
              "path.query.value.is.null",
              $"The path query returned a null value using the specified query path",
              $"When resolving a value for a \"{providerName}\" provider, the associated path query failed "
              + $"because the location specified by the query path contained a null value. To resolve the issue, "
              + $"please ensure that the query path will result in a non null value, "
              + $"or alternatively you can specify a default return value by adding a \"valueIfNull\" or \"defaultValue\" property "
              + $"to the \"{providerName}\" provider. If you require further assistance please contact technical support.",
              HttpStatusCode.PreconditionFailed,
              data);

        public static Error PathQueryValueInvalidType(string providerName, string queryResultValueType, string supportedValueType, JObject data) =>
          AutomationError.GenerateErrorWithAdditionalDetailsFromData(
              "path.query.value.invalid.type",
              $"The path query returned a value with an invalid type",
              $"When resolving a value for a \"{providerName}\" provider, the associated path query failed "
              + $"because the resulting value was of an invalid type (\"{queryResultValueType}\"). The \"{providerName}\" provider "
              + $"can only return values of type \"{supportedValueType}\". To resolve this issue, ensure that the value resulting from "
              + $"the path query is of type \"{supportedValueType}\". If you require further assistance please contact technical support.",
              HttpStatusCode.PreconditionFailed,
              data);

        public static Error RequiredActionParameterValueMissing(string actionType, string parameterName, JObject data, string? reasonWhyParamerWasRequiredIfApplicable = null) =>
          AutomationError.GenerateErrorWithAdditionalDetailsFromData(
              "required.action.parameter.value.missing",
              $"A required action parameter value was missing",
              $"When trying to resolve a value for a \"{actionType}\", the attempt failed because the required parameter \"{parameterName}\" was null. "
              + $"{reasonWhyParamerWasRequiredIfApplicable}To resolve the issue, please ensure that all required action parameters are included and have valid values. "
              + $"If you require further assistance please contact technical support.",
              HttpStatusCode.BadRequest,
              data);

        public static Error RequiredProviderParameterValueMissing(string providerType, string parameterName, JObject data, string? reasonWhyParamerWasRequiredIfApplicable = null) =>
          AutomationError.GenerateErrorWithAdditionalDetailsFromData(
              "required.provider.parameter.value.missing",
              $"A required provider parameter value was missing",
              $"When trying to resolve a value for a \"{providerType}\" provider, the attempt failed because the required parameter \"{parameterName}\" was null. "
              + $"{reasonWhyParamerWasRequiredIfApplicable}To resolve the issue, please ensure that all required provider parameters are included and have valid values. "
              + $"If you require further assistance please contact technical support.",
              HttpStatusCode.BadRequest,
              data);

        public static Error TimeParseFailure(string value, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.providers.time.parse.failure",
                $"We couldn't recognise that as a valid time",
                GenerateAutomationErrorMessage($"When trying to interpret \"{value}\" as a time, it was found not to be in the correct format."),
                HttpStatusCode.BadRequest,
                data);

        public static Error ValueParseFailure(string value, string expectedType, string providerName, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.providers.value.parse.failure",
                $"We couldn't recognise that as a valid {expectedType}",
                GenerateAutomationErrorMessage(
                    $"When trying to interpret \"{value}\" as {ArticleHelper.GetArticle(expectedType)} {expectedType} for provider {providerName}, it was found not to be in the correct format/type."),
                HttpStatusCode.BadRequest,
                data);

        public static Error ValueResolutionError(string providerName, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.providers.value.resolution.error",
                $"An exception was raised when trying to resolve the value from a provider",
                GenerateAutomationErrorMessage($"{providerName} raised an error when trying to resolve its value."),
                HttpStatusCode.PreconditionRequired,
                data);

        public static Error PathSyntaxError(string error, string providerName, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.providers.path.syntax.error",
                $"An exception was raised when trying to resolve an object property by path from a provider",
                GenerateAutomationErrorMessage(
                    $"{providerName} raised an error when trying to resolve its value. The path {data["path"]} has a syntax error: {error}."),
                HttpStatusCode.PreconditionRequired,
                data);

        public static Error PathResolutionError(string providerName, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.providers.path.not.found",
                $"An exception was raised when trying to resolve an object property by path from a provider",
                GenerateAutomationErrorMessage(
                    $"{providerName} raised an error when trying to resolve its value. The segment {data["segment"]} in path {data["path"]} could not be found."),
                HttpStatusCode.ExpectationFailed,
                data);

        public static Error ParameterValueTypeInvalid(
            string providerName,
            string parameterName,
            object? invalidValue = null,
            JObject? data = null,
            IEnumerable<string>? details = null,
            string? reasonWhyValueIsInvalidIfAvailable = null) =>
            AutomationError.GenerateError(
                "provider.parameter.invalid",
                $"A provider parameter had an invalid value",
                $"When trying to resolve a value for a \"{providerName}\" provider, the attempt failed because the value "
                + (string.IsNullOrEmpty(invalidValue?.ToString()) ? string.Empty : invalidValue?.ToString()?.ToLower() + " ") + $"resolved for the \"{parameterName}\" parameter was found to be invalid. "
                + $"{reasonWhyValueIsInvalidIfAvailable}To resolve the issue, please ensure that all provider parameters have valid values. "
                + $"If you require further assistance please contact technical support.",
                HttpStatusCode.PreconditionFailed,
                data,
                details);

        public static Error IntegerValueConvertionFailure(string value, string valueType, string expectedType, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.providers.value.convertion.failure",
                $"The {valueType} value could not be converted into an {expectedType}",
                GenerateAutomationErrorMessage(
                    $"We cannot convert {value} {valueType} value into an {expectedType} because it contains decimal places."),
                HttpStatusCode.PreconditionFailed,
                data);

        public static Error NullValueError(string providerName, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.providers.null.value.error",
                $"{providerName} resolved to a null value but expects a not nullable value",
                GenerateAutomationErrorMessage(
                    $"We're trying to resolve the value of {providerName} but we found a null value."),
                HttpStatusCode.BadRequest,
                data);

        public static Error PathNotFound(
            string providerName,
            string fullPath,
            string resolvedPath,
            string failedSegment,
            JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.providers.path.not.found",
                "The property was not found for the given path",
                GenerateAutomationErrorMessage(
                    $"When processing the provider \"{providerName}\", we were trying "
                    + $"to resolve an object property at the path \"{fullPath}\", however the segment "
                    + $"\"{failedSegment}\" does not exist at path \"{resolvedPath}\".",
                    "Please check that you have specified the correct path, and the object contains the "
                    + "properties you expect."),
                HttpStatusCode.NotFound,
                data);

        public static Error PathResolvesToArrayWhenObjectExpected(
            string providerName,
            string fullPath,
            string resolvedPath,
            string failedSegment,
            JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.providers.path.resolves.to.array.when.object.expected",
                "The path provided resolves to an array when an object was expected",
                GenerateAutomationErrorMessage(
                    $"When processing the provider \"{providerName}\", we were trying "
                    + $"to resolve an object property at the path \"{fullPath}\", however the segment "
                    + $"\"{failedSegment}\" does not exist at path \"{resolvedPath}\", because that "
                    + "segment represents an array and not an object.",
                    "Please check that you have specified the correct path, and the object contains the "
                    + "properties you expect."),
                HttpStatusCode.NotFound,
                data: data);

        public static Error PathResolvesToPrimitiveWhenWhenObjectOrArrayExpected(
            string providerName,
            string fullPath,
            string resolvedPath,
            string failedSegment,
            JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.providers.path.resolves.to.primitive.when.object.or.array.expected",
                "The path provided resolves to a primitive when an object or array was expected",
                GenerateAutomationErrorMessage(
                    $"When processing the provider \"{providerName}\", we were trying "
                    + $"to resolve a property at the path \"{fullPath}\", however the segment "
                    + $"\"{failedSegment}\" at path \"{resolvedPath}\" was a primitive (e.g. a string or number) "
                    + "when we were expecting it to be an array or an object. "
                    + "This can sometimes happen when a json string has not yet been parsed into an object. ",
                    "Please check that you have specified the correct path, and the object contains the "
                    + "properties you expect."),
                HttpStatusCode.NotFound,
                data);

        public static Error IndexOutOfRangeError(
            string providerName,
            string fullPath,
            string resolvedPath,
            int failedIndex,
            int arrayCount,
            JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.providers.path.index.out.of.range",
                "The list did not contain that many elements",
                GenerateAutomationErrorMessage(
                    $"When processing the provider \"{providerName}\", we were trying "
                    + $"to resolve an object property at the path \"{fullPath}\", however the array "
                    + $"at path \"{resolvedPath}\" only contained {arrayCount} elements so retreiving the "
                    + $"element at index {failedIndex} is not possible.",
                    "Please check that you have specified the correct path, and the array or list contains "
                    + "the number of elements you expect."),
                HttpStatusCode.RequestedRangeNotSatisfiable,
                data);

        public static Error ExpressionAliasNotFound(string providerName, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.providers.expression.alias.not.found",
                $"An exception was raised when trying to resolve an object property by path from a provider",
                GenerateAutomationErrorMessage(
                    $"{providerName} raised an error when trying to resolve its value. The alias {data["alias"]} was not found in the current scope."),
                HttpStatusCode.NotFound,
                data: data);

        public static Error DuplicateExpressionAlias(string providerName, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.providers.duplicate.expression.alias",
                $"An exception was raised when trying to resolve an object property by path from a provider",
                GenerateAutomationErrorMessage(
                    $"{providerName} raised an error when trying to resolve its value. The expression alias {data["alias"]} already exists in the current scope."),
                HttpStatusCode.Conflict,
                data: data);

        public static Error InvalidXmlTextValue(string providerName, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.providers.invalid.xml.text.value",
                "Text value must contain valid XML",
                GenerateAutomationErrorMessage(
                    $"Text did not contain valid XML data for {providerName}.",
                    "Please refer to the data in the HTTP response for more details, and check the automation configuration."),
                HttpStatusCode.PreconditionFailed,
                data);

        public static Error ActionExecutionErrorEncountered(string actionAllias, JObject? data = null) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.action.execution.error",
                "An error was encountered when trying to execute an action",
                GenerateAutomationErrorMessage($"An action under this automation has encountered an error when trying to complete execution. Action ID: {actionAllias}."),
                HttpStatusCode.InternalServerError,
                data);

        public static Error ContentBodyTypeNotSupported(string contentBodyTypeName, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.http.content",
                "The requested content body type is not supported",
                GenerateAutomationErrorMessage(
                    $"During the processing of an automation, when trying to generate a HTTP response, we came across a content body of type {contentBodyTypeName} which was not a recognised or supported content body type.",
                    "Please check your automations configuration to ensure you are using the correct provider when defining your http response."),
                HttpStatusCode.NotImplemented,
                data);

        public static Error ExcelJsonPathNotFoundError()
        {
            return new Error(
                "automation.provider.excel.json.path.not.found",
                "No JSON path found in the excel automation template file.",
                GenerateAutomationErrorMessage(
                    $"Please check your excel automation template file to ensure that you've defined "
                    + "the JSON paths required to populate the table columns."),
                HttpStatusCode.NotFound);
        }

        public static Error ExcelProviderOutputFilenameError(JObject errorData, List<string>? additionalDetails = null)
        {
            return AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.excel.provider.output.invalid",
                "Invalid output filename for an excel file.",
                GenerateAutomationErrorMessage(
                    $"You have attempted to set an output filename for this Excel file "
                    + "with an extension that is not valid for an MS Excel file."),
                HttpStatusCode.PreconditionFailed,
                errorData,
                additionalDetails);
        }

        public static Error ExcelProviderSourceFilenameError(JObject errorData, List<string>? additionalDetails = null)
        {
            return AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.excel.provider.source.invalid",
                "Invalid source filename for an excel file.",
                GenerateAutomationErrorMessage(
                    $"The sourceFile property must be an MS Excel filename with a valid extension."),
                HttpStatusCode.PreconditionFailed,
                errorData,
                additionalDetails);
        }

        public static Error ExcelSourceCorruptedOrInvalidFormat(IEnumerable<string> details) =>
            new Error(
                "automation.excel.source.corrupted.or.invalid.format",
                "Source file corrupted or invalid file format",
                GenerateAutomationErrorMessage(
                    "The source file appears to be of a supported format, " +
                    "however an error is generated when trying to open it. " +
                    "It's possible that the file has become corrupted, " +
                    "or that it is of a format that is not consistent with the filename extension."),
                HttpStatusCode.NotAcceptable,
                details);

        public static Error ExcelGenerateContentFailed(IEnumerable<string> details) =>
            new Error(
                "automation.excel.generate.content.failed",
                "Excel generate content failed",
                GenerateAutomationErrorMessage("Failed to generate excel content."),
                HttpStatusCode.NotAcceptable,
                details);

        public static Error ConvertedFileToPdfIsEmpty(JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "pdf.file.empty",
                "Pdf file must have a content",
                "You have converted a file into pdf which is empty, please check the source file",
                httpStatusCode: HttpStatusCode.PreconditionFailed,
                data);

        public static Error PdfFileGenerationFailed(JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "pdf.file.is.null",
                "Exported pdf file is missing",
                "The source file has failed to be exported as pdf",
                httpStatusCode: HttpStatusCode.PreconditionFailed,
                data);

        public static Error PdfSourcesNotFound(List<string> additionalDetails, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automations.source.pdf.build.error",
                "All sources are missing",
                "One of the sources must be defined in automations.json under send email action. Please refer to "
                + "the automations schema for guidance. After adding the source in automations.json sync the "
                + "product that relates to this automation",
                httpStatusCode: HttpStatusCode.NotFound,
                data,
                additionalDetails);

        public static Error ExportingContentToPdfOrMsDocFailed(
            JObject data, string temporaryTargetOutput) =>
            new Error(
                "export.to.msdoc.or.pdf.failed.",
                "Exporting the content to pdf or ms doc failed",
                "There was an unexpected error when it tried to export or save the content of the source template "
                + $"to pdf or ms doc. If the error only occurs while exporting the content of this \"{temporaryTargetOutput}\" "
                + "file then please do the following steps. First, check its content for possible error. Second, "
                + "please check if the source document has bookmarks. If the problem persists on other templates as well "
                + "then please contact your customer support for assistance. "
                + "There is a possibility that the ms office application installed in the server is not "
                + "enabled to support other file formats or the attempt of the system to write the pdf or ms doc "
                + "to a temporary folder is being restricted by the server due to lack of control access. "
                + "We apologise for the inconvenience.",
                HttpStatusCode.BadRequest,
                null,
                data);

        public static Error LoadingOfAssetContentFailed(JObject data, string templateFileName) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "load.asset.content.failed",
                "Reading asset's content failed",
                $"There was an error in retrieving the content of template \"{templateFileName}\". It could be that the "
                + "quote id is mapped to an invalid tenant id, product id or environment. Asset's content contains "
                + "the actual content of the template file. Please contact your customer"
                + " support for assistance. We apologise for the inconvenience.",
                HttpStatusCode.InternalServerError,
                data,
                new List<string>()
                {
                    { $"The name of template is {templateFileName}" },
                });

        public static Error AssetNotFound(string fileName, JObject? data = null) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "asset.not.found",
                "Asset not found",
                $"There was an error in finding asset \"{fileName}\". Asset content contains "
                + "the actual content of the template file. Please contact your customer"
                + " support for assistance. We apologise for the inconvenience.",
                HttpStatusCode.NotFound,
                data,
                new List<string>()
                {
                    { $"The name of asset is {fileName}" },
                });

        public static Error RetrievalOfTemplateNameFailed(JObject data, Exception ex) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "retrieval.template.name.failed",
                "Retrieval of template name failed",
                $"There was an error in retrieving the template because of this error {ex.Message}. It could be "
                + "that the quote id is mapped to an incorrect tenant id, product id or environment. "
                + "Please contact your customer support for assistance. We apologise for the inconvenience.",
                HttpStatusCode.InternalServerError,
                data);

        public static Error WriteContentToTemporaryFileFailed(
            JObject data,
            string temporaryFilePath,
            Exception ex) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "write.to.temporary.file.failed",
                "Writing to a temporary file failed",
                "There was an error in writing the content of the source template into a temporary file "
                + $"\"{temporaryFilePath}\" because of this error \"{ex.Message}\". "
                + "Please contact the customer support for assistance. We apologise for the inconvenience.",
                HttpStatusCode.InternalServerError,
                data);

        public static Error UnableToOpenTemporarySourceTemplate(JObject? data) =>
            new Error(
                "temporary.source.template.cannot.open",
                "Unable to open source template",
                "There was an expected error when opening the temporary source template. There are several possible"
                + " reasons such as file went missing, the server is preventing the tool to open the file due to "
                + "security access restrictions, if you are using a pdfFile provider and you configured the "
                + "automations.json to use productFile as its source file and the outputFilename and filePath are "
                + "using a ms word document then please check the extension they should be using same extension "
                + "because it is likely to cause an error (i.e 'filePath':'sample.docm', 'outputFilename':'output.docx'), "
                + "etc. Please contact your customer support for assistance. We "
                + "apologise for the inconvenience. Please see the additional details.",
                HttpStatusCode.InternalServerError,
                null,
                data);

        public static Error TranslationOfFieldIntoDataFailure(JObject data, string template) =>
            new Error(
                "application.document.translation.field.into.error",
                "A field of applicaton document has error",
                "There was an unexpected error when trying to translate a field into a data which is "
                + $"defined in the  source template \"{template}\". Please check this template for possible error, "
                + "correct it if you found one then product synch it. If the problem still persists, please do not "
                + "hesitate to contact the customer support.",
                HttpStatusCode.BadRequest,
                null,
                data);

        public static Error ExtensionChangeFailed(JObject data, string sourceFilePath) =>
            new Error(
                "extension.change.failed",
                "Changing the file extension had failed",
                $"There was an error during the file extension replacement for source file \"{sourceFilePath}\". "
                + "It could be the source file is empty or file extension is not valid. "
                + "Please contact your customer support for assistance. We apologise for the inconvenience.",
                HttpStatusCode.InternalServerError,
                null,
                data);

        public static Error FailedToReadFromFinalOutput(JObject data, string finalOutputPath) =>
            new Error(
                "read.output.failed",
                "Reading the content from the output file failed",
                "There was an error in reading the final output of the merge field operation. It is possible that "
                + $"the final output \"{finalOutputPath}\" went missing or the server denied the system in reading "
                + "the file due to security access permission. Please contact your customer support. "
                + "We apologise for the inconvenience.",
                HttpStatusCode.InternalServerError,
                null,
                data);

        public static Error MergeFieldCleanUpError(JObject data, string temporaryPath) =>
            new Error(
                "clean.up.error",
                "File clean up failed",
                $"There was an unexpected error in the clean up of temporary file in {temporaryPath} while merging "
                + "data to a template source. This error is not expected to have a huge impact on the over all "
                + "process of merging fields. You can reach out the customer support about this issue. "
                + "We apologise for the inconvenience.",
                HttpStatusCode.FailedDependency,
                null,
                data);

        public static Error UnableToCreateTemporaryPath(JObject data, string fileName) =>
            new Error(
                "temporary.path.creation.failed",
                "The creation of temporary path failed",
                $"There was an error in creating temporary path for \"{fileName}\" due to security restriction. "
                + "The server is not allowing the system due to security access restrictions. "
                + "Please contact your customer support for assistance. We apologise for the inconvenience. ",
                HttpStatusCode.FailedDependency,
                new List<string>()
                {
                    { $"Source file name is {fileName}" },
                },
                data);

        public static Error OutputFileHasInvalidExtensionForItsInputFile(
            JObject data, IList<string> additionalDetails) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "outputfile.extension.not.valid",
                "Output filename has an invalid extension",
                "You have attempted to set an output filename with an extension that mismatches the filename "
                + "extension of the source file. Certain source file types only allow output filenames with "
                + "specific corresponding extensions. Please change the output filename to use a valid extension.",
                HttpStatusCode.BadRequest,
                data,
                additionalDetails);

        public static Error OutputFileHasUnsupportedExtensionForTheCurrentProvider(
            JObject data, IList<string> additionalDetails, string providerName) =>
            FileNameHasUnsupportedExtensionForTheCurrentProvider(data, additionalDetails, providerName, "Output");

        public static Error InputFileHasUnsupportedExtensionForTheCurrentProvider(
            JObject data, IList<string> additionalDetails, string providerName) =>
            FileNameHasUnsupportedExtensionForTheCurrentProvider(data, additionalDetails, providerName, "Input");

        public static Error SupportedExtensionsAreNotDefinedForSourceFileName(
            JObject data, IList<string> additionalDetails, string providerName) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "supported.extension.not.defined",
                "Supported extensions are missing",
                "A supported extensions need to be specified for the defined input file name extension. It is "
                + $"possible that this extension has been newly added extension to be supported by {providerName} "
                + "provider. Please contact you customer support",
                HttpStatusCode.PreconditionFailed,
                data,
                additionalDetails);

        public static Error ServiceProviderNotFound(JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.service.providers.not.found",
                "The service provider from automation data is not found",
                $"When trying to initialize the automation data or load an entity, "
                + "one of the service providers required to complete the process was missing. "
                + "We apologize for the inconvenience, please contact customer support for assistance.",
                HttpStatusCode.NotFound,
                data);

        public static Error PropertyValueInvalid(string propertyName, string automationObject, JObject data) =>
              AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                  "automation.property.value.not.valid",
                  $"One of the properties for {automationObject} is not valid",
                  GenerateAutomationErrorMessage($"The value of the property {propertyName} for {automationObject} is invalid."),
                  HttpStatusCode.PreconditionFailed,
                  data);

        public static Error AdditionalPropertyDefinitionDoesNotExist(string propertyAlias, List<string> additionalDetails, JObject data, string entityTypeName) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.additional.property.definition.does.not.exist",
                "Additional property does not exist",
                $"You have attempted to read the value on an additional property definition with the alias {JsonConvert.SerializeObject(propertyAlias)} on a {entityTypeName.Humanize().ToLower()} entity, " +
                $"however an additional property definition with this alias does not exist on {entityTypeName.Humanize().ToLower()} entities in this context. Before you can succeed with this action, " +
                $"please ensure that an additional property with the alias {JsonConvert.SerializeObject(propertyAlias)} exists on {entityTypeName.Humanize().ToLower()} entities in this context.",
                HttpStatusCode.NotFound,
                data,
                additionalDetails);

        public static Error AdditionalPropertyAliasInvalid(string entityType, string propertyAlias, List<string> additionalDetails, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "read.additional.property.alias.invalid",
                "An additional property with the specified alias was not found",
                $"When trying to read a value from an additional property on {ArticleHelper.GetArticle(entityType.Humanize().ToLower())} {entityType.Humanize().ToLower()} entity, the attempt failed because an additional property with alias {JsonConvert.SerializeObject(propertyAlias)} "
                + $"has not been defined on {entityType.Humanize().ToLower()} entities in the applicable context. To resolve this issue please ensure that the specified additional property alias "
                + $"matches the alias of an additional property defined for the {entityType.Humanize().ToLower()} entity type in the applicable context. "
                + $"If you need further assistance please contact technical support.",
            HttpStatusCode.BadRequest,
            data,
            additionalDetails);

        public static Error AdditionalPropertiesNotSupportedOnEntityType(JObject data, string entityTypeName, List<string>? additionalDetails = null) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
              "automation.additional.properties.not.supported.for.entity.type",
              "Additional properties not supported on entity type",
              $"You have attempted to access an additional property value on a {entityTypeName.Humanize().ToLower()} entity. {entityTypeName.Humanize().ToTitleCase()} entities do not support additional properties. " +
              $"Please ensure that the entity you are trying to set an additional property value on is of a type that support additional properties.",
              HttpStatusCode.ExpectationFailed,
              data,
              additionalDetails);

        public static Error AdditionalPropertyValueMustBeUnique(string propertyAlias, List<string> additionalDetails, JObject data, string entityTypeName) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.action.additional.property.value.must.be.unique",
                "Additional property value must be unique",
                $"This action failed because it would have resulted in two {entityTypeName.Humanize().ToLower()} entities in the same context having the same value for the additional property with alias {JsonConvert.SerializeObject(propertyAlias)}. " +
                    "This additional property requires a unique value for all entities of the same type within the same context. To avoid this error, please ensure that the outcome of this action does not result in two entities having the same value for this additional property.",
                HttpStatusCode.ExpectationFailed,
                data,
                additionalDetails);

        public static Error AdditionalPropertyValueRequired(string propertyAlias, List<string> additionalDetails, JObject data, string entityTypeName) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
              "automation.additional.property.value.required",
              "Empty value not allowed on additional property",
              $"You have attempted to set an empty value on an additional property with the alias {JsonConvert.SerializeObject(propertyAlias)} on a {entityTypeName.Humanize().ToLower()} entity, " +
              $"however that additional property is required, and does not allow an empty value to be set. " +
              $"Before you can succeed with this action, " +
              $"please ensure that the {JsonConvert.SerializeObject(propertyAlias)} additional property on the {entityTypeName.Humanize().ToLower()} entity in this context is not set to 'required', or try to set a value that is not empty.",
              HttpStatusCode.ExpectationFailed,
              data,
              additionalDetails);

        public static Error AdditionalPropertyValueNotSet(string entityType, string propertyAlias, List<string> additionalDetails, JObject data) =>
            AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "read.additional.property.value.not.set",
                "An additional property value had not been set",
                $"When trying to read a value from an additional property on {ArticleHelper.GetArticle(entityType.Humanize().ToLower())} {entityType.Humanize().ToLower()} entity, the attempt failed because "
                + $"a value has not been set on the additional property with alias {JsonConvert.SerializeObject(propertyAlias)} for the specified {entityType.Humanize().ToLower()} entity. "
                + $"To resolve this issue please ensure that the {JsonConvert.SerializeObject(propertyAlias)} additional property on the specified {entityType.Humanize().ToLower()} entity has had a value set. "
                + $"If you need further assistance please contact customer support.",
              HttpStatusCode.ExpectationFailed,
              data,
              additionalDetails);

        public static Error AdditionalPropertyValueInvalidType(string propertyAlias, string resultValueType, string supportedValueType, JObject data) =>
          AutomationError.GenerateErrorWithAdditionalDetailsFromData(
              "automation.additional.property.value.invalid.type",
              $"The additional property text returned a value with an invalid type",
              GenerateAutomationErrorMessage($"When resolving a value for a additional property with the alias {JsonConvert.SerializeObject(propertyAlias)} "
                  + $"resulting value was of an invalid type ({resultValueType}). "
                  + $"The additional property text can only return values of type {supportedValueType}. "
                  + $"To resolve this issue, ensure that the value resulting from the additional property text is of type {supportedValueType}."),
              HttpStatusCode.PreconditionFailed,
              data);

        public static Error HttpTriggersWithTheSameHttpVerbMustHaveUniquePathValues(JObject errorData, List<string>? additionalDetails = null) =>
            AutomationError.GenerateError(
                "httptriggers.with.same.httpverb.must.have.a.unique.path.values",
                "Two or more httpTrigger endpoints with identical paths use the same httpVerb",
                "This is a product misconfiguration. Two or more httpTrigger endpoints using the same httpVerb have path values that are considered identical. " +
                "For a path to be considered unique the static path segments of that path must be unique. " +
                "Alternatively you can ensure that endpoints with non-unique path values use a different httpVerb from each other. " +
                "Please note that any variable names that may be used in a path value will not count when assessing a path for uniqueness. ",
                HttpStatusCode.ExpectationFailed,
                errorData,
                additionalDetails);

        public static Error RelationshipTypeNotValid(string relationshipType)
        {
            var errorMessage = relationshipType == null
                ? "No value for the \"relationshipType\" property was provided, and it is required."
                : $"A value of \"{relationshipType}\" was provided for the relationship type, " +
                $"however that's not one of the allowed relationship types.";
            var validRelationshipTypes = string.Join(", ", Enum.GetNames(typeof(RelationshipType))
                .Select(name => name.ToCamelCase()));
            return AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                "automation.relationship.type.not.valid",
                $"The relationship type is not valid",
                GenerateAutomationErrorMessage(errorMessage),
                HttpStatusCode.PreconditionFailed,
                new JObject(),
                new List<string>() { $"Valid values are: {validRelationshipTypes}" });
        }

        private static Error FileNameHasUnsupportedExtensionForTheCurrentProvider(
            JObject data, IList<string> additionalDetails, string providerName, string fileType)
        {
            var toLowerFileType = fileType.ToLowerInvariant();
            return AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                $"{toLowerFileType}file.extension.not.supported",
                $"{fileType} filename has an invalid extension",
                $"You have attempted to set an {toLowerFileType} filename with an extension that is not supported by the "
                + $"{providerName} provider. Certain providers will only allow {toLowerFileType} filenames with certain "
                + $"extensions. Please change the {toLowerFileType} filename to use a valid extension.",
                HttpStatusCode.BadRequest,
                data,
                additionalDetails);
        }

        private static string GenerateAutomationErrorMessage(
           string specificErrorMessage, string whatToTry = "Please check the automation configuration.") =>
           string.Join(
               Environment.NewLine,
               specificErrorMessage,
               "This is a product misconfiguration, which a product developer needs to fix.",
               whatToTry,
               "If you need assistance or would like to report this to us, please contact customer support.");

        public static class Action
        {
            public static Error AdditionalPropertyValueCannotBeParsedAsInteger(string propertyAlias, string propertyValue, List<string> additionalDetails, JObject data, string entityTypeName) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.additional.property.value.cannot.be.parsed.as.integer",
                    "Additional property value cannot be parsed as integer",
                    $"The value {JsonConvert.SerializeObject(propertyValue)} from the additional {entityTypeName.Humanize().ToLower()} property with alias {JsonConvert.SerializeObject(propertyAlias)} could not be parsed as an integer. " +
                    "To avoid this error, please ensure that the value set on this additional property contains only numeric characters.",
                    HttpStatusCode.ExpectationFailed,
                    data,
                    additionalDetails);

            public static Error SendEmailError(string alias, JObject data) =>
                 AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                     "automation.action.send.email",
                     "An error response was returned by mail server for given request",
                     GenerateAutomationErrorMessage(
                         $"The request fired for the action with alias \"{alias}\" returned an unexpected error.",
                         "Please see additional details."),
                     HttpStatusCode.InternalServerError,
                     data);

            public static Error SmsActionSendError(string alias, string? message, JObject data) =>
                 AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                     "automation.action.sms.provider.send.error",
                     "An error response was returned by SMS client for given request",
                     GenerateAutomationErrorMessage(
                         $"The request fired for the action with alias \"{alias}\" returned an unexpected error"
                         + (string.IsNullOrEmpty(message) ? "." : $" with the message \"{message}\"."),
                         "Please see additional details."),
                     HttpStatusCode.InternalServerError,
                     data: data);

            public static Error ActionRequiresAdditionalPropertyToHaveAValue(string propertyAlias, JObject data, string entityTypeName) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.action.requires.additional.property.to.have.a.value",
                    "Action requires additional property to have a value",
                    $"This action failed because the additional {entityTypeName.Humanize().ToLower()} property with alias {JsonConvert.SerializeObject(propertyAlias)} did not contain a value. " +
                        "To avoid this error, please ensure that a value has been set on this additional property.",
                    HttpStatusCode.ExpectationFailed,
                    data);
        }

        public static class Provider
        {
            public static Error ExpressionMethodNotSupportedForEntityQueries(string providerName, JObject errorData) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.provider.expression.method.not.supported",
                    "Expression method is not supported",
                    $"The use of the {providerName} provider with SQL queries are currently not supported. Please update your automation to ensure that this provider "
                    + "is not used with entityQueryList",
                    HttpStatusCode.NotImplemented,
                    errorData);

            public static Error PropertyKeyInvalid(string providerName, string key, JObject errorData) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.provider.invalid.property.key",
                    "Object property key must have a valid property name",
                    GenerateAutomationErrorMessage(
                        $"The provider {providerName} raised an error while resolving a property value. " +
                        $"A property name in the expected output was configured with a key that is not in the expected syntax. Name: {key}"),
                    HttpStatusCode.ExpectationFailed,
                    errorData);

            public static Error TextFileHasInvalidFileName(List<string> additionalDetails, JObject errorData) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.provider.file.invalid.filename",
                    "Text file must have a valid filename",
                    GenerateAutomationErrorMessage(
                        "An output filename was configured with a value that is not a valid filename.",
                        "Please see additional details in the error response for more information, and check the automation configuration."),
                    HttpStatusCode.PreconditionFailed,
                    errorData,
                    additionalDetails);

            public static Error ProductFileHasInvalidFileName(JObject errorData, List<string>? additionalDetails = null) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.provider.file.invalid.filename",
                    "Product file must have a valid filename",
                    GenerateAutomationErrorMessage(
                        "An output filename was configured with a value that is not a valid filename.",
                        "Please see additional details in the error response for more information, and check the automation configuration."),
                    HttpStatusCode.PreconditionFailed,
                    errorData,
                    additionalDetails);

            public static Error FileAttachmentHasInvalidFileName(JObject errorData, List<string>? additionalDetails = null) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.provider.attachment.invalid.filename",
                    "Attachment must have a valid filename",
                    GenerateAutomationErrorMessage(
                        "An attachment filename was configured with a value that is not a valid filename.",
                        "Please see additional details in the error response for more information, and check the automation configuration."),
                    HttpStatusCode.PreconditionFailed,
                    errorData,
                    additionalDetails);

            public static Error ProductIdBlank(JObject erroData) =>
              AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                  "automation.providers.file.product.id.blank",
                  "No product reference available in automation context",
                  GenerateAutomationErrorMessage(
                    "You have attempted to read a file from a product without providing a specific product reference, and there is no product reference in the automation context.",
                    "Please see additional details in the error response for more information, and check the automation configuration."),
                  HttpStatusCode.NotFound,
                  erroData);

            public static Error ProductNotFound(string productAlias, JObject data) =>
              AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                  "automation.providers.file.product.not.found",
                  "Cannot resolve product reference",
                  GenerateAutomationErrorMessage(
                      $"The product reference ({productAlias}) you have supplied cannot be resolved.",
                      "Please see additional details in the error response for more information, and check the automation configuration."),
                  HttpStatusCode.NotFound,
                  data);

            public static Error FileNotFound(JObject errorData, List<string>? additionalDetails = null) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.providers.file.not.found",
                    "The specified file cannot be found",
                    GenerateAutomationErrorMessage(
                        "The specified file path does not resolve to a file within the product release.",
                        "Please see additional details in the error response for more information, and check the automation configuration."),
                    HttpStatusCode.NotFound,
                    errorData,
                    additionalDetails);

            public static Error InvalidInputData(string expectedFormat, string providerName, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.provider.invalid.input.data",
                    "The input data for this provider was not in the format",
                    $"When trying to process the {providerName} provider, we were expecting the input to be in "
                    + $"{expectedFormat} format, however it was not.",
                    HttpStatusCode.BadRequest,
                    data);

            public static Error PathLookupFileNotFound(JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "path.lookup.file.not.found",
                    "The specified file cannot be found",
                    GenerateAutomationErrorMessage(
                        "objectPathLookupFile did not find a file referenced at the specified path.",
                        "Please see additional details in the error response for more information, and check the automation configuration."),
                    HttpStatusCode.NotFound,
                    data);

            public static Error PathLookupFileImplementationNotSupported(JObject errorData, List<string>? additionalDetails = null) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "path.lookup.file.implementation.not.supported",
                    "The expected implementation is not yet supported",
                    GenerateAutomationErrorMessage(
                        "The expected implementation is not yet supported by the PathLookupFile provider.",
                        "Please see additional details in the error response for more information, " +
                        "and check the automation configuration."),
                    HttpStatusCode.NotImplemented,
                    errorData,
                    additionalDetails);

            public static Error KmlPlacemarksNotFound(double latitude, double longitude, List<string> additionalDetails, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.providers.kml.placemarks.not.found",
                    "KML Placemarks cannot be found",
                    $"KML Placemarks at the coordinate (Latitude: {latitude} Longitude: {longitude}) was not found. " +
                    "Please try and input another proper coordinate value. " +
                    "If you need assistance or would like to report this to us, please contact customer support.",
                    HttpStatusCode.NotFound,
                    data,
                    additionalDetails: additionalDetails);

            public static Error InvalidKmlData(List<string> additionalDetails, JObject data) =>
               new Error(
                   "automation.providers.kml.placemark.invalid.kmldata",
                   "The provided KML Data is invalid",
                   GenerateAutomationErrorMessage(
                        "The specified KML Data format is invalid.",
                        "Please see additional details in the error response for more information, and check the automation configuration."),
                   additionalDetails: additionalDetails,
                   data: data);

            public static class Entity
            {
                public static Error NotFound(string type, string referenceType, string reference, JObject data, List<string>? additionalDetails = null)
                {
                    var referenceTypeCamelize = referenceType.EndsWith("id", StringComparison.OrdinalIgnoreCase) ? "GUID" : $"\"{referenceType.Camelize()}\"";
                    var typeCamelize = type.Camelize();
                    return AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        "automation.providers.entity.not.found",
                        "Cannot resolve entity reference",
                        $"When trying to resolve a {typeCamelize} reference using a {referenceTypeCamelize}, "
                        + $"the attempt failed because the {referenceTypeCamelize} did not match the {typeCamelize} {(referenceTypeCamelize == "GUID" ? "ID" : referenceTypeCamelize)} of a valid {typeCamelize} entity. "
                        + $"To resolve this issue, please ensure that the specified {referenceTypeCamelize} identifies a valid {typeCamelize} entity, "
                        + $"or provide an alternative way to identify the {typeCamelize}"
                        + $"{(typeCamelize == "policy" ? $" (for example using a policy number and environment)." : ".")} "
                        + "If you need further assistance please contact technical support.",
                        httpStatusCode: HttpStatusCode.NotFound,
                        data,
                        additionalDetails);
                }

                public static Error NoEntityType(JObject data, List<string>? additionalDetails = null) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        "automation.providers.entity.not.found",
                        "Cannot resolve entity reference",
                        $"You have attempted to resolve an entity using a dynamic entity provider, but the type of "
                        + $"the entity was not resolved.",
                        httpStatusCode: HttpStatusCode.NotFound,
                        data,
                        additionalDetails);

                public static Error TypeNotSupported(string type, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        "automation.providers.entity.type.not.supported",
                        $"We don't support creating an object representation of a {type}",
                        $"You have attempted to create an object representation of an entity of a {type} that does not support object representations. " +
                        "We apologise for the inconvenience. Currently supported entity types are quote, quoteVersion, policy, policyTransaction, claim, claimVersion, customer, user, product, tenant, organisation, email and document. " +
                        "Please check your configuration and try again. " +
                        "For any questions, please contact customer support for assistance.",
                        httpStatusCode: HttpStatusCode.BadRequest,
                        data);

                public static Error AttachmentNotSupported(string type, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        "automation.providers.entity.attachment.not.supported",
                        $"{type} entity does not support file attachments",
                        $"You have attempted to attach a file to {type} which does not support file attachments. We apologise for the inconvenience. " +
                        "Here's the list of entities that currently support file attachment - [quote, quoteVersion, claim, claimVersion, policyTransaction]." +
                        "Please check your configuration and try again. For any questions, please contact customer support for assistance.",
                        httpStatusCode: HttpStatusCode.BadRequest,
                        data ?? new JObject()
                        {
                            { "entityType", type },
                        });

                public static Error EntityNotFoundForAttachment(string type) =>
                   AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                       "automation.providers.entity.not.found",
                       $"{type} entity cannot be found",
                       $"You have attempted to attach a file to {type} entity, however the entity was found to be missing. We apologise for the inconvenience. " +
                       "Please contact customer support for assistance.",
                       httpStatusCode: HttpStatusCode.BadRequest,
                       new JObject()
                       {
                            { "entityType", type },
                       });

                public static Error EntityNotInContext(string path, JObject data, List<string>? additionalDetails = null) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        "automation.providers.entity.context.entity.not.found",
                        "The entity reference being requested cannot be found.",
                        $"You have attempted to obtain the representation of an entity from the current context thru the path {path} which does not exist. " +
                        "Please check that your configuration and automation request does indeed pass the necessary parameters to obtain the reference. " +
                        "For any questions, please contact customer support for assistance.",
                        httpStatusCode: HttpStatusCode.NotFound,
                        data,
                        additionalDetails);
            }

            public static class PatchObject
            {
                // This error is only used internally.
                public static Error PathAlreadyExists(string operation, string errorTitle, JObject debugContext) =>
                      AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                          "automation.providers.patchObject.path.already.exists",
                          errorTitle,
                          $"The {operation} operation failed because the path is already existing in the object.",
                          HttpStatusCode.Conflict,
                          debugContext);

                // This error is only used internally.
                public static Error PathNotFound(string operation, string errorTitle, JObject debugContext) =>
                      AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                          "automation.providers.patchObject.path.not.found",
                          errorTitle,
                          $"The {operation} operation failed because the path does not exists in the object.",
                          HttpStatusCode.NotFound,
                          debugContext);

                public static Error PatchOperationFailed(HttpStatusCode statusCode, JObject data, List<string> additionalDetails) =>
                      AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                          "automation.providers.patchObject.operaton.failed",
                          $"The patch operation failed",
                          $"A patchObject provider failed to perform a patch operation. Please check the error details and adjust your configuration accordingly.",
                          statusCode,
                          data,
                          additionalDetails);

                public static Error CannotAddPrimitiveToObject(string value, JObject data, List<string> additionalDetails) =>
                      AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                          "automation.providers.patchObject.cannot.add.primitive.to.object",
                          $"You can't add a primitive to an object without a property name",
                          $"When trying to patch an object with the \"add\" operation, a primitive value "
                          + $"\"{value}\" was resolved, and the path chosen resolved to an object. "
                          + "You can't simply add a primitive value to an object. It needs "
                          + "to have a property name to hold the value.",
                          HttpStatusCode.BadRequest,
                          data,
                          additionalDetails);

                public static Error CannotCopyPrimitiveToObject(string value, JObject data, List<string> additionalDetails) =>
                      AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                          "automation.providers.patchObject.cannot.copy.primitive.to.object",
                          $"You can't copy a primitive to an object without a property name",
                          $"When trying to patch an object with the \"copy\" operation, a primitive value "
                          + $"\"{value}\" was resolved, and the target chosen resolved to an "
                          + "object. You can't simply copy a primitive value to an object. It needs "
                          + "to have a property name to hold the value.",
                          HttpStatusCode.BadRequest,
                          data,
                          additionalDetails);

                public static Error CannotMovePrimitiveToObject(string value, JObject data, List<string> additionalDetails) =>
                      AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                          "automation.providers.patchObject.cannot.move.primitive.to.object",
                          $"You can't move a primitive to an object without a property name",
                          $"When trying to patch an object with the \"move\" operation, a primitive value "
                          + $"\"{value}\" was resolved, and the target chosen resolved to an "
                          + "object. You can't simply copy a primitive value to an object. It needs "
                          + "to have a property name to hold the value.",
                          HttpStatusCode.BadRequest,
                          data,
                          additionalDetails);
            }
        }

        public static class Trigger
        {
            public static Error TriggerParameterMissing(string propertyName, bool startWithAn, string triggerType, string? reasonParameterIsRequired = null) =>
                AutomationError.GenerateError(
                    "required.trigger.parameter.missing",
                    "A required trigger parameter is missing",
                    $"When processing the automation configuration for {(startWithAn ? "an" : "a")} {triggerType}, the attempt failed because the required parameter \"{propertyName}\" was missing."
                    + (reasonParameterIsRequired.IsNullOrEmpty() ? string.Empty : " " + reasonParameterIsRequired) + $" To resolve the issue, please ensure that all required parameters are included and have valid values. If you require "
                    + "further assistance please contact technical support.",
                    HttpStatusCode.PreconditionFailed,
                    null);
        }

        public static class HttpRequest
        {
            public static Error HttpRequestError(string alias, string message, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.action.http.request.failed",
                    "The outbound HTTP request failed.",
                    GenerateAutomationErrorMessage(
                        $"The HTTP request action with alias \"{alias}\" failed."
                        + (message != null ? $" {message}".WithDot() : string.Empty)),
                    HttpStatusCode.InternalServerError,
                    data);

            public static Error HttpResponseError(string alias, string message, HttpStatusCode httpStatusCode, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.action.http.response",
                    "An error response was returned by the external API.",
                    GenerateAutomationErrorMessage(
                        $"The HTTP request action with alias \"{alias}\" returned an error."
                        + (message != null ? $" {message}".WithDot() : string.Empty)),
                    HttpStatusCode.BadRequest,
                    data);

            public static Error HttpResponseGenerationError(string message, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.http.response.generation.failed",
                    "Error encountered when generating a response.",
                    GenerateAutomationErrorMessage(
                        $"An error was encountered when trying to generate the response for an automation request."
                        + (message.IsNotNullOrEmpty() ? $" {message}".WithDot() : string.Empty)),
                    HttpStatusCode.PreconditionFailed,
                    data);

            public static Error ExtensionPointGenerationError(string message, JObject data) =>
              AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                  "automation.extensionpoint.response.failed",
                  "Error encountered when finishing a request to the extension point.",
                  GenerateAutomationErrorMessage(
                      $"An error was encountered when trying to finish a request to the extension point."
                      + (message.IsNotNullOrEmpty() ? $" {message}".WithDot() : string.Empty)),
                  HttpStatusCode.PreconditionFailed,
                  data);

            public static Error CustomCertificateUnsupportedFormat(string format, JObject data, List<string>? additionalDetails = null) =>
                AutomationError.GenerateError(
                    "automation.http.client.certificate.unsupported.format",
                    "Unsupported certificate format",
                    GenerateAutomationErrorMessage($"The specified certificate format is not currently supported. " +
                        $"Currently only certificates in PFX format are supported. " +
                        $"To resolve this issue, change the value of the \"format\" property to \"pfx\" and " +
                        "ensure that the certificate you are trying to use is valid binary data in PFX format."),
                    HttpStatusCode.BadRequest,
                    data,
                    additionalDetails);

            public static Error CustomCertificateNoPasswordProtection(JObject data, List<string>? additionalDetails = null) =>
                AutomationError.GenerateError(
                    "automation.http.client.certificate.not.password.protected",
                    "Certificate data is not password protected",
                    GenerateAutomationErrorMessage($"The certificate data you have attempted to import is not password protected. "
                        + "For security reasons it's a requirement that all certificates used for outbound HTTP "
                        + "request must be protected by password. Please add password protection to the certificate data in order to "
                        + "resolve this issue."),
                    HttpStatusCode.BadRequest,
                    data,
                    additionalDetails);

            public static Error CustomCertificateIncorrectPassword(JObject data, List<string>? additionalDetails = null) =>
                AutomationError.GenerateError(
                    "automation.http.client.certificate.incorrect.password",
                    "Incorrect password for certificate data",
                    GenerateAutomationErrorMessage($"You have attempted to import certificate data using an incorrect password. "
                        + "Please check that you have used the correct credentials to resolve this issue."),
                    HttpStatusCode.BadRequest,
                    data,
                    additionalDetails);

            public static Error CustomCertificateInvalidDataFormat(string format, JObject data, List<string>? additionalDetails = null) =>
              AutomationError.GenerateError(
                  "automation.http.client.certificate.invalid.data.format",
                  "Invalid certificate data format",
                  GenerateAutomationErrorMessage($"The certificate data you have tried to import is not valid {format?.ToUpper()} data. "
                      + $"To resolve this issue, please ensure that the certificate data matches the specified format."),
                  HttpStatusCode.BadRequest,
                  data,
                  additionalDetails);

            public static Error CustomCertificateInvalidCertificate(JObject data, List<string>? additionalDetails = null) =>
              AutomationError.GenerateError(
                  "automation.http.client.certificate.invalid.certificate",
                  "Invalid certificate",
                  GenerateAutomationErrorMessage($"The certificate data you have tried to import contains at least one certificate that is not valid. " +
                      "To resolve this issue, please ensure that all certificates in the imported certificate data are valid."),
                  HttpStatusCode.BadRequest,
                  data,
                  additionalDetails);

            public static Error InvalidHttpVerbFormat(string httpVerb, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.action.http.verb.invalid",
                    "HttpVerb format is invalid",
                    GenerateAutomationErrorMessage("HttpVerb used in the request has an invalid format."),
                    HttpStatusCode.BadRequest,
                    data);

            public static Error PathSegmentObsolete(
                string oldPathSegment,
                string newPathSegment,
                List<string>? additionalDetails = null,
                JObject? data = null) => new Error(
                    "automation.action.http.pathsegment.obsolete",
                    "That automations path segment name is obsolete",
                    $"You've used the path segment name \"{oldPathSegment}\" which is obsolete. Please use \"{newPathSegment}\" instead.",
                    HttpStatusCode.BadRequest,
                    additionalDetails,
                    data);
        }

        public static class VariableAction
        {
            public static Error VariableSettingError(string alias, string message, JObject? data = null) =>
                 AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                     "automation.action.set.variable.failed",
                     "The Set Variable Action Failed.",
                     GenerateAutomationErrorMessage(
                         $"The set variable action with alias \"{alias}\" failed."
                         + (message != null ? $" {message}".WithDot() : string.Empty)),
                     HttpStatusCode.BadRequest,
                     data: data);
        }

        public static class CreateUserAction
        {
            public static Error AccountEmailAddressIsAlreadyInUse(string accountEmailAddress, JObject data) =>
               AutomationError.GenerateError(
                   "automation.create.user.action.account.email.already.in.use",
                   "The account email address is already in use",
                   GenerateAutomationErrorMessage(
                       $"The account email address \"{accountEmailAddress}\" is already in use by an existing user for the same organisation.",
                       "Please use a different email address that is unique."),
                   HttpStatusCode.BadRequest,
                   data: data);

            public static Error IsNotAustralianPhoneNumber(string phoneNumber, JObject data) =>
                AutomationError.GenerateError(
                   "automation.create.user.action.invalid.phone.number",
                   "The phone number is not valid",
                   GenerateAutomationErrorMessage(
                       $"The phone number \"{phoneNumber}\" is not a valid australian phone number.",
                       "Please use valid Australian phone number of the following format e.g. 0x xxxx xxxx, or +61 x xxxx xxxx."),
                   HttpStatusCode.BadRequest,
                   data: data);

            public static Error AccountEmailAddressInvalid(string emailAddress, JObject data) =>
               AutomationError.GenerateError(
                  "automation.create.user.action.account.email.address.invalid",
                  "The account email address is invalid",
                  GenerateAutomationErrorMessage(
                      $"The account email address \"{emailAddress}\" is invalid.",
                      "Please input a proper account email address in the format of 'xxx@xx.xx'."),
                  HttpStatusCode.BadRequest,
                  data: data);

            public static Error EmailAddressInvalid(string emailAddress, JObject data) =>
               AutomationError.GenerateError(
                  "automation.create.user.action.email.address.invalid",
                  "The email address is invalid",
                  GenerateAutomationErrorMessage(
                      $"The email address \"{emailAddress}\" is invalid.",
                      "Please input a proper email address in the format of 'xxx@xx.xx'"),
                  HttpStatusCode.BadRequest,
                  data: data);

            public static Error OrganisationIsNotValidInThisTenancy(string organisationAlias, JObject data) =>
               AutomationError.GenerateError(
                   "automation.create.user.action.organisation.id.is.not.valid",
                   "The organisation ID is not valid",
                   GenerateAutomationErrorMessage(
                       $"The specified organisation ID is not an ID of a valid organisation in this tenancy.",
                       "Please input a valid organisation ID that belongs to the tenancy."),
                   HttpStatusCode.BadRequest,
                   data: data,
                   new List<string> { $"Organisation Alias: {organisationAlias}" });

            public static Error AssigningInvalidRole(string roleName, JObject data) =>
              AutomationError.GenerateError(
                  "automation.create.user.action.invalid.role.name",
                  "The role name is invalid",
                  GenerateAutomationErrorMessage(
                      $"The role name \"{roleName}\" being assigned is not able to be resolved as a valid client role in the tenancy.",
                      "Please input a proper role name that corresponds to a matching non-deleted client role in the tenancy."),
                  HttpStatusCode.BadRequest,
                  data: data);
        }

        public static class Archive
        {
            public static Error FormatNotSpecified(JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.archive.format.not.specified",
                    "You didn't specify an archive format",
                    $"When trying to create or open an archive, you didn't specify the format, and we couldn't infer "
                    + "the format from the filename. "
                    + "Please ensure you specify an archive format, e.g. \"zip\".",
                    HttpStatusCode.UnsupportedMediaType,
                    data: data);

            public static Error UnsupportedFormatSpecified(string format, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.archive.unsupported.format.specified",
                    "We don't support that format for archives",
                    $"When trying to operate on an archive, you specified the format {format}, however we don't "
                    + "support that format. Currently the only format we do support is \"zip\". Please get in "
                    + "touch with our support team to request that we add support for your format.",
                    HttpStatusCode.UnsupportedMediaType,
                    data: data);

            public static Error NotAValidZipArchive(string message, JObject? data = null) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.archive.not.a.valid.zip.archive",
                    "That's not a valid Zip archive",
                    $"When trying to read a Zip stream, the data was not actually a valid Zip archive, so it could not be read. "
                    + message,
                    HttpStatusCode.BadRequest,
                    data: data);

            public static Error EntryNotFound(string filePath, JObject? data = null) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.archive.entry.not.found",
                    "We couldn't find that archive entry",
                    $"When trying to read a Zip stream, an entry with the path {filePath} could not be found. "
                    + "Please check that you have the correct archive, containing the files you expect.",
                    HttpStatusCode.NotFound,
                    data: data);

            public static Error NoPasswordSupplied(JObject? data = null) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.archive.no.password.supplied",
                    "We need a password to extract from that archive",
                    $"When trying to read a password protected Zip stream, no password was supplied. "
                    + "Please ensure you provide a password.",
                    HttpStatusCode.BadRequest,
                    data: data);

            public static Error WrongPasswordSupplied(JObject? data = null) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.archive.wrong.password.supplied",
                    "The archive password was incorrect",
                    $"When trying to read a password protected Zip stream, the wrong password was supplied. "
                    + "Please ensure you provide the correct password.",
                    HttpStatusCode.BadRequest,
                    data: data);

            public static Error FilenameRequiredWhenCreatingNewArchive(JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "automation.archive.filename.required.when.creating.new.archive",
                    "An output filename is required when creating a new archive",
                    $"When trying to create a new archive, no output filename for the archive was provided. "
                    + "Please ensure you specify an output filename when creating a new archive.",
                    HttpStatusCode.BadRequest,
                    data: data);

            public class AddFileOperation
            {
                public static Error DestinationFolderNotFound(string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        "automation.archive.add.file.operation.destination.folder.not.found",
                        "There was no such path in the archive",
                        $"When trying to add a file to the archive with a destination folder \"{path}\", that path "
                        + "was not found in the archive. Either create a folder archive entry, or configure "
                        + "the add operation to automatically create the folder when it doesn't exist.",
                        HttpStatusCode.BadRequest,
                        data: data);

                public static Error EntryAlreadyExists(string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        "automation.archive.add.file.operation.entry.already.exists",
                        "An archive entry with that path already exists",
                        $"When trying to add a file to the archive with path \"{path}\", an entry "
                        + "was found with the same path. Make sure the archive doesn't contain an entry "
                        + "with that path, configure the add operation to replace the entry, or configure the "
                        + "add operation to skip when this situation occurs.",
                        HttpStatusCode.BadRequest,
                        data: data);
            }

            public class AddFolderOperation
            {
                public static Error EntryAlreadyExists(string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        "automation.archive.add.folder.operation.entry.already.exists",
                        "An archive entry with that path already exists",
                        $"When trying to add a file to the archive at the path \"{path}\", an entry "
                        + "was found with the same path. Make sure the archive doesn't contain an entry "
                        + "with that path, configure the add operation to replace the file, or configure the "
                        + "add operation to skip when this situation occurs.",
                        HttpStatusCode.BadRequest,
                        data: data);

                public static Error ParentFolderNotFound(string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        "automation.archive.add.folder.operation.path.not.found",
                        "The parent folder didn't exist in the archive",
                        $"When trying to add a folder to the archive at the path \"{path}\", that path "
                        + "was not found in the archive. Create a folder archive entry before hand, or configure "
                        + "the add operation to automatically create the parent folder when it doesn't exist.",
                        HttpStatusCode.BadRequest,
                        data: data);
            }

            public class CopyOperation
            {
                public static Error SourceFileNotFound(string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        "automation.archive.copy.operation.source.file.not.found",
                        "That file didn't exist in the archive",
                        "When trying to copy a file from one location to another inside an archive, the source "
                        + $"file was not found at the location \"{path}\".",
                        HttpStatusCode.BadRequest,
                        data: data);

                public static Error DestinationPathNotFound(string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        "automation.archive.copy.operation.destination.path.not.found",
                        "There was no such path in the archive",
                        $"When trying to copy a file from one location in an archive to another, the destination "
                        + "path \"{path}\" was not found in the archive. Either create a directory archive entry, "
                        + "configure the copy operation to automatically create the path when it doesn't exist, or "
                        + "configure the copy operation to skip when this situation occurs.",
                        HttpStatusCode.BadRequest,
                        data: data);

                public static Error DestinationEntryAlreadyExists(string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        "automation.archive.copy.operation.destination.entry.already.exists",
                        "An archive entry with the destination path already exists",
                        "When trying to copy a file from one location in an archive to another, an existing "
                        + $"entry was found with the destination \"{path}\". Either make sure the archive doesn't "
                        + "contain an entry with that path, configure the copy operation to replace the entry, or "
                        + "configure the copy operation to skip when this situation occurs.",
                        HttpStatusCode.BadRequest,
                        data: data);
            }

            public class MoveOrCopyFileOperation
            {
                public static Error SourceFileNotFound(string moveOrCopy, string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        $"automation.archive.{moveOrCopy}.file.operation.source.file.not.found",
                        "That file didn't exist in the archive",
                        $"When trying to {moveOrCopy} a file from one location to another inside an archive, the source "
                        + $"file was not found at the location \"{path}\".",
                        HttpStatusCode.BadRequest,
                        data: data);

                public static Error DestinationFolderNotFound(string moveOrCopy, string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        $"automation.archive.{moveOrCopy}.file.operation.destination.folder.not.found",
                        "There was no such folder in the archive",
                        $"When trying to {moveOrCopy} a file from one location in an archive to another, the destination "
                        + $"folder \"{path}\" was not found in the archive. Either create the folder archive entry, "
                        + "configure the operation to automatically create the folder when it doesn't exist, or "
                        + "configure the operation to skip when this situation occurs.",
                        HttpStatusCode.BadRequest,
                        data: data);

                public static Error DestinationEntryAlreadyExists(string moveOrCopy, string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        $"automation.archive.{moveOrCopy}.file.operation.destination.entry.already.exists",
                        "An archive entry with the destination path already exists",
                        $"When trying to {moveOrCopy} a file from one location in an archive to another, an existing "
                        + $"entry was found with the destination \"{path}\". Either make sure the archive doesn't "
                        + "contain an entry with that path, configure the operation to replace the entry, or "
                        + "configure the operation to skip when this situation occurs.",
                        HttpStatusCode.BadRequest,
                        data: data);
            }

            public class MoveOrCopyFolderOperation
            {
                public static Error SourceFolderNotFound(string moveOrCopy, string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        $"automation.archive.{moveOrCopy}.folder.operation.source.folder.not.found",
                        "That folder didn't exist in the archive",
                        $"When trying to {moveOrCopy} a folder from one location to another inside an archive, "
                        + $"the source folder was not found at the location \"{path}\".",
                        HttpStatusCode.BadRequest,
                        data: data);

                public static Error DestinationFolderParentNotFound(string moveOrCopy, string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        $"automation.archive.{moveOrCopy}.folder.operation.destination.folder.parent.not.found",
                        "There was no such folder in the archive",
                        $"When trying to {moveOrCopy} a folder from one location in an archive to another, the destination "
                        + $"folder parent \"{path}\" was not found in the archive. Either create the parent folder archive entry, "
                        + "configure the operation to automatically create the folder when it doesn't exist, or "
                        + "configure the operation to skip when this situation occurs.",
                        HttpStatusCode.BadRequest,
                        data: data);

                public static Error DestinationFileExists(string moveOrCopy, string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        $"automation.archive.{moveOrCopy}.folder.operation.destination.file.already.exists",
                        $"A file exists at the location you're trying to {moveOrCopy} the folder to",
                        $"When trying to {moveOrCopy} a folder from one location in an archive to another, a file exists "
                        + $"at the destination location \"{path}\". Either make sure the archive doesn't "
                        + "contain a file with that path, configure the operation to replace the entry, or "
                        + "configure the operation to skip when this situation occurs.",
                        HttpStatusCode.BadRequest,
                        data: data);

                public static Error DestinationFolderExists(string moveOrCopy, string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        $"automation.archive.{moveOrCopy}.folder.operation.destination.folder.already.exists",
                        $"A folder exists at the location you're trying to {moveOrCopy} the folder to",
                        $"When trying to {moveOrCopy} a folder from one location in an archive to another, a folder exists "
                        + $"at the destination location \"{path}\". Either make sure the archive doesn't "
                        + "contain a folder with that path, configure the operation to replace the entry, "
                        + "configure the operation to merge the folder into the other, or "
                        + "configure the operation to skip when this situation occurs.",
                        HttpStatusCode.BadRequest,
                        data: data);
            }

            public class RemoveEntryOperation
            {
                public static Error EntryNotFound(string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        "automation.archive.remove.entry.operation.entry.not.found",
                        "An entry with that path didn't exist in the archive",
                        "When trying to remove an entry from an archive, no such entry was found to exist with "
                        + $"the path \"{path}\".",
                        HttpStatusCode.BadRequest,
                        data: data);
            }

            public class ReplaceFileOperation
            {
                public static Error FileNotFound(string path, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        "automation.archive.replace.file.operation.file.not.found",
                        "That file didn't exist in the archive",
                        "When trying to replace a file at a location inside an archive, no such file was found "
                        + $"at the location \"{path}\".",
                        HttpStatusCode.BadRequest,
                        data: data);

                public static Error FolderNotFound(string filePath, string folderPath, JObject data) =>
                    AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                        "automation.archive.replace.operation.folder.not.found",
                        "There was no such folder in the archive",
                        $"When trying to replace the file at the location \"{filePath}\" in an archive, the file "
                        + "was not found, and the operation was configured to add the file when it doesn't "
                        + "already exist in the archive. "
                        + "Unfortunately, we couldn't do that because the parent folder \"{folderPath}\" also "
                        + "didn't exist. It's possible to configure the operation to automatically create the "
                        + "parent folder, however that hasn't been done in this case.",
                        HttpStatusCode.BadRequest,
                        data: data);
            }
        }

        public static class ApproveQuoteAction
        {
            public static Error IncompleteWithoutBindingQuote(Guid quoteId, string quoteState, string calculationState, JObject? data = null) =>
                AutomationError.GenerateError(
                    "automation.approve.quote.action.invalid.calculation.state",
                    "Quote cannot be approved with this calculation state",
                    GenerateAutomationErrorMessage(
                        $"You are trying to approve a quote with an invalid calculation state. "
                        + "Please ensure that the calculation associated with the quote you are trying to approve is in \"bindingQuote\" state before attempting this action."),
                    HttpStatusCode.BadRequest,
                    data: data,
                    new List<string> { "Quote ID: " + quoteId, "Quote State: " + quoteState, "Calculation State: " + calculationState });

            public static Error InvalidQuoteStateForApproval(Guid quoteId, string quoteState, JObject? data = null) =>
                 AutomationError.GenerateError(
                     "automation.approve.quote.action.invalid.quote.state",
                     "Quote cannot be approved in this state.",
                     GenerateAutomationErrorMessage(
                         $"You are trying to approve a quote that is in an invalid state. Please ensure that the quote you are trying to approve is in \"incomplete\", \"review\" or \"endorsement\" state before attempting this action."),
                     HttpStatusCode.BadRequest,
                     data: data,
                     new List<string> { "Quote ID: " + quoteId, "Quote State: " + quoteState });
        }

        public static class CreateOrganisationAction
        {
            public static Error OrganisationNameInvalid(string organisationName, JObject? data = null) =>
                 AutomationError.GenerateError(
                     "automation.create.organisation.action.organisation.name.invalid",
                     "The organisation name is invalid",
                     GenerateAutomationErrorMessage(
                         $"The organisation name must start with a letter, and may only contain letters, spaces, hyphens, apostrophes, commas and period characters."),
                     HttpStatusCode.BadRequest,
                     data: data,
                     new List<string> { "Organisation Name: " + organisationName });

            public static Error OrganisationAliasInvalid(string organisationAlias, JObject? data = null) =>
             AutomationError.GenerateError(
                 "automation.create.organisation.action.organisation.alias.invalid",
                 "The organisation alias is invalid",
                 GenerateAutomationErrorMessage(
                     $"The organisation alias must start with a letter, must only contain lowercase alphabetic characters, numbers and hyphens. It must not start or end in hyphen."),
                 HttpStatusCode.BadRequest,
                 data: data,
                 new List<string> { "Organisation Alias: " + organisationAlias });

            public static Error OrganisationNameExists(string tenantAlias, string organisationName, JObject? data = null) =>
                 AutomationError.GenerateError(
                     "automation.create.organisation.action.organisation.name.already.in.use",
                     "The organisation name is already in use",
                     GenerateAutomationErrorMessage(
                         $"You tried to create a new organisation using a name \"{organisationName}\" that is already in use by an existing organisation in this tenancy. The name of your new organisation must be unique."),
                     HttpStatusCode.BadRequest,
                     data: data,
                     new List<string> { "Tenant Alias: " + tenantAlias, "Organisation Name: " + organisationName });

            public static Error OrganisationAliasNonUnique(string tenantAlias, string organisationAlias, JObject? data = null) =>
             AutomationError.GenerateError(
                 "automation.create.organisation.action.organisation.alias.already.in.use",
                 "The organisation alias is already in use",
                 GenerateAutomationErrorMessage(
                     $"You tried to create a new organisation using an alias \"{organisationAlias}\" that is already in use by an existing organisation in this tenancy. The alias of your new organisation must be unique."),
                 HttpStatusCode.BadRequest,
                 data: data,
                 new List<string> { "Tenant Alias: " + tenantAlias, "Organisation Alias: " + organisationAlias });
        }

        public static class UploadFileAction
        {
            public static Error InvalidProtocol(string protocol, JObject? data = null) =>
                AutomationError.GenerateError(
                         "file.upload.protocol.invalid",
                         "The protocol to upload file is invalid",
                         GenerateAutomationErrorMessage(
                             $"The protocol {protocol} is invalid. " +
                             $"The value currently allowed is 'SFTP', this may be updated in the future. "),
                         HttpStatusCode.BadRequest,
                         data: data,
                         new List<string> { });

            public static Error PasswordIncorrect(string protocol, string hostname, JObject? data = null) =>
                 AutomationError.GenerateError(
                     "file.upload.password.incorrect",
                     $"The password provided for the {protocol} protocol is incorrect",
                     GenerateAutomationErrorMessage(
                         $"The password provided for the host \"{hostname}\" is incorrect. " +
                         $"Please try again and use another password to properly setup the connection to the {protocol} protocol. "),
                     HttpStatusCode.Unauthorized,
                     data: data,
                     new List<string> { });

            public static Error PrivateKeyIncorrect(string protocol, string hostname, JObject? data = null) =>
                 AutomationError.GenerateError(
                     "file.upload.private.key.invalid",
                     $"The private key or password provided for the {protocol} protocol is incorrect",
                     GenerateAutomationErrorMessage(
                         $"The private key or password provided for the host \"{hostname}\" is incorrect. " +
                         $"Please try again and use another PEM file or password to properly setup the connection to the {protocol} protocol. "),
                     HttpStatusCode.Unauthorized,
                     data: data,
                     new List<string> { });

            public static Error HostnameInvalid(string protocol, string hostname, JObject? data = null) =>
                 AutomationError.GenerateError(
                     "file.upload.hostname.invalid",
                     $"The hostname provided for the {protocol} protocol is invalid or unreachable",
                     GenerateAutomationErrorMessage(
                         $"The provided hostname \"{hostname}\" is invalid or unreachable at the moment. " +
                         $"Please provide a different hostname or try again later. "),
                     HttpStatusCode.BadRequest,
                     data: data,
                     new List<string> { });

            public static Error TimeoutException(string protocol, string hostname, JObject? data = null) =>
                 AutomationError.GenerateError(
                     "file.upload.timeout.exception",
                     $"Connecting to host timed out",
                     GenerateAutomationErrorMessage(
                         $"A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond. " +
                         $"Please try connecting again later or try to connect to the proper VPN server. "),
                     HttpStatusCode.BadRequest,
                     data: data,
                     new List<string> { "Protocol: " + protocol, "Hostname: " + hostname });

            public static Error ConnectionRefusedException(string protocol, string hostname, JObject? data = null) =>
                AutomationError.GenerateError(
                    "file.upload.connection.refused.exception",
                    $"Host refused establishing a connection",
                    GenerateAutomationErrorMessage(
                        $"Target host refused establishing a connection to the client." +
                        $"Please check the credentials required for connecting to the target host."),
                    HttpStatusCode.BadRequest,
                    data: data,
                    new List<string> { "Protocol: " + protocol, "Hostname: " + hostname });

            public static Error PermissionDenied(string protocol, string hostname, string? path, JObject? data = null) =>
                 AutomationError.GenerateError(
                     "file.upload.permission.denied",
                     $"The authenticated user access was denied",
                     GenerateAutomationErrorMessage(
                         $"The authenticated user does not have read and write access to the remote folder{(!string.IsNullOrEmpty(path) ? path : "./")} or does not have permission to overwrite the existing file in the target location. "),
                     HttpStatusCode.Forbidden,
                     data: data,
                     new List<string> { "Protocol: " + protocol, "Hostname: " + hostname, "Remote Path: " + (!string.IsNullOrEmpty(path) ? path : "./") });

            public static Error WriteAccessDenied(string protocol, string hostname, string? path, JObject? data = null) =>
              AutomationError.GenerateError(
                  "file.upload.write.access.denied",
                  $"The authenticated user write access was denied",
                  GenerateAutomationErrorMessage(
                      $"The authenticated user does not have permission to overwrite the existing file in the target folder or does not have permission to write to the target folder in general. "),
                  HttpStatusCode.Forbidden,
                  data: data,
                  new List<string> { "Protocol: " + protocol, "Hostname: " + hostname, "Remote Path: " + (!string.IsNullOrEmpty(path) ? path : "./") });

            public static Error OverrideExistingFileDenied(string protocol, string fileName, string hostname, string? path, JObject? data = null) =>
              AutomationError.GenerateError(
                  "file.upload.duplicate.filename.in.target.location",
                  $"A file with the same name exists in the target location",
                  GenerateAutomationErrorMessage(
                      $"When trying to upload a file with the name \"{fileName}\" to the remote path \"{(!string.IsNullOrEmpty(path) ? path : "./")}\" an existing file with the same name was found there. " +
                      $"This issue can be resolved by using a unique filename for the uploaded file, or by including the replaceExistingFile parameter with a \"true\" value. "),
                  HttpStatusCode.Forbidden,
                  data: data,
                  new List<string> { "Protocol: " + protocol, "Hostname: " + hostname, "Remote Path: " + (!string.IsNullOrEmpty(path) ? path : "./") });

            public static Error CreateFolderDenied(string protocol, string hostname, string? path, JObject? data = null) =>
              AutomationError.GenerateError(
                  "file.upload.remote.path.create.folder.denied",
                  $"The authenticated user permission to create folder was denied",
                  GenerateAutomationErrorMessage(
                      $"The authenticated user does not have permission to create a folder in the target location. "),
                  HttpStatusCode.Forbidden,
                  data: data,
                  new List<string> { "Protocol: " + protocol, "Hostname: " + hostname, "Remote Path: " + (!string.IsNullOrEmpty(path) ? path : "./") });

            public static Error PathDoesntExist(string protocol, string hostname, string? path, JObject? data = null) =>
              AutomationError.GenerateError(
                  "file.upload.remote.path.does.not.exist",
                  $"The remote path does not exist",
                  GenerateAutomationErrorMessage(
                      $"The remote path \"{path}\" does not exist and createMissingFolder is either set to \"false\" or is omitted. " +
                      $"This can be resolved by setting the createMissingFolders parameter to \"true\" by creating the missing folder. "),
                  HttpStatusCode.BadRequest,
                  data: data,
                  new List<string> { "Protocol: " + protocol, "Hostname: " + hostname, "Remote Path: " + (!string.IsNullOrEmpty(path) ? path : "./") });
        }

        public static class IssuePolicyAction
        {
            public static Error MissingQuote(JObject? data = null) =>
               AutomationError.GenerateError(
                   "issue.policy.action.missing.quote",
                    "The quote parameter is missing",
                    GenerateAutomationErrorMessage(
                        $"The quote parameter is missing. " +
                        $"Please include the quote parameter to the request to proceed. "),
                    HttpStatusCode.NotFound,
                    data: data,
                    new List<string> { });

            public static Error MissingInputData(JObject? data = null) =>
                AutomationError.GenerateError(
                    "issue.policy.action.missing.input.data",
                    "The input data parameter is missing",
                    GenerateAutomationErrorMessage(
                        $"The input data parameter is missing. " +
                        $"Please include in the body a JSON value as the input data. "),
                    HttpStatusCode.NotFound,
                    data: data,
                    new List<string> { });

            public static Error QuoteAndInputDataNotProvided(JObject? data = null) =>
                AutomationError.GenerateError(
                    "issue.policy.action.quote.and.form.data.not.provided",
                    "Required parameters are missing",
                    GenerateAutomationErrorMessage(
                        $"The required input data or quote parameter is missing. " +
                        $"Please include in the body a JSON value as the input data or a quote parameter. "),
                    HttpStatusCode.NotFound,
                    data: data,
                    new List<string> { });

            public static Error MissingResultFromCalculation(JObject? data = null) =>
                AutomationError.GenerateError(
                    "issue.policy.action.missing.calculation.result.data.from.calculation",
                    "Missing Calculation Result Data",
                    "When performing a calculation for a quote, the calculation result data was missing. "
                    + "Unfortunately this is an unexpected situation. We apologise for the inconvenience. "
                    + "We would appreciate it if you would contact customer support and provide a description of the steps you took to uncover this issue. "
                    + "If you require further assistance please contact technical support.",
                    HttpStatusCode.NotFound,
                    data: data,
                    new List<string>());

            public static Error MissingFormDataFromCalculation(JObject? data = null) =>
                AutomationError.GenerateError(
                    "issue.policy.action.missing.final.form.data.from.calculation",
                    "Missing Final Form Data",
                    "The final form data for the quote is missing. " +
                    "When performing a calculation for a quote, the final form data was missing. "
                    + "Unfortunately this is an unexpected situation. We apologise for the inconvenience. "
                    + "We would appreciate it if you would contact customer support and provide a description of the steps you took to uncover this issue. "
                    + "If you require further assistance please contact technical support.",
                    HttpStatusCode.NotFound,
                    data: data,
                    new List<string>());
        }

        public static class RenewPolicyAction
        {
            public static Error QuoteAndPolicyNotProvided(JObject? data = null) =>
                AutomationError.GenerateError(
                    "policy.renewal.quote.and.policy.not.provided",
                    "Neither a quote nor policy reference was provided",
                    $"When trying to renew a policy, the attempt failed because neither a quote reference nor a policy reference was provided. " +
                    $"To resolve this issue please ensure that either a quote reference or policy reference is provided when trying to renew a policy. " +
                    $"If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data,
                    new List<string> { });

            public static Error PolicyStatusInvalid(Guid policyId, string policyNumber, string policyStatus, JObject? data = null) =>
                AutomationError.GenerateError(
                    "policy.renewal.policy.status.invalid",
                    "The specified policy had an invalid status",
                    $"When trying to renew a policy, the specified policy (\"{policyNumber}\") could not be renewed " +
                    $"because it had an invalid status (\"{policyStatus}\"). " +
                    $"To resolve this issue please ensure that the specified policy has the status \"issued\", \"active\" or \"expired\". " +
                    $"If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data,
                    new List<string> { $"Policy ID: {policyId}" });

            public static Error QuoteTypeInvalid(Guid quoteId, string quoteReference, QuoteType quoteType, JObject? data = null) =>
                AutomationError.GenerateError(
                    "policy.renewal.quote.type.invalid",
                    "The specified quote was of an invalid type",
                    $"When trying to renew a policy in relation to the quote with reference \"{quoteReference}\", " +
                    $"a policy could not be renewed because the specified quote was of an invalid type (\"{quoteType}\"). " +
                    $"To resolve this issue, please ensure that the specified quote is of type \"Renewal\". " +
                    $"If you require further assistance, please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data,
                    new List<string> { $"Quote ID: {quoteId}" });

            public static Error QuoteStateInvalid(Guid quoteId, string quoteReference, string? quoteState, JObject? data = null) =>
                AutomationError.GenerateError(
                    "policy.renewal.quote.state.invalid",
                    "The specified quote had an invalid state",
                    $"When trying to renew a policy in relation to the quote with reference \"{quoteReference}\", " +
                    $"the specified quote could not be completed because it was in an invalid state (\"{quoteState}\"). " +
                    $"To resolve this issue please ensure that the specified quote is in either 'approved' or 'incomplete' state. " +
                    $"If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data,
                    new List<string> { $"Quote ID: {quoteId}" });

            public static Error QuoteCalculationStateInvalid(Guid quoteId, string quoteReference, string calculationState, JObject? data = null) =>
                AutomationError.GenerateError(
                    "policy.renewal.quote.calculation.state.invalid",
                    "The quote calculation result had an invalid state",
                    $"When trying to renew a policy in relation to the quote with reference \"{quoteReference}\", the quote could not be bound " +
                    $"because it was in state \"Incomplete\" and its most recent calculation result was in an invalid calculation state (\"{calculationState}\"). " +
                    $"To resolve this issue, either ensure that the specified quote is in \"Approved\" state, or that its most recent calculation result has " +
                    $"calculation state \"Binding Quote\". If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data,
                    new List<string> { $"Quote ID: {quoteId}" });

            public static Error CalculationTriggersActive(
                FormData? formData, CalculationResult? calculationResult)
            {
                var data = new JObject
                {
                    { "formData", formData != null ? JToken.FromObject(formData) : JValue.CreateNull() },
                    { "calculationResult", calculationResult != null ? JToken.FromObject(calculationResult) : JValue.CreateNull() },
                };

                return AutomationError.GenerateError(
                    "policy.renewal.calculation.triggers.active",
                    "The calculation result contained active triggers",
                    "When trying to renew a policy using a set of form data, the attempt failed " +
                    "because the quote calculation performed using the provided form data produced a calculation result with active triggers. " +
                    "To resolve this issue please ensure that a quote calculation performed using the provided form data will produce a calculation result without active triggers. " +
                    "If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data,
                    new List<string> { });
            }

            public static Error QuoteCalculationTriggersActive(Guid quoteId, string quoteReference, JObject? data = null) =>
                AutomationError.GenerateError(
                    "policy.renewal.quote.calculation.triggers.active",
                    "The quote calculation result contained active triggers",
                    $"When trying to renew a policy in relation to the quote with reference \"{quoteReference}\", the quote could not be bound " +
                    $"because it was in state \"Incomplete\" and its most recent calculation result has active triggers. " +
                    $"To resolve this issue, either ensure that the specified quote is in \"Approved\" state, or that its most recent calculation result has no active triggers. " +
                    $"If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data,
                    new List<string> { $"Quote ID: {quoteId}" });

            public static Error QuoteMismatchWithPolicy(
                Guid policyId, Guid? policyQuoteId, Guid quoteId, string policyNumber, string quoteReference, string quotePolicyNumber, JObject? data = null) =>
                AutomationError.GenerateError(
                    "policy.renewal.quote.mismatch.with.policy",
                    "The specified policy was different from the one associated with the specified quote",
                    $"When trying to renew policy \"{policyNumber}\" with reference to a quote, " +
                    $"the attempt failed because the specified quote (\"{quoteReference}\") was associated with a different policy (\"{quotePolicyNumber}\"). " +
                    $"To resolve this problem, either renew the policy without reference to a specific quote, " +
                    $"renew the policy without reference to a specific policy, ensure that the specified policy is associated with the specified quote, " +
                    $"or ensure that the specified policy is not associated with a quote. " +
                    $"If you require further assistance please contact technical support.",
                    HttpStatusCode.Conflict,
                    data,
                    new List<string> { $"Policy ID: {policyId}", $"Policy Quote ID: {policyQuoteId}", $"Specified Quote ID: {quoteId}" });

            public static Error TransactionTypeDisabled(Guid productId, string? productAlias, string? productName, JObject? data = null) =>
                AutomationError.GenerateError(
                    "policy.renewal.transaction.type.disabled",
                    "Renewal policy transactions are disabled for this product",
                    $"When trying to renew a policy for the \"{productName}\" product, " +
                    $"the attempt failed because the product settings prevented the creation of renewal policy transactions. " +
                    $"To resolve this issue please enable renewal policy transactions in the product settings for the \"{productName}\" product. " +
                    $"If you need further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data,
                    new List<string> { $"Product Alias: {productAlias}", $"Product ID: {productId}" });

            public static Error CalculationError(JObject? data = null) =>
                AutomationError.GenerateError(
                    "policy.renewal.calculation.error",
                    "The quote calculation resulted in an error",
                    "An attempt was made to renew a policy using a set of form data, " +
                    "however when a quote calculation was performed using the provided form data an error was encountered. " +
                    "A policy can only be renewed based on a set of form data if that form data can be used to perform a successful quote calculation. " +
                    "Please ensure that the provided form data is valid before attempting to renew a policy.",
                    HttpStatusCode.BadRequest,
                    data);

            public static Error CalculationStateInvalid(
                CalculationResult? calculationResult, JObject? data = null) =>
                AutomationError.GenerateError(
                    "policy.renewal.calculation.state.invalid",
                    "The calculation result had an invalid state",
                    $"When trying to renew a policy using a set of form data, the attempt failed because the quote calculation performed " +
                    $"using the provided form data produced a calculation result with an invalid calculation state (\"{calculationResult?.CalculationResultState}\"). " +
                    $"To resolve this issue please ensure that a quote calculation performed using the provided form data " +
                    $"will produce a calculation result with calculation state \"Binding Quote\". " +
                    $"If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data);

            public static Error PaymentDetailsNotProvided(string paymentMethodAlias, JObject? data = null) =>
                AutomationError.GenerateError(
                    "payment.details.not.provided",
                    "Payment details were not provided",
                    $"When trying to create a new policy transaction that requires payment, " +
                    $"the attempt failed because no payment details were provided for the selected payment method. " +
                    $"To resolve this issue please ensure that payment details are provided that match the selected payment method. " +
                    $"If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data,
                    new List<string> { $"Payment Method: {paymentMethodAlias}" });

            public static Error PaymentFailed(string errorReason, string fullErrorMessage, JObject? data = null) =>
                AutomationError.GenerateError(
                    "payment.failed",
                    "Payment failed",
                    $"When trying to process a payment for a new policy transaction, the payment attempt failed because {errorReason}. " +
                    $"To resolve this issue please ensure that the payment details used are valid and will result in a successful payment. " +
                    $"If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data,
                    new List<string> { $"Error Message: {fullErrorMessage}" });

            public static Error InvoiceNumberNotAvailable(string? productName, DeploymentEnvironment environment, JObject? data = null)
            {
                var environmentName = environment.ToString().ToLower();
                return AutomationError.GenerateError(
                    "policy.renewal.invoice.number.not.available",
                    "An invoice number could not be allocated",
                    $"When trying to renew the policy for the \"{productName}\" product, " +
                    $"an accompanying invoice could not be issued because the invoice number pool for the {environmentName} environment is empty. " +
                    $"To resolve this issue please ensure that the invoice number pool for the {environmentName} environment contains invoice numbers. " +
                    $"If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data);
            }

            public static Error QuoteAndInputDataNotProvided(JObject? data = null) =>
                AutomationError.GenerateError(
                    "policy.renewal.quote.and.input.data.not.provided",
                    "Neither a quote reference nor input data was provided",
                    $"When trying to renew a policy, the attempt failed because neither a quote reference nor a set of input data was provided. " +
                    $"To resolve this issue please ensure that either a quote reference or a set of input data is provided when trying to renew a policy. " +
                    $"If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data,
                    new List<string> { });

            public static Error EnvironmentDefaultProductReleaseNotSet(string? productName, DeploymentEnvironment environment, JObject? data = null)
            {
                var environmentName = environment.ToString().ToLower();
                return AutomationError.GenerateError(
                    "policy.renewal.environment.default.product.release.not.set",
                    "A default product release was not set for the environment",
                    $"When trying to renew a policy for the \"{productName}\" product, " +
                    $"a quote calculation could not be performed because there was no default product release set for the applicable product environment (\"{environmentName}\")." +
                    $" To resolve this issue set a default product release for the {environmentName} environment for the \"{productName}\" product." +
                    $" If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data,
                    new List<string> { });
            }

            // Others
            public static Error QuotePolicyAndInputDataNotProvided(JObject? data = null) =>
            AutomationError.GenerateError(
            "renew.policy.action.quote.policy.and.form.data.not.provided",
                    "Required parameters are missing",
                    $"The required input data, quote and policy parameter is missing. " +
                    $"Please include in the body a JSON value as the input data or a quote parameter. " +
                    $"If you require further assistance please contact technical support.",
                    HttpStatusCode.NotFound,
                    data: data,
                    new List<string> { });

            public static Error MissingLatestCalculationResult(JObject? data = null) =>
                AutomationError.GenerateError(
                    "renew.policy.action.missing.latest.calculation.result",
                    "Missing Latest Calculation Result",
                    "The latest calculation result for the quote is missing. " +
                    "Please ensure that the quote used for renewal has a valid latest calculation result. " +
                    $"If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data: data,
                    new List<string>());

            public static Error MissingResultFromCalculation(JObject? data = null) =>
                AutomationError.GenerateError(
                    "renew.policy.action.missing.calculation.result.data.from.calculation",
                    "Missing Calculation Result Data",
                    "When performing a calculation for a renewal quote, the calculation result data was missing. "
                    + "Unfortunately this is an unexpected situation. We apologise for the inconvenience. "
                    + "We would appreciate it if you would contact customer support and provide a description of the steps you took to uncover this issue. "
                    + "If you require further assistance please contact technical support.",
                    HttpStatusCode.NotFound,
                    data: data,
                    new List<string>());

            public static Error MissingFormDataFromCalculation(JObject? data = null) =>
                AutomationError.GenerateError(
                    "renew.policy.action.missing.final.form.data.from.calculation",
                    "Missing Final Form Data",
                    "The final form data for the quote is missing. " +
                    "When performing a calculation for a renewal quote, the final form data was missing. "
                    + "Unfortunately this is an unexpected situation. We apologise for the inconvenience. "
                    + "We would appreciate it if you would contact customer support and provide a description of the steps you took to uncover this issue. "
                    + "If you require further assistance please contact technical support.",
                    HttpStatusCode.NotFound,
                    data: data,
                    new List<string>());
        }

        public static class IpAddressInRangeCondition
        {
            public static Error IpAddressFormatError(string specifiedIpAddress) =>
                AutomationError.GenerateError(
                    "automation.ip.address.cannot.be.parsed",
                    "The specified IP Address cannot be parsed",
                    $"When trying to parse the IP address \"{specifiedIpAddress}\", it failed because it is invalid. " +
                    $"To resolve this problem, please ensure to specify a valid IP address. If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    null);

            public static Error IpAddressRangeFormatError(string specifiedIPAddressRange) =>
                AutomationError.GenerateError(
                    "automation.ip.address.range.cannot.be.parsed",
                    "The specified IP address range cannot be parsed",
                    $"When trying to parse the IP address range \"{specifiedIPAddressRange}\", it failed because the value cannot be parsed. " +
                    $"To resolve this problem, please ensure to specify a valid IP address range value. If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    null);

            public static Error SubnetMaskInvalid(string specifiedIpAddressSubnetMask) =>
                AutomationError.GenerateError(
                    "automation.ip.address.subnet.mask.cannot.be.parsed",
                    "The specified IP address subnet mask cannot be parsed",
                    $"When trying to parse the IP address subnet mask \"/{specifiedIpAddressSubnetMask}\", it failed because the value cannot be parsed. " +
                    $"To resolve this issue, make sure the subnet mask length must be a non-negative integer between 0 and 32. " +
                    $"If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    null);
        }

        public static class PortalPageTrigger
        {
            public static Error ProductIdRequired(string automationAlias, string triggerAlias) => new Error(
                "automation.portal.page.trigger.product.id.required",
                "The product ID was not provided",
                "Currently it is not possible to define an automation outside of a product, so portal page triggers "
                + "must pass the product ID. In this case no product ID was provided, so the automation to execute "
                + "could not be found.",
                HttpStatusCode.InternalServerError,
                null,
                new JObject
                {
                    { "automationAlias", automationAlias },
                    { "triggerAlias", triggerAlias },
                });

            public static Error NotFound(string automationAlias, string triggerAlias) => new Error(
                "automation.portal.page.trigger.not.found",
                "The portal page trigger wasn't found",
                $"When attempting to execute the portal page trigger \"{triggerAlias}\" in the automation \"{automationAlias}\""
                + "no such portal page trigger could be found. "
                + "Please try reloading the page or waiting 5 minutes then reloading the page, in case the portal page trigger "
                + "has been updated and you are attempting to execute a stale copy.",
                HttpStatusCode.NotFound,
                null,
                new JObject
                {
                    { "automationAlias", automationAlias },
                    { "triggerAlias", triggerAlias },
                });
        }
    }
}
