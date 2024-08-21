// <copyright file="RazorTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Dynamic;
    using Humanizer;
    using Microsoft.Extensions.Logging;
    using MorseCode.ITask;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RazorEngine.Templating;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Generates a text output from a razor template as well as an optional data object that will be exposed to the template.
    /// If no data object is passed, the entire automation data is passed.
    /// </summary>
    public class RazorTextProvider : IProvider<Data<string>>
    {
        private readonly IProvider<Data<string>> razorTemplateProvider;
        private readonly IObjectProvider dataObjectProvider;
        private readonly IRazorEngineService razorEngine;
        private readonly ILogger<RazorTextProvider> logger;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="RazorTextProvider"/> class.
        /// </summary>
        /// <param name="razorTemplateProvider">A razor template passed as a text provider.</param>
        /// <param name="dataObjectProvider">The data object that will be exposed to the razor template.</param>
        /// <param name="razorEngineService">Razor engine service.</param>
        public RazorTextProvider(
            IProvider<Data<string>> razorTemplateProvider,
            IObjectProvider dataObjectProvider,
            IRazorEngineService razorEngineService,
            ILogger<RazorTextProvider> logger,
            ICachingResolver cachingResolver)
        {
            Contract.Assert(razorTemplateProvider != null);
            Contract.Assert(dataObjectProvider != null);

            this.razorTemplateProvider = razorTemplateProvider;
            this.dataObjectProvider = dataObjectProvider;
            this.razorEngine = razorEngineService;
            this.logger = logger;
            this.cachingResolver = cachingResolver;
        }

        public string SchemaReferenceKey => "razorText";

        /// <summary>
        ///  Provides a text output from a razor template as well as a data object that will be exposed to the template.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>A text value.</returns>
        public async ITask<IProviderResult<Data<string>>> Resolve(IProviderContext providerContext)
        {
            string razorTemplate = (await this.razorTemplateProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var dataObject = (await this.dataObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var jsonSerialization = JsonConvert.SerializeObject(dataObject);

            try
            {
                var tenant = await this.cachingResolver.GetTenantOrThrow(providerContext.AutomationData.ContextManager.Tenant.Id);
                OrganisationReadModel? organisation = null;
                if (providerContext.AutomationData.ContextManager.Organisation != null)
                {
                    organisation = await this.cachingResolver.GetOrganisationOrNull(
                        tenant.Id, providerContext.AutomationData.ContextManager.Organisation.Id);
                }

                UBind.Domain.Product.Product? product = null;
                if (providerContext.AutomationData.ContextManager.Product != null)
                {
                    product = await this.cachingResolver.GetProductOrNull(
                        tenant.Id, providerContext.AutomationData.ContextManager.Product.Id);
                }

                var expandoObject = JsonConvert.DeserializeObject<ExpandoObject>(jsonSerialization);
                var executionId = Guid.NewGuid();
                this.logger.LogWarning($"Automations: About to execute the razor template at path "
                    + $"{providerContext.CurrentActionDataPath} for tenant {tenant.Details.Alias}, organisation "
                    + $"{organisation?.Alias ?? "[unknown]"}, product {product?.Details.Alias ?? "[unknown]"}. "
                    + "Poorly written Razor templates can crash the app with a stack overflow, and our Razor execution "
                    + "does not currently run in a separate sandboxed process, so it could crash uBind. "
                    + $"Execution ID: {executionId}");
                string result = this.razorEngine.RunCompile(razorTemplate, razorTemplate, null, expandoObject);
                this.logger.LogWarning($"Automations: Razor template execution {executionId} completed successfully.");
                return ProviderResult<Data<string>>.Success(result);
            }
            catch (Exception ex)
            {
                JObject errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.ValueToParse, jsonSerialization.Truncate(80, "..."));
                errorData.Add(ErrorDataKey.ErrorMessage, ex.Message.Truncate(350, "..."));
                throw new ErrorException(Errors.Automation.ValueResolutionError(
                    this.SchemaReferenceKey, errorData));
            }
        }
    }
}
