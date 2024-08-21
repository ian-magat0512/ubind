// <copyright file="GenericJObjectParser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Organisation JSON object parser.
    /// </summary>
    public class GenericJObjectParser : IJsonObjectParser
    {
        private readonly TextInfo textInfo
            = new CultureInfo("en-US", false).TextInfo;

        private readonly int startIndex;

        private Dictionary<string, string> dict
            = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericJObjectParser"/> class.
        /// </summary>
        /// <param name="prefix">Prefix string.</param>
        /// <param name="jToken">JSON token.</param>
        public GenericJObjectParser(string prefix, JToken jToken, bool flattenDataObject = true, int startIndex = 1)
        {
            this.startIndex = startIndex;
            if (flattenDataObject)
            {
                string finalPrefix = this.textInfo.ToTitleCase(prefix);
                this.WriteDataToJson(finalPrefix, jToken);
                this.JsonObject = JObject.FromObject(this.dict);
            }
            else
            {
                var obj = JObject.FromObject(jToken);
                this.ConvertPropertiesToPascalCase(obj);
                this.JsonObject = obj;
            }
        }

        /// <inheritdoc/>
        public JObject JsonObject { get; }

        private void GetAllProperties(string rawPrefix, JToken children)
        {
            foreach (JToken child in children.Children())
            {
                JProperty property = child as JProperty;
                if (property != null)
                {
                    string prefix
                        = rawPrefix + this.FirstCharToUpper(property.Name);
                    this.WriteDataToJson(prefix, property.Value);
                }
            }
        }

        private void GetAllEnumeration(string rawPrefix, JToken arrayToken)
        {
            int counter = this.startIndex;
            foreach (JToken token in arrayToken)
            {
                string key = string.Concat(rawPrefix, counter.ToString());
                this.WriteDataToJson(key, token);
                counter++;
            }
        }

        private void WriteDataToJson(string key, JToken token)
        {
            if (this.IsValidJTokenOutput(token))
            {
                this.dict[key] = token.ToString();
            }
            else if (token.Type == JTokenType.Object)
            {
                this.GetAllProperties(key, token);
            }
            else if (token.Type == JTokenType.Array)
            {
                this.GetAllEnumeration(key, token);
            }
        }

        private bool IsValidJTokenOutput(JToken jToken)
        {
            return jToken.Type == JTokenType.Integer
                || jToken.Type == JTokenType.Float
                || jToken.Type == JTokenType.String
                || jToken.Type == JTokenType.Boolean
                || jToken.Type == JTokenType.Date
                || jToken.Type == JTokenType.Guid
                || jToken.Type == JTokenType.Uri
                || jToken.Type == JTokenType.TimeSpan;
        }

        private string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException();
            }

            return string.Concat(input.First().ToString().ToUpper(), input.AsSpan(1));
        }

        private void ConvertPropertiesToPascalCase(JToken token)
        {
            if (token == null)
            {
                return;
            }

            if (token.Type == JTokenType.Object)
            {
                var jObject = (JObject)token;
                foreach (var property in jObject.Properties().ToList())
                {
                    string newPropertyName = this.FirstCharToUpper(property.Name);
                    this.ConvertPropertiesToPascalCase(property.Value);
                    property.Replace(new JProperty(newPropertyName, property.Value));
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (var item in token)
                {
                    this.ConvertPropertiesToPascalCase(item);
                }
            }
        }
    }
}
