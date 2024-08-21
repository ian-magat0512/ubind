// <copyright file="IDelimiterSeparatedValuesService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.DelimiterSeparatedValues
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// Provides the contract to be used for the delimiter separated values services.
    /// </summary>
    public interface IDelimiterSeparatedValuesService
    {
        /// <summary>
        /// Get the list of pipe separated values files by the given base path and file group function.
        /// </summary>
        /// <param name="delimiterSeparatedValuesFileType">xx.</param>
        /// <param name="basePath">The base path where delimiter file(s) are located.</param>
        /// <param name="funcConstructGroupName">The function to be invoked to retrieve the file group name.</param>
        /// <returns>Returns an array of tuples with filename and group name.</returns>
        IReadOnlyList<(string FileName, string GroupName)> GetDsvFilesWithGroup(DelimiterSeparatedValuesFileTypes delimiterSeparatedValuesFileType, string basePath, Func<string, string> funcConstructGroupName);

        /// <summary>
        /// Convert a delimiter separated values files into a data table.
        /// </summary>
        /// <param name="fileFullPath">The path of the delimiter separated values file.</param>
        /// <param name="delimiter">The string delimiter of the file.</param>
        /// <param name="dataTableContainerAndDefinition">The data table containing the column definition.</param>
        /// <returns>Return a data table filled with records from the converted files.</returns>
        DataTable ConvertDelimiterSeparatedValuesToDataTable(
            string fileFullPath,
            string delimiter,
            DataTable dataTableContainerAndDefinition);

        /// <summary>
        /// Convert a csv string into a data table.
        /// </summary>
        /// <param name="csvString">The csv string.</param>
        /// <returns>Return a data table filled with records from the converted files.</returns>
        DataTable ConvertCsvStringToDataTable(string csvString);
    }
}
