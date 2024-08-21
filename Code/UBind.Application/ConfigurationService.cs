// <copyright file="ConfigurationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using StackExchange.Profiling;
    using UBind.Application.MicrosoftGraph;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Dto;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component.Form;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Service for retrieving form product configuration (form schema etc.)
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly ICachingAuthenticationTokenProvider authenticator;
        private readonly IFilesystemFileRepository fileRepository;
        private readonly IFilesystemStoragePathService pathService;
        private readonly IReleaseQueryService releaseQueryService;
        private readonly IProductRepository productRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly IReleaseRepository releaseRepository;
        private readonly IReleaseService releaseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
        /// </summary>
        /// <param name="authenticator">Authenticator for obtainingn authentication token.</param>
        /// <param name="fileRepository">A repository for accessing source files for a release.</param>
        /// <param name="oneDriveFilePathService">Service for building one drive file paths.</param>
        /// <param name="releaseQueryService">Service for getting the current release for a given product context.</param>
        /// <param name="productRepository">Repository for products.</param>
        /// <param name="releaseRepository">Repository for releases.</param>
        /// <param name="tenantRepository">Repository for tenant.</param>
        /// <param name="releaseService">Service for creating releases.</param>
        public ConfigurationService(
            ICachingAuthenticationTokenProvider authenticator,
            IFilesystemFileRepository fileRepository,
            IFilesystemStoragePathService oneDriveFilePathService,
            IReleaseQueryService releaseQueryService,
            IProductRepository productRepository,
            IReleaseRepository releaseRepository,
            ICachingResolver cachingResolver,
            IReleaseService releaseService)
        {
            this.cachingResolver = cachingResolver;
            this.authenticator = authenticator;
            this.fileRepository = fileRepository;
            this.pathService = oneDriveFilePathService;
            this.releaseQueryService = releaseQueryService;
            this.productRepository = productRepository;
            this.releaseRepository = releaseRepository;
            this.releaseService = releaseService;
        }

        /// <inheritdoc/>
        public async Task<ReleaseProductConfiguration> GetConfigurationAsync(
            ReleaseContext releaseContext,
            WebFormAppType webFormAppType = WebFormAppType.Quote)
        {
            using (MiniProfiler.Current.Step(nameof(ConfigurationService) + "." + nameof(this.GetConfigurationAsync)))
            {
                await this.CheckTenantAndProduct(releaseContext);
                var release = this.releaseQueryService.GetRelease(releaseContext);
                ProductComponentConfiguration componentConfig =
                    release.GetProductComponentConfigurationOrThrow(webFormAppType);
                var config = componentConfig.WebFormAppConfigurationJson;
                return await Task.FromResult(new ReleaseProductConfiguration
                {
                    ConfigurationJson = config,
                    ProductReleaseId = release.ReleaseId,
                });
            }
        }

        /// <inheritdoc/>
        public async Task<IProductComponentConfiguration> GetProductComponentConfiguration(
            ReleaseContext releaseContext,
            WebFormAppType webFormAppType)
        {
            await this.CheckTenantAndProduct(releaseContext);
            var release = this.releaseQueryService.GetRelease(releaseContext);
            return release.GetProductComponentConfigurationOrThrow(webFormAppType);
        }

        /// <inheritdoc/>
        public async Task<DisplayableFieldDto> GetDisplayableFieldsAsync(
            ReleaseContext releaseContext,
            WebFormAppType formType = WebFormAppType.Quote)
        {
            var release = this.releaseQueryService.GetRelease(releaseContext);
            if (release[formType] == null)
            {
                return new DisplayableFieldDto(new List<string>(), new List<string>(), false, false);
            }

            ProductComponentConfiguration componentConfig = release.GetProductComponentConfigurationOrThrow(formType);
            if (componentConfig.IsVersion1)
            {
                ReleaseProductConfiguration config = await this.GetConfigurationAsync(releaseContext, formType);
                return this.GetDisplayableFieldsFromConfigurationV1(config);
            }
            else
            {
                return this.GetDisplayableFieldsFromConfigurationV2(componentConfig);
            }
        }

        /// <inheritdoc/>
        public bool DoesConfigurationExist(ReleaseContext releaseContext, WebFormAppType webFormAppType)
        {
            var release = this.releaseQueryService.GetRelease(releaseContext);
            ProductComponentConfiguration componentConfig = release[webFormAppType];
            if (componentConfig == null)
            {
                return false;
            }

            var config = componentConfig.WebFormAppConfigurationJson;
            return config != null;
        }

        /// <inheritdoc/>
        public bool DoesConfigurationExist(ProductContext productContext, WebFormAppType webFormAppType)
        {
            var releaseContext = this.releaseQueryService.GetDefaultReleaseContextOrNull(
                productContext.TenantId, productContext.ProductId, productContext.Environment);
            if (releaseContext == null)
            {
                return false;
            }

            return this.DoesConfigurationExist(releaseContext.Value, webFormAppType);
        }

        private async Task CheckTenantAndProduct(ReleaseContext releaseContext)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(releaseContext.TenantId);
            if (tenant.Details == null)
            {
                throw new ErrorException(Errors.General.NotFound("tenant", releaseContext.TenantId));
            }

            if (tenant.Details.Deleted)
            {
                throw new ErrorException(Errors.Tenant.Deleted(tenant.Details.Alias));
            }

            if (tenant.Details.Disabled)
            {
                throw new ErrorException(Errors.Tenant.Disabled(tenant.Details.Alias));
            }

            var product = await this.cachingResolver.GetProductOrThrow(tenant.Id, releaseContext.ProductId);
            if (product.Details == null)
            {
                throw new ErrorException(Errors.General.NotFound("product", releaseContext.ProductId));
            }

            if (product.Details.Disabled)
            {
                throw new ErrorException(Errors.Product.Disabled(product.Details.Alias));
            }

            if (product.Details.Deleted)
            {
                throw new ErrorException(Errors.Product.Deleted(product.Details.Alias));
            }
        }

        private DisplayableFieldDto GetDisplayableFieldsFromConfigurationV1(ReleaseProductConfiguration config)
        {
            List<string> displayableFields = new List<string>();
            List<string> repeatingDisplayableFields = new List<string>();
            bool displayableField = false;
            bool repeatingDisplayableField = false;

            JObject rawConfigObj;
            JObject baseConfigObj;
            try
            {
                rawConfigObj = JObject.Parse(config.ConfigurationJson.Replace(",}", "}"));
                baseConfigObj = JObject.Parse(rawConfigObj["baseConfiguration"].ToString());
            }
            catch (JsonReaderException ex)
            {
                throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration("Could not parse product configuration."), ex);
            }

            var displayableFieldsObj = baseConfigObj.GetValue("displayableFields");
            var repeatingQuestiondisplayableFieldsObj = baseConfigObj.GetValue("repeatingQuestiondisplayableFields");
            if (displayableFieldsObj != null)
            {
                displayableFields = displayableFieldsObj.ToObject<List<string>>();
                displayableField = true;
            }

            if (repeatingQuestiondisplayableFieldsObj != null)
            {
                repeatingDisplayableFields = repeatingQuestiondisplayableFieldsObj.ToObject<List<string>>();
                repeatingDisplayableField = true;
            }

            return new DisplayableFieldDto(displayableFields, repeatingDisplayableFields, displayableField, repeatingDisplayableField);
        }

        private DisplayableFieldDto GetDisplayableFieldsFromConfigurationV2(ProductComponentConfiguration componentConfig)
        {
            List<string> displayableFields = componentConfig.Component.Form.QuestionSets
                .SelectMany(qs => qs.Fields)
                .Where(field => field is IDataStoringField dataStoringField
                    && (!dataStoringField.Displayable.HasValue || dataStoringField.Displayable.Value))
                .Select(field => field.Key)
                .ToList();

            List<string> displayableRepeatingFields = componentConfig.Component.Form.QuestionSets
                .SelectMany(qs => qs.Fields)
                .Where(field => field is RepeatingField repeatingField
                    && (repeatingField.Displayable == null || repeatingField.Displayable.Value))
                .Select(field => field.Key)
                .ToList();

            // Include repeating question set fields for the displayable repeating fields
            var displayableRepeatingQuestionSetFields = componentConfig.Component.Form.RepeatingQuestionSets
                .SelectMany(r => r.Fields)
                .Where(r => displayableRepeatingFields.Contains(r.QuestionSetKey))
                .Where(field => field is IDataStoringField dataStoringField
                    && (!dataStoringField.Displayable.HasValue || dataStoringField.Displayable.Value))
                .Select(r => r.Key).ToList();
            displayableRepeatingFields.AddRange(displayableRepeatingQuestionSetFields);

            return new DisplayableFieldDto(
                displayableFields,
                displayableRepeatingFields,
                displayableFields.Any(),
                displayableRepeatingFields.Any());
        }
    }
}
