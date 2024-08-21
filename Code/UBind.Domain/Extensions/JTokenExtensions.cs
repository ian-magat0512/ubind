// <copyright file="JTokenExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Extension methods for JObject.
    /// </summary>
    public static class JTokenExtensions
    {
        /// <summary>
        /// Return a value indicatingn whether the json property contains any data or not.
        /// </summary>
        /// <param name="jToken">The token to test.</param>
        /// <returns>true if the token contains data other than empty string, otherwise false.</returns>
        public static bool IsNullOrEmpty(this JToken jToken)
        {
            return jToken == null
                || (jToken.Type == JTokenType.Array && !jToken.HasValues)
                || (jToken.Type == JTokenType.Object && !jToken.HasValues)
                || (jToken.Type == JTokenType.Null)
                || (jToken.Type == JTokenType.String && string.IsNullOrEmpty(jToken.ToString()));
        }
    }
}
