// <copyright file="IProviderContext.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Data;

    public interface IProviderContext
    {
        AutomationData AutomationData { get; }

        CancellationToken CancellationToken { get; }

        string CurrentActionDataPath { get; }

        Task<JObject> GetDebugContext();

        Task<JObject> GetDebugContextForProviders(string providerReferenceKey);
    }
}
