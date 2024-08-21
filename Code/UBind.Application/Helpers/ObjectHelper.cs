// <copyright file="ObjectHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.JsonConverters;

    /// <summary>
    /// object helper methods.
    /// </summary>
    public static class ObjectHelper
    {
        /// <summary>
        /// Convert object to object dictionary.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A object dictionary.</returns>
        public static Dictionary<string, object> ToDictionary(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, new DictionaryConverter());

            return dictionary;
        }
    }
}
