// <copyright file="JsonPrettifier.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Text;
    using Humanizer;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Converts json to human readable formats for data display to non technical users.
    /// </summary>
    public static class JsonPrettifier
    {
        /// <summary>
        /// Pretty print data from json.
        /// </summary>
        /// <param name="json">The json data to pretty print.</param>
        /// <param name="excludedFieldKeys">Optional array of field keys to be excluded.</param>
        /// <returns>A string containing the pretty-printed data.</returns>
        public static string PrettyPrint(this JObject json, string[] excludedFieldKeys = null)
        {
            return JsonPrettifier.PrettyPrint(json.ToString(), excludedFieldKeys);
        }

        /// <summary>
        /// Pretty print data from json in HTML format.
        /// </summary>
        /// <param name="json">The json data to pretty print.</param>
        /// <param name="excludedFieldKeys">Optional array of field keys to be excluded.</param>
        /// <returns>A string containing the pretty-printed data.</returns>
        public static string PrettyPrintHtml(this JObject json, string[] excludedFieldKeys = null)
        {
            return JsonPrettifier.PrettyPrintHtml(json.ToString(), excludedFieldKeys);
        }

        /// <summary>
        /// Pretty print data from json in HTML Table format.
        /// </summary>
        /// <param name="json">The json data to pretty print.</param>
        /// <param name="includeRepeatingQuestionSets">Optional to include the repeating set in the result.</param>
        /// <returns>A string containing the pretty-printed data.</returns>
        public static string PrettyPrintHtmlTables(this JObject json, bool includeRepeatingQuestionSets = true)
        {
            return JsonPrettifier.PrettyPrintHtmlTables(json.ToString(), includeRepeatingQuestionSets);
        }

        /// <summary>
        /// Pretty print data from json.
        /// </summary>
        /// <param name="json">The json data to pretty print.</param>
        /// <param name="excludedFieldKeys">Optional array of field keys to be excluded.</param>
        /// <returns>A string containing the pretty-printed data.</returns>
        public static string PrettyPrint(string json, string[] excludedFieldKeys = null)
        {
            excludedFieldKeys = excludedFieldKeys ?? Array.Empty<string>();

            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(json);

            var sb = new StringBuilder();

            foreach (KeyValuePair<string, object> elem in obj)
            {
                if (!excludedFieldKeys.Contains(elem.Key))
                {
                    PrintElement(sb, elem, 1, excludedFieldKeys);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Pretty print data from json in HTML format.
        /// </summary>
        /// <param name="json">The json data to pretty print.</param>
        /// <param name="excludedFieldKeys">Optional array of field keys to be excluded.</param>
        /// <returns>A string containing the pretty-printed data.</returns>
        public static string PrettyPrintHtml(string json, string[] excludedFieldKeys = null)
        {
            excludedFieldKeys = excludedFieldKeys ?? Array.Empty<string>();

            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(json);

            var sb = new StringBuilder();

            /* TODO: count number of fields that are not excluded and ensure that at least one will be printed, if not, don't include this part at all */

            sb.AppendLine("<ul>");

            foreach (KeyValuePair<string, object> elem in obj)
            {
                if (!excludedFieldKeys.Contains(elem.Key))
                {
                    PrintElementHtml(sb, elem, 1, excludedFieldKeys);
                }
            }

            sb.Append("</ul>");

            return sb.ToString();
        }

        /// <summary>
        /// Pretty print data from json in HTML Table format.
        /// </summary>
        /// <param name="json">The json data to pretty print.</param>
        /// <param name="includeRepeatingQuestionSets">Optional to include Repeating Question Sets in the result.</param>
        /// <returns>A string containing the pretty-printed data.</returns>
        public static string PrettyPrintHtmlTables(string json, bool includeRepeatingQuestionSets = true)
        {
            dynamic jsonObject = JsonConvert.DeserializeObject<ExpandoObject>(json);

            var container = new StringBuilder();
            var nonRepeatingQuestionSets = new StringBuilder();
            bool nonRepeatingQuestionSetsHasQuestion = false;
            var repeatingQuestionSets = new StringBuilder();
            bool repeatingQuestionSetsHasQuestion = false;

            var header = @"
            <tr>
                <th class=""summary-name"">Question</th>
                <th class=""summary-value"">Answer</th>
            </tr>";

            nonRepeatingQuestionSets.AppendLine("<table class=\"summary-table\">");
            nonRepeatingQuestionSets.Append(header);
            foreach (KeyValuePair<string, object> elem in jsonObject)
            {
                string nonRepeatingQuestionSetsLine = string.Empty;
                string repeatingQuestionSetsLine = string.Empty;
                dynamic elementValue;
                try
                {
                    // try to parse if a json string
                    var expConverter = new ExpandoObjectConverter();
                    elementValue = JsonConvert.DeserializeObject<List<ExpandoObject>>(elem.Value + string.Empty, expConverter);
                }
                catch
                {
                    elementValue = null;
                }

                if (elem.Value is ExpandoObject)
                {
                    var questionSets = elem.Value as ExpandoObject;
                    foreach (KeyValuePair<string, object> questionSet in questionSets)
                    {
                        if (!string.IsNullOrEmpty(questionSet.Value.ToString()))
                        {
                            nonRepeatingQuestionSetsHasQuestion = true;
                            nonRepeatingQuestionSetsLine = @"
                            <tr>
                                <td class=""summary-name"">" + questionSet.Key.Humanize(LetterCasing.Title) + @"</td>
                                <td class=""summary-value"">" + questionSet.Value + @"</td>
                            </tr>";
                            nonRepeatingQuestionSets.Append(nonRepeatingQuestionSetsLine);
                        }
                    }
                }

                if (elementValue == null && (elem.Value is string || elem.Value is bool) && !string.IsNullOrEmpty(elem.Value.ToString()))
                {
                    nonRepeatingQuestionSetsHasQuestion = true;
                    nonRepeatingQuestionSetsLine = @"
                    <tr>
                        <td class=""summary-name"">" + elem.Key.Humanize(LetterCasing.Title) + @"</td>
                        <td class=""summary-value"">" + elem.Value + @"</td>
                    </tr>";
                    nonRepeatingQuestionSets.Append(nonRepeatingQuestionSetsLine);
                }

                if ((elementValue is List<ExpandoObject> || elem.Value is IEnumerable<object> || elem.Value is Array) && includeRepeatingQuestionSets)
                {
                    var repeatingQuestions = elem.Value is string ? elementValue : (elem.Value as IEnumerable<object>).OfType<ExpandoObject>();
                    repeatingQuestionSets.AppendLine("<h2>" + elem.Key.Humanize(LetterCasing.Title) + "</h2>");

                    int repeatingQuestionCount = 0;

                    foreach (var repeatingQuestion in repeatingQuestions)
                    {
                        var repeatingQuestionSet = new StringBuilder();
                        bool repeatingQuestionHasQuestionAndAnswer = false;
                        repeatingQuestionCount++;

                        repeatingQuestionSet.AppendLine("<h3>" + elem.Key.Singularize(false).Humanize(LetterCasing.Title) + " " + repeatingQuestionCount + "</h3>");
                        repeatingQuestionSet.AppendLine("<table class=\"summary-table\">");
                        repeatingQuestionSet.Append(header);
                        foreach (KeyValuePair<string, object> repeatingQuestionElem in repeatingQuestion)
                        {
                            if (!string.IsNullOrEmpty(repeatingQuestionElem.Value as string))
                            {
                                repeatingQuestionHasQuestionAndAnswer = true;

                                repeatingQuestionSetsLine = @"
                                <tr>
                                    <td class=""summary-name"">" + repeatingQuestionElem.Key.Humanize(LetterCasing.Title) + @"</td>
                                    <td class=""summary-value"">" + repeatingQuestionElem.Value + @"</td>
                                </tr>";
                                repeatingQuestionSet.Append(repeatingQuestionSetsLine);
                            }
                        }

                        repeatingQuestionSet.Append("</table>");

                        if (repeatingQuestionHasQuestionAndAnswer)
                        {
                            repeatingQuestionSetsHasQuestion = true;
                            repeatingQuestionSets.Append(repeatingQuestionSet);
                        }
                    }
                }
            }

            nonRepeatingQuestionSets.Append("</table>");

            if (nonRepeatingQuestionSetsHasQuestion)
            {
                container.Append(nonRepeatingQuestionSets);
            }

            if (includeRepeatingQuestionSets && repeatingQuestionSetsHasQuestion)
            {
                container.Append(repeatingQuestionSets);
            }

            return container.ToString();
        }

        private static void PrintElement(StringBuilder sb, KeyValuePair<string, object> element, int indentation, string[] excludedFieldKeys)
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
                    sb.Append(new string(' ', indentation));
                    sb.Append(element.Key.Humanize(LetterCasing.Sentence) + ": ");
                    sb.AppendLine(value);
                }
            }
            else
            {
                /* TODO: count number of fields that are not excluded and ensure that at least one will be printed, if not, don't include this part at all */

                // NOP
                sb.Append(new string(' ', indentation));
                sb.AppendLine(element.Key.Humanize(LetterCasing.Sentence) + ": ");
                indentation++;

                var blah = element.Value.GetType();

                var expando = element.Value as ExpandoObject;
                var children = element.Value as IEnumerable<object>;
                var index = 1;
                foreach (var child in children.OfType<ExpandoObject>())
                {
                    sb.Append(new string(' ', indentation));
                    sb.AppendLine(element.Key.Singularize(false).Humanize(LetterCasing.Sentence) + " " + index + ": ");
                    foreach (KeyValuePair<string, object> elem in child)
                    {
                        if (!excludedFieldKeys.Contains(elem.Key))
                        {
                            PrintElement(sb, elem, indentation + 1, excludedFieldKeys);
                        }
                    }

                    index++;
                }
            }
        }

        private static void PrintElementHtml(StringBuilder sb, KeyValuePair<string, object> element, int indentation, string[] excludedFieldKeys)
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
                    sb.Append(new string(' ', indentation));
                    sb.Append("<li>");
                    sb.Append(element.Key.Humanize(LetterCasing.Sentence) + ": ");
                    sb.Append(value);
                    sb.AppendLine("</li>");
                }
            }
            else
            {
                // NOP
                sb.Append(new string(' ', indentation));
                sb.Append("<li>");
                sb.AppendLine(element.Key.Humanize(LetterCasing.Sentence) + ": ");

                sb.Append(new string(' ', indentation + 1));
                sb.AppendLine("<ul>");

                var expando = element.Value as ExpandoObject;
                var children = element.Value as IEnumerable<object>;
                var index = 1;
                foreach (var child in children.OfType<ExpandoObject>())
                {
                    /* TODO: count number of fields that are not excluded and ensure that at least one will be printed, if not, don't include this part at all */

                    sb.Append(new string(' ', indentation + 2));
                    sb.Append("<li>");
                    sb.AppendLine(element.Key.Singularize(false).Humanize(LetterCasing.Sentence) + " " + index + ": ");

                    sb.Append(new string(' ', indentation + 3));
                    sb.AppendLine("<ul>");

                    foreach (KeyValuePair<string, object> elem in child)
                    {
                        if (!excludedFieldKeys.Contains(elem.Key))
                        {
                            PrintElementHtml(sb, elem, indentation + 4, excludedFieldKeys);
                        }
                    }

                    sb.Append(new string(' ', indentation + 3));
                    sb.AppendLine("</ul>");
                    sb.Append(new string(' ', indentation + 2));
                    sb.AppendLine("</li>");

                    index++;
                }

                sb.Append(new string(' ', indentation + 1));
                sb.AppendLine("</ul>");

                sb.Append(new string(' ', indentation));
                sb.AppendLine("</li>");
            }
        }
    }
}
