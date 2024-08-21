// <copyright file="IMsWordEngineService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using System;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;

    /// <summary>
    /// Defines the required contract for implementing a template service.
    /// </summary>
    public interface IMsWordEngineService
    {
        /// <summary>
        /// Populates the fields of the provider to the templated source.
        /// </summary>
        /// <param name="templateSourceFile">Name of the template.</param>
        /// <param name="templateSource">Contains the styles and the format of the document.</param>
        /// <param name="environment"><see cref="DeploymentEnvironment"/>.</param>
        /// <param name="fileFormat"><see cref="IMsWordFileFormat"/>.</param>
        /// <param name="errorData">The details for a specific operation which will be included for tracking.</param>
        /// <param name="getFieldValue">Delegate function that returns field value(args:[fieldCode, MERGEFIELDVALUECASING]).</param>
        /// <param name="additionalProcessingOfFieldCode">Delegate function that returns field code(args:[fieldCode]).</param>
        /// <returns>Byte array.</returns>
        byte[] PopulateFieldsToTemplatedData(
            string templateSourceFile,
            byte[] templateSource,
            DeploymentEnvironment environment,
            IMsWordFileFormat fileFormat,
            JObject errorData,
            Func<string, StringComparison, string?> getFieldValue,
            Func<string, string> additionalProcessingOfFieldCode = null);
    }
}
