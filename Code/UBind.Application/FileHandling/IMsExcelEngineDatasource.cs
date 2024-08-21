// <copyright file="IMsExcelEngineDatasource.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using Newtonsoft.Json.Linq;
    using UBind.Domain;

    /// <summary>
    /// Defines the required contract for implementing Excel engine datasource.
    /// </summary>
    public interface IMsExcelEngineDatasource
    {
        /// <summary>
        /// Gets the datasource content.
        /// </summary>
        byte[] Content { get; }

        /// <summary>
        /// Gets the deployment environment.
        /// </summary>
        DeploymentEnvironment Environment { get; }

        /// <summary>
        /// Gets the object at the given json pointer.
        /// </summary>
        /// <param name="path">The JSON pointer/path to the location where the object is to be found.</param>
        /// <returns>The entity from datasource.</returns>
        JToken GetObject(string path);
    }
}
