// <copyright file="TextElementsJObjectParser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Collections.Generic;
    using System.Globalization;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Text elements JSON object parser.
    /// </summary>
    public class TextElementsJObjectParser : IJsonObjectParser
    {
        private readonly TextInfo textInfo = new CultureInfo("en-US", false)
            .TextInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextElementsJObjectParser"/> class.
        /// </summary>
        /// <param name="prefix">Prefix string.</param>
        /// <param name="jObject">JSON object.</param>
        public TextElementsJObjectParser(string prefix, JObject jObject)
        {
            string organisationPrefix = this.textInfo.ToTitleCase(prefix);

            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (var content in jObject)
            {
                dict.Add(
                    string.Concat(organisationPrefix, this.textInfo.ToTitleCase(content.Key)),
                    content.Value["text"].ToString());
            }

            this.JsonObject = JObject.FromObject(dict);
        }

        /// <inheritdoc/>
        public JObject JsonObject { get; }
    }
}
