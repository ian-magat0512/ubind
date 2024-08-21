// <copyright file="FormConfigurationGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.FlexCel;
    using UBind.Application.MicrosoftGraph;
    using UBind.Application.Product.Component.Configuration.Parsers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Repositories;
    using UBind.Domain.Validation;

    /// <inheritdoc/>
    public class FormConfigurationGenerator : IFormConfigurationGenerator
    {
        private const string FormDataLocatorTokenName = "dataLocators";
        private const string ContextEntitiesTokenName = "contextEntities";
        private readonly ICachingResolver cachingResolver;
        private readonly ICachingAuthenticationTokenProvider authenticator;
        private readonly IClock clock;
        private readonly ILogger<FormConfigurationGenerator> logger;
        private readonly IFilesystemFileRepository fileRepository;
        private readonly IFilesystemStoragePathService filesystemStoragePathService;
        private readonly IDevReleaseRepository devReleaseRepository;
        private readonly IWorkbookProductComponentConfigurationReader workbookConfigReader;
        private readonly IFieldSerializationBinder fieldSerializationBinder;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormConfigurationGenerator"/> class.
        /// </summary>
        /// <param name="authenticator">Authenticator for obtainingn authentication token.</param>
        /// <param name="devReleaseRepository">Repository for Dev Release.</param>
        /// <param name="fileRepository">A repository for accessing source files for a release.</param>
        /// <param name="filesystemStoragePathService">Service for building storage file paths.</param>
        /// <param name="workbookConfigReader">The workbook product component configuration reader.</param>
        /// <param name="fieldSerializationBinder">The binder that binds field type properties to a Field type.</param>
        /// <param name="clock">The clock.</param>
        /// <param name="logger">The logger.</param>
        public FormConfigurationGenerator(
            ICachingAuthenticationTokenProvider authenticator,
            IDevReleaseRepository devReleaseRepository,
            IFilesystemFileRepository fileRepository,
            IFilesystemStoragePathService filesystemStoragePathService,
            IWorkbookProductComponentConfigurationReader workbookConfigReader,
            IFieldSerializationBinder fieldSerializationBinder,
            IClock clock,
            ILogger<FormConfigurationGenerator> logger,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.authenticator = authenticator;
            this.fileRepository = fileRepository;
            this.filesystemStoragePathService = filesystemStoragePathService;
            this.devReleaseRepository = devReleaseRepository;
            this.workbookConfigReader = workbookConfigReader;
            this.fieldSerializationBinder = fieldSerializationBinder;
            this.clock = clock;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string> Generate(
            Guid tenantId,
            Guid productId,
            WebFormAppType webFormAppType,
            FlexCelWorkbook workbook,
            string workflowJson,
            string? productJson,
            string? paymentFormJson)
        {
            using (MiniProfiler.Current.Step(nameof(FormConfigurationGenerator) + "." + nameof(this.Generate)))
            {
                string authenticationToken = await this.fileRepository.GetAuthenticationToken();
                var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
                var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(tenantId, productId);

                var formModelPath = this.filesystemStoragePathService.GetDevFormModelFilePath(tenantAlias, productAlias, webFormAppType);
                Task<JObject> formModelTask = this.TryReadJsonFile(formModelPath, authenticationToken);
                Task<Component> componentTask = this.ReadWorkbookForComponent(workbook);
                await Task.WhenAll(
                    formModelTask,
                    componentTask);
                Component component = componentTask.Result;
                component.Form.WorkflowConfiguration = this.ParseJsonOrThrow(workflowJson, "workflow.json");
                if (!string.IsNullOrEmpty(paymentFormJson))
                {
                    component.PaymentFormConfiguration = this.ParseJsonOrThrow(paymentFormJson, "payment.form.json");
                }

                component.Form.FormModel = formModelTask.Result;
                if (!string.IsNullOrEmpty(productJson))
                {
                    JObject productJObject = this.ParseJsonOrThrow(productJson, "product.json");
                    var isProductJsonValid = productJObject != null && productJObject.Type != JTokenType.Null;
                    component.DataLocators = isProductJsonValid
                        ? (JObject?)productJObject.SelectToken(FormDataLocatorTokenName)
                        : JObject.Parse("{}");
                    component.ContextEntities = isProductJsonValid
                        ? productJObject.SelectToken(ContextEntitiesTokenName)
                        : null;
                }
                this.MergeOldStripePublicApiKeys(workbook, component);
                this.ValidateComponentOrThrow(component);
                return this.SerialiseToJson(component);
            }
        }

        /// <summary>
        /// Reads any old stripe public api key settings from older workbooks and merges them into the
        /// public payment configuration JObject of the component.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="component">The component.</param>
        private void MergeOldStripePublicApiKeys(FlexCelWorkbook workbook, Component component)
        {
            JObject oldStripePublicApiKeys =
                new WorkbookSettingsTableParser(workbook, null).ParseForOldStripePublicApiKeys();
            if (oldStripePublicApiKeys != null)
            {
                if (component.PaymentFormConfiguration != null)
                {
                    component.PaymentFormConfiguration.Merge(oldStripePublicApiKeys);
                }
                else
                {
                    component.PaymentFormConfiguration = oldStripePublicApiKeys;
                }
            }
        }

        private string SerialiseToJson(Component component)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = this.fieldSerializationBinder,
            };
            return JsonConvert.SerializeObject(component, serializerSettings);
        }

        private void ValidateComponentOrThrow(Component component)
        {
            IReadOnlyList<ValidationResult> validationResults = component.Validate();
            if (validationResults.Any())
            {
                var additionalDetails = validationResults.FlattenCompositeResults()
                    .Select(vr => vr.ErrorMessage);
                throw new ErrorException(Errors.Product.WorkbookParseFailure(
                        "Configuration validation failed.",
                        additionalDetails));
            }
        }

        private async Task<Component> ReadWorkbookForComponent(FlexCelWorkbook workbook)
        {
            return await Task.Run(() => this.workbookConfigReader.Read(workbook));
        }

        private async Task<JObject> TryReadJsonFile(string path, string authenticationToken)
        {
            Maybe<string> result = await this.fileRepository.TryGetFileStringContents(path, authenticationToken);
            if (result.HasNoValue)
            {
                return null;
            }

            return this.ParseJsonOrThrow(result.Value, Path.GetFileName(path));
        }

        private async Task<JObject> ReadJsonFile(string path, string authenticationToken)
        {
            string json = await this.fileRepository.GetFileStringContents(path, authenticationToken);
            return this.ParseJsonOrThrow(json, Path.GetFileName(path));
        }

        private JObject ParseJsonOrThrow(string json, string filename)
        {
            try
            {
                return JObject.Parse(json);
            }
            catch (JsonReaderException ex)
            {
                var error = Errors.JsonDocument.JsonInvalid(
                    filename,
                    ex.Message,
                    ex.LineNumber,
                    ex.LinePosition,
                    ex.Path,
                    json);
                throw new ErrorException(error);
            }
        }
    }
}
