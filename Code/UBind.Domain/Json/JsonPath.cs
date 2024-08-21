// <copyright file="JsonPath.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Json
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using UBind.Domain.JsonConverters;

    /// <summary>
    /// Type for representing Json paths.
    /// </summary>
    [JsonConverter(typeof(JsonPathConverter))]
    public class JsonPath
    {
        private const string SimpleDotNotationExpression =
            "^[a-zA-Z0-9_]+(\\.[a-zA-Z0-9_]+)*$";

        private const string InvalidPathExpression =
            @"^(\[)|^(\])|^(\.$)|^(\.\.)|^(\[\])|^(\[\?\(.*?\)\])|^(\['.*?'\])|^(\[.*?\])|^([a-zA-Z0-9_]+(\ [a-zA-Z0-9_]+)*$)";

        private readonly string jsonPath;
        private readonly string parentPath;
        private readonly string finalChild;
        private readonly IEnumerable<string> ancestors;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPath"/> class.
        /// </summary>
        /// <param name="jsonPath">A string containing the Json path.</param>
        /// <exception cref="ArgumentException">Thrown when parameter does not contain a valid Json path.</exception>
        public JsonPath(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath))
            {
                return;
            }

            this.jsonPath = jsonPath;
            if (Regex.IsMatch(jsonPath, SimpleDotNotationExpression))
            {
                var segments = jsonPath.Split('.');
                var lastIndex = segments.Length - 1;
                var ancestorsList = new string[lastIndex];
                Array.Copy(segments, 0, ancestorsList, 0, lastIndex);
                this.ancestors = ancestorsList;
                this.parentPath = lastIndex > 0 ? string.Join(".", this.ancestors) : string.Empty;
                this.finalChild = segments[lastIndex];
            }
            else if (Regex.IsMatch(jsonPath, InvalidPathExpression))
            {
                throw new JsonPathFormatException(jsonPath);
            }
        }

        /// <summary>
        /// Gets a string containing the JSON path.
        /// </summary>
        public string Value => this.jsonPath;

        /// <summary>
        /// Gets a value indicating whether it is possible to derive the JSON path for the parent item of the property specified in the JSON path.
        /// </summary>
        public bool CanIdentifyAncestors => this.ancestors != null;

        /// <summary>
        /// Gets the path to the parent of the item specified by this JSON path.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when it is not possible to derive the parent path.</exception>
        public string ParentPath
        {
            get
            {
                if (this.parentPath == null)
                {
                    throw new InvalidOperationException("Illegal attempt to access non-existant ParentPath of JsonPath. Always check first by calling CanIdentifyAncestors.");
                }

                return this.parentPath;
            }
        }

        /// <summary>
        /// Gets the name of the final child in the path, when it can be determined.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when it is not possible to derive the name of the final child in the path.</exception>
        public string FinalChild
        {
            get
            {
                if (this.finalChild == null)
                {
                    throw new InvalidOperationException("Illegal attempt to access final child part of JsonPath. Always check first by calling CanIdentifyAncestors.");
                }

                return this.finalChild;
            }
        }

        /// <summary>
        /// Gets a list of the names of the ancestor objects in the path, if they can be determined.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the ancestors cannot be determined because the path is not a simple dot-separated list of property names.</exception>
        public IEnumerable<string> Ancestors
        {
            get
            {
                if (this.ancestors == null)
                {
                    throw new InvalidOperationException("Illegal attempt to access ancestors of JsonPath. Always check first by calling CanIdentifyAncestors.");
                }

                return this.ancestors;
            }
        }
    }
}
