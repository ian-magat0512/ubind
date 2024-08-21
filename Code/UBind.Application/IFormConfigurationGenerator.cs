// <copyright file="IFormConfigurationGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Threading.Tasks;
    using UBind.Application.FlexCel;
    using UBind.Domain;

    /// <summary>
    /// Service for retrieving form product configuration (form schema etc.).
    /// </summary>
    public interface IFormConfigurationGenerator
    {
        /// <summary>
        /// Reads a product's configuration.
        /// </summary>
        /// <param name="webFormAppType">The release details type.</param>
        /// <returns>A task from which a string containging the configuration can be retrieved.</returns>
        /// <param name="workbook">The workbook to get the configuration from.</param>
        Task<string> Generate(
            Guid tenantId,
            Guid productId,
            WebFormAppType webFormAppType,
            FlexCelWorkbook workbook,
            string workflowJson,
            string? productJson,
            string? paymentFormJson);
    }
}
