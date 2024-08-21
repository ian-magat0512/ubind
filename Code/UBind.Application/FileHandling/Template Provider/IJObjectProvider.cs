// <copyright file="IJObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;

    /// <summary>
    /// A template JSON object provider.
    /// </summary>
    public interface IJObjectProvider
    {
        /// <summary>
        /// Gets the stored and processed JSON Object.
        /// </summary>
        JObject JsonObject { get; }

        /// <summary>
        /// Creates the Json Object from application event.
        /// </summary>
        /// <param name="applicationEvent">The application event.</param>
        Task CreateJsonObject(ApplicationEvent applicationEvent);
    }
}
