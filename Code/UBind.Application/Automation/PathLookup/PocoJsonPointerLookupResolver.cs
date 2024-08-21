// <copyright file="PocoJsonPointerLookupResolver.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.PathLookup
{
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Helper for obtaining data from a dynamic object by path using json pointer.
    /// </summary>
    public class PocoJsonPointerLookupResolver
    {
        /// <summary>
        /// Fetch data from a dynamic object by path.
        /// </summary>
        /// <param name="pocObject">The plain old c# object.</param>
        /// <param name="path">The path.</param>
        /// <param name="providerName">The name of the provider that called for resolution.</param>
        /// <param name="debugContext">THe context/data to be used for debugging purposes. This includes, among others, configuration metadata.</param>
        /// <returns>The data located at the given path.</returns>
        public IProviderResult<IData> Resolve(object pocObject, string path, string providerName, JObject debugContext)
        {
            var jPointer = new PocoJsonPointer(path, providerName, debugContext);
            var obtainedValue = jPointer.Evaluate(pocObject);
            if (obtainedValue.IsFailure)
            {
                return ProviderResult<IData>.Failure(obtainedValue.Error);
            }

            return ProviderResult<IData>.Success(ObjectWrapper.Wrap(obtainedValue.Value));
        }
    }
}
