// <copyright file="JsonHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using CSharpFunctionalExtensions;
    using Humanizer;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Json helper methods.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Convert json to URL parameter format.
        /// </summary>
        /// <param name="json">The json data to convert.</param>
        /// <param name="excludedFieldKeys">Optional array of field keys to be excluded.</param>
        /// <returns>A string containing the URL parameter formatted data.</returns>
        public static string PrintAsURLParams(this JObject json, string[] excludedFieldKeys = null)
        {
            return JsonHelper.PrintAsURLParams(json.ToString(), excludedFieldKeys);
        }

        /// <summary>
        /// Convert json to URL parameter format.
        /// </summary>
        /// <param name="json">The json data to convert.</param>
        /// <param name="excludedFieldKeys">Optional array of field keys to be excluded.</param>
        /// <returns>A string containing the URL parameter formatted data.</returns>
        public static string PrintAsURLParams(string json, string[] excludedFieldKeys = null)
        {
            excludedFieldKeys = excludedFieldKeys ?? Array.Empty<string>();

            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(json);

            var sb = new StringBuilder();

            foreach (KeyValuePair<string, object> elem in obj)
            {
                if (!excludedFieldKeys.Contains(elem.Key))
                {
                    PrintElementAsURLParams(sb, elem, string.Empty, excludedFieldKeys);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Flatten a nested json string and return as dictionary.
        /// </summary>
        /// <param name="jsonString">The json string.</param>
        /// <returns>A flatten json string.</returns>
        public static IDictionary<string, string> FlattenNestedJsonStringAsDictionary(string jsonString)
        {
            JObject jsonObject = JObject.Parse(jsonString);
            IEnumerable<JToken> jTokens = jsonObject.Descendants().Where(p => !p.Any());
            IDictionary<string, string> results = jTokens.Aggregate(new Dictionary<string, string>(), (properties, jToken) =>
            {
                properties.Add(jToken.Path.Replace("[", string.Empty).Replace("]", string.Empty), jToken.ToString());
                return properties;
            });
            return results;
        }

        /// <summary>
        /// Capitalize the property name of json object.
        /// </summary>
        /// <param name="token">The json token to convert.</param>
        /// <returns>The converted JToken.</returns>
        public static JToken CapitalizePropertyNames(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                JObject copy = new JObject();
                foreach (var prop in token.Children())
                {
                    copy.Add(CapitalizePropertyNames(prop));
                }

                return copy;
            }
            else if (token.Type == JTokenType.Property)
            {
                var tokenName = ((JProperty)token).Name;
                var name = !string.IsNullOrEmpty(tokenName)
                    ? string.Concat(tokenName.Substring(0, 1).ToUpperInvariant(), tokenName.AsSpan(1))
                    : string.Empty;
                var value = CapitalizePropertyNames(((JProperty)token).Value);
                return new JProperty(name, value);
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray copy = new JArray();
                foreach (JToken item in token.Children())
                {
                    JToken child = item;
                    if (child.HasValues)
                    {
                        child = CapitalizePropertyNames(child);
                    }

                    copy.Add(child);
                }

                return copy;
            }

            return token;
        }

        /// <summary>
        /// Check if the string is a valid json.
        /// </summary>
        /// <param name="input">The string input.</param>
        /// <returns>A value indicates wether the input is a valid json.</returns>
        public static bool IsValidJson(this string input)
        {
            try
            {
                var obj = JToken.Parse(input);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Flatten a Json into a string separated by space. Great for having searchable fields in search indexes.
        /// </summary>
        /// <param name="json">The object to flatten.</param>
        /// <returns>The flattened string.</returns>
        public static string FlattenJsonToString(string json)
        {
            var jsonFormData = JToken.Parse(json);
            JObject jsonObject = JObject.FromObject(jsonFormData);
            var flattenedString = jsonObject.Flatten();
            return string.Join(" ", flattenedString.Select(x => x.Value));
        }

        /// <summary>
        /// When parsing a json string, a backslash-quote (\') sequence will be convered to just a single quote.
        /// In order to preserve the backslash, the backslash needs to be escaped.
        /// </summary>
        /// <param name="jsonString">The string of json that may contain a backslash-quote (\').</param>
        /// <returns>a string with the backslash escaped when it's part of a backslash-quote sequence.</returns>
        public static string ReplaceBackslashQuoteWithDoubleBackslashQuote(string jsonString)
        {
            // We need run the regex replace TWICE because if two sequences are next to each other only the first will be detected.
            string fixedJsonString = Regex.Replace(jsonString, @"([^\\])\\'", "$1\\\\'");
            fixedJsonString = Regex.Replace(fixedJsonString, @"([^\\])\\'", "$1\\\\'");

            return fixedJsonString;
        }

        /// <summary>
        /// Checks the JSON string is valid JSON, including detecting missing/undefined values for properties, and throws an exception if it is not valid.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="jsonDocumentName">The name of the json document, so that we can identify it in the errors generated.</param>
        public static void CheckJsonIsValid(this string json, string jsonDocumentName)
        {
            try
            {
                var jObject = JObject.Parse(json);
                foreach (var jToken in jObject.Descendants())
                {
                    // if the element has a missing value, it will be Undefined - this is invalid
                    if (jToken.Type == JTokenType.Undefined)
                    {
                        throw new ErrorException(
                            Errors.JsonDocument.JsonInvalidMissingPropertyValue(jsonDocumentName, jToken.Path, json));
                    }
                }
            }
            catch (JsonReaderException ex)
            {
                var error = Errors.JsonDocument.JsonInvalid(
                        jsonDocumentName,
                        ex.Message,
                        ex.LineNumber,
                        ex.LinePosition,
                        ex.Path,
                        json);
                throw new ErrorException(error, ex);
            }
        }

        public static string StandardizeJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return json;
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = false, // Eliminates unnecessary whitespace.
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Uses camelCase which can reduce size if originally in PascalCase.
            };

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                return System.Text.Json.JsonSerializer.Serialize(doc.RootElement, options);
            }
        }

        private static void PrintElementAsURLParams(StringBuilder sb, KeyValuePair<string, object> element, string prefix, string[] excludedFieldKeys)
        {
            if (element.Key == null || element.Value == null)
            {
                return;
            }

            if (element.Value.GetType() == typeof(string) ||
                element.Value.GetType() == typeof(long) ||
                element.Value.GetType() == typeof(double) ||
                element.Value.GetType() == typeof(bool))
            {
                var value = element.Value.ToString();

                if (!string.IsNullOrWhiteSpace(value))
                {
                    sb.Append(prefix + element.Key + "=" + Uri.EscapeDataString(value) + "&");

                    CultureInfo culture = CultureInfo.CreateSpecificCulture("en-AU");
                    DateTimeStyles styles = DateTimeStyles.None;

                    if (DateTime.TryParse(value, culture, styles, out DateTime dateValue))
                    {
                        sb.Append(prefix + element.Key + "_day=" + dateValue.Day + "&");
                        sb.Append(prefix + element.Key + "_month=" + dateValue.Month + "&");
                        sb.Append(prefix + element.Key + "_year=" + dateValue.Year + "&");
                    }

                    var valueDehumanized = value.Dehumanize();
                    var valueInCamelCase = (string.Empty + valueDehumanized[0]).Transform(To.LowerCase);
                    if (valueDehumanized.Length > 1)
                    {
                        valueInCamelCase += valueDehumanized.Substring(1, valueDehumanized.Length - 1);
                    }

                    sb.Append(prefix + element.Key + "_" + valueInCamelCase + "=On&");
                }
            }
            else
            {
                // NOP
                var expando = element.Value as ExpandoObject;
                var children = element.Value as IEnumerable<object>;
                var index = 0;
                var newPrefix = (prefix != string.Empty) ? prefix + element.Key : element.Key;

                foreach (var child in children.OfType<ExpandoObject>())
                {
                    var newPrefixWithIndex = newPrefix + "_" + index + "_";

                    foreach (KeyValuePair<string, object> elem in child)
                    {
                        if (!excludedFieldKeys.Contains(elem.Key))
                        {
                            PrintElementAsURLParams(sb, elem, newPrefixWithIndex, excludedFieldKeys);
                        }
                    }

                    index++;
                }
            }
        }
    }
}
