// <copyright file="AutomationError.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Humanizer;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Represents an error raised by the automations feature.
    /// </summary>
    public class AutomationError : Error
    {
        public AutomationError(
            string code,
            string title,
            string message,
            HttpStatusCode httpStatusCode,
            JObject? data,
            IEnumerable<string>? additionalDetails = null)
            : base(code, title, message, httpStatusCode, additionalDetails, data)
        {
        }

        /// <summary>
        /// Creates an instance of AutomationError but copies data to additionalDetails.
        /// THIS SHOULD NOT BE USED.
        /// This is because additionalDetails is supposed to be human readable
        /// while data can contain machine readable information like GUIDs.
        /// Instead, only add all relevant human readable information to additionalDetails
        /// and use <see cref="AutomationError.GenerateError(string, string, string, int, JObject, IEnumerable{string})"/>.
        /// </summary>
        /// <returns>Automation Error.</returns>
        public static AutomationError GenerateErrorWithAdditionalDetailsFromData(
           string code,
           string title,
           string message,
           HttpStatusCode httpStatusCode,
           JObject? data,
           IEnumerable<string>? additionalDetails = null)
        {
            var error = new AutomationError(code, title, message, httpStatusCode, data, additionalDetails);
            error.AdditionalDetails = error.ConvertToAdditionalDetailsList(data, additionalDetails);
            return error;
        }

        public static AutomationError GenerateError(
           string code,
           string title,
           string message,
           HttpStatusCode httpStatusCode,
           JObject? data,
           IEnumerable<string>? additionalDetails = null)
        {
            var error = new AutomationError(code, title, message, httpStatusCode, data, additionalDetails);
            return error;
        }

        private List<string>? ConvertToAdditionalDetailsList(
            JObject? errorData,
            IEnumerable<string>? additionalDetails = null)
        {
            if (errorData == null)
            {
                return null;
            }

            var detailsList = new List<string>();
            var forTitleCaseChange = new List<string>() { "environment", "entityEnvironment", "feature" };
            foreach (var property in errorData)
            {
                if (property.Value == null)
                {
                    continue;
                }

                JToken errorDataPropertyValue = property.Value.Type == JTokenType.String && forTitleCaseChange.Any(property.Key.EqualsIgnoreCase) ?
                    property.Value.ToString().Titleize() :
                    property.Value;
                detailsList.Add($"{property.Key.Titleize().Replace(" id", " ID", StringComparison.CurrentCultureIgnoreCase)}: {errorDataPropertyValue}");
            }

            if (additionalDetails != null)
            {
                detailsList.AddRange(additionalDetails);
            }

            return detailsList;
        }
    }
}
