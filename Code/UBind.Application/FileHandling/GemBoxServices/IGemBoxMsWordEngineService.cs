// <copyright file="IGemBoxMsWordEngineService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>
namespace UBind.Application.FileHandling.GemBoxServices
{
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers.File;

    /// <summary>
    /// Defines the required contract for implementing a template service.
    /// </summary>
    public interface IGemBoxMsWordEngineService
    {
        /// <summary>
        ///  Merges the data of the provider into the template.
        /// </summary>
        /// <param name="dataSource">Contains the data that will be merged</param>
        /// <param name="templateContent">The content of the template file</param>
        /// <returns>The content of the output word document</returns>
        byte[] MergeDataToTemplate(
            Guid tenantId,
            JObject dataSource,
            byte[] templateContent,
            bool removeUnusedMergeField,
            bool removeRangesWhereAllMergeFieldsAreUnused,
            bool removeTablesWhereAllMergeFieldsAreUnused,
            bool removeTableRowsWhereAllMergeFieldsAreUnused,
            bool removeParagraphsWhereAllMergeFieldsAreUnused,
            IEnumerable<ContentSourceFile> sourceFiles);
    }
}
