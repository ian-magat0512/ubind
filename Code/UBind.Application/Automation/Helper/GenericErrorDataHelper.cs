// <copyright file="GenericErrorDataHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Helper
{
    using System;
    using System.Collections.Generic;
    using Humanizer;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Domain;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This class is for creating a dictionary of generic automations-related error data for machine-processing and investigation.
    /// </summary>
    public class GenericErrorDataHelper
    {
        /// <summary>
        /// Returns a dictionary of generic automation-related data for error reporting.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant used for the automation.</param>
        /// <param name="productId">The ID of the product used for the automation.</param>
        /// <param name="environment">The environment used for the automation.</param>
        /// <returns>A dictionary of general error details.</returns>
        /// <remarks>To be used in instances wherein the automation data cannot be provided.</remarks>
        public static JObject GetGeneralErrorDetails(Guid tenantId, Guid? productId, DeploymentEnvironment? environment)
        {
            var genericErrorDetails = new Dictionary<string, string?>()
            {
                    { ErrorDataKey.Tenant, tenantId.ToString() },
                    { ErrorDataKey.Product, productId?.ToString() },
                    { ErrorDataKey.Environment, environment?.Humanize() },
                    { ErrorDataKey.Feature, "Automations" },
            };
            return JObject.FromObject(genericErrorDetails);
        }

        public static JObject RetrieveErrorData(Domain.SerialisedEntitySchemaObject.IEntity entity)
        {
            var errorData = new JObject
            {
                { ErrorDataKey.EntityType, $"{entity.GetType().Name.Humanize().ToCamelCase()}" },
                { ErrorDataKey.EntityId, $"{entity.Id}" },
            };

            if (!string.IsNullOrEmpty(entity.EntityReference))
            {
                errorData.Add(ErrorDataKey.EntityReference, $"{entity.EntityReference}");
            }

            if (!string.IsNullOrEmpty(entity.EntityEnvironment))
            {
                errorData.Add(ErrorDataKey.EntityEnvironment, $"{entity.EntityEnvironment.Humanize().ToCamelCase()}");
            }

            if (!string.IsNullOrEmpty(entity.EntityDescriptor))
            {
                errorData.Add(ErrorDataKey.EntityDescriptor, $"{entity.EntityDescriptor}");
            }

            return errorData;
        }

        public static List<string> GenerateAdditionalDetailList(
            JObject errorData)
        {
            if (errorData == null)
            {
                return null;
            }

            var detailsList = new List<string>();
            var forTitleCaseChange = new List<string>() { "environment", "entityEnvironment", "feature" };
            foreach (var property in errorData)
            {
                JToken errorDataPropertyValue = property.Value.Type == JTokenType.String && forTitleCaseChange.Any(property.Key.EqualsIgnoreCase) ?
                    property.Value.ToString().Titleize() :
                    property.Value;
                detailsList.Add($"{property.Key.Titleize()}: {errorDataPropertyValue}");
            }

            return detailsList;
        }
    }
}
