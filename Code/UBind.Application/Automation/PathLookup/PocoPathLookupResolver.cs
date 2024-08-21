// <copyright file="PocoPathLookupResolver.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.PathLookup
{
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Helper for obtaining data from a dynamic object by path.
    /// </summary>
    public static class PocoPathLookupResolver
    {
        /// <summary>
        /// Fetch data from a dynamic object by path.
        /// </summary>
        /// <param name="pocObject">The plain old C# object.</param>
        /// <param name="path">The path.</param>
        /// <param name="providerName">The name of the provider the path lookup is for (for use in error messages).</param>
        /// <param name="debugContext">THe context/data to be used for debugging purposes. This includes, among others, configuration metadata.</param>
        /// <returns>The data located at the given path.</returns>
        public static IProviderResult<IData> Resolve(object pocObject, string path, string providerName, JObject debugContext = null)
        {
            if (!PathHelper.IsJsonPointer(path))
            {
                path = PathHelper.ToJsonPointer(path);
            }

            return new PocoJsonPointerLookupResolver().Resolve(pocObject, path, providerName, debugContext);
        }
    }
}
