// <copyright file="IDelimiterSeparatedValuesFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.DelimiterSeparatedValues
{
    /// <summary>
    /// Provides the contract to be used for the delimiter separated values file provider.
    /// </summary>
    public interface IDelimiterSeparatedValuesFileProvider
    {
        /// <summary>
        /// Get the list of delimiter separated values files from a given base path and file type.
        /// </summary>
        /// <param name="basePath">The base path where the delimiter file(s) are located.</param>
        /// <param name="dataSeparatedValuesFileTypes">The type of the delimiter file.</param>
        /// <returns>Return a list of delimiter separated values files from a given base path and file types.</returns>
        string[] GetDelimiterSeparatedValuesFiles(string basePath, DelimiterSeparatedValuesFileTypes dataSeparatedValuesFileTypes);
    }
}
