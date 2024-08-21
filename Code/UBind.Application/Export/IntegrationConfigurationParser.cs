// <copyright file="IntegrationConfigurationParser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using Newtonsoft.Json;
    using UBind.Application.Export.WebServiceTextProviders;
    using UBind.Application.FileHandling;

    /// <summary>
    /// For parsing configuration json into a model that can be built.
    /// </summary>
    public static class IntegrationConfigurationParser
    {
        private static JsonConverter[] converters = new JsonConverter[]
            {
                new GenericConverter<IExporterModel<EventExporterAction>>(
                    new TypeMap
                    {
                        { "email", typeof(EmailEventExporterActionModel) },
                        { "http", typeof(HttpEventExporterActionModel) },
                        { "filedocument", typeof(FileDocumentEventExporterActionModel) },
                    }),
                new GenericConverter<IExporterModel<EventExporterCondition>>(
                    new TypeMap
                    {
                        { "formFieldValueEquals", typeof(FormFieldValueEqualsEventExporterConditionModel) },
                        { "newWorkflowStepEquals", typeof(WorkFlowStepEventExporterConditionModel) },
                        { "quoteStateEquals", typeof(QuoteStateChangeEventExporterModel) },
                        { "replayOnly", typeof(ReplayOnlyEventExporterConditionModel) },
                    }),

                new TextProviderModelConverter(
                    new TypeMap
                    {
                        { "fixed", typeof(FixedTextProviderModel) },
                        { "formField", typeof(FormFieldTextProviderModel) },
                        { "formDataJson", typeof(FormModelTextProviderModel) },
                        { "razor", typeof(RazorTextProviderModel) },
                        { "environment", typeof(EnvironmentTextProviderModel) },
                        { "jsonToUrlEncoded", typeof(JsonToUrlEncodingTextProviderModel) },
                        { "applicationData", typeof(FlatApplicationJsonProviderModel) },
                    }),
                new WebServiceTextProviderModelConverter(
                    new TypeMap
                    {
                        { "fixed", typeof(FixedTextWebServiceTextProviderModel) },
                        { "dotLiquid", typeof(DotLiquidTemplateTextProviderModel) },
                        { "url", typeof(UrlWebServiceTextProviderModel) },
                        { "formField", typeof(FormFieldWebServiceTextProviderModel) },
                    }),
                new AttachmentProviderModelConverter(
                    new TypeMap
                    {
                        { "document", typeof(MsWordTemplateFileProviderModel) },
                        { "file", typeof(StaticFileProviderModel) },
                        { "existingDocument", typeof(SavedFileProviderModel) },
                        { "upload", typeof(UploadedFileProviderModel) },
                        { "text", typeof(SourceTextProviderModel) },
                    }),
            };

        /// <summary>
        /// Parse integration configuration json.
        /// </summary>
        /// <param name="exportConfigJson">The configuration JSON.</param>
        /// <returns>An integration configuration model that can be built.</returns>
        public static IntegrationConfigurationModel Parse(string exportConfigJson) =>
            JsonConvert.DeserializeObject<IntegrationConfigurationModel>(exportConfigJson, converters);
    }
}
