// <copyright file="IApplicationIntegrationRequestService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Threading.Tasks;
    using UBind.Application.Export;
    using UBind.Domain;

    /// <summary>
    /// Service for handling webhook/third-party integration requests.
    /// </summary>
    public interface IApplicationIntegrationRequestService
    {
        /// <summary>
        /// Executes a request to a third-party API and returns the templated response.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="webIntegrationId">The unique ID of the config integration to be used.</param>
        /// <param name="quoteId">The ID of the quote aggregate.</param>
        /// <param name="productId">The ID of the product belonging to the given tenant.</param>
        /// <param name="environment">The environment currently used.</param>
        /// <param name="payloadJson">The payload JSON.</param>
        /// <returns>The response from the web service request.</returns>
        Task<WebServiceIntegrationResponse> ExecuteRequest(
            Guid tenantId,
            string webIntegrationId,
            Guid quoteId,
            Guid productId,
            DeploymentEnvironment environment,
            string payloadJson);
    }
}
