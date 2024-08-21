// <copyright file="SystemJsonHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Helper class for using System.Text.Json.
    /// </summary>
    public static class SystemJsonHelper
    {
        private static JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public static JsonSerializerOptions GetSerializerOptions()
        {
            return serializerOptions;
        }
    }
}
