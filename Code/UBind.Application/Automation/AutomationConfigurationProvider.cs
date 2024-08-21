// <copyright file="AutomationConfigurationProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json.Linq;
    using StackExchange.Profiling;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <inheritdoc/>
    public class AutomationConfigurationProvider : IAutomationConfigurationProvider
    {
        private readonly IAutomationConfigurationModelProvider modelProvider;
        private readonly IServiceProvider dependencyProvider;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationConfigurationProvider"/> class.
        /// </summary>
        /// <param name="modelProvider">Provider for configuration model.</param>
        /// <param name="dependencyProvider">Container for depenencies required in automation building.</param>
        public AutomationConfigurationProvider(
            IAutomationConfigurationModelProvider modelProvider,
            IServiceProvider dependencyProvider,
            ICachingResolver cachingResolver)
        {
            this.modelProvider = modelProvider;
            this.dependencyProvider = dependencyProvider;
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public async Task<AutomationsConfiguration?> GetAutomationConfigurationOrNull(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            Guid? productReleaseId)
        {
            using (MiniProfiler.Current.Step(nameof(AutomationConfigurationProvider) + "." + nameof(this.GetAutomationConfigurationOrNull)))
            {
                var schemaAssertionFailedError = Errors.Automation.InvalidAutomationConfiguration(new JObject());
                try
                {
                    AutomationsConfigurationModel? model;
                    model = this.modelProvider.GetAutomationConfigurationOrNull(
                        tenantId,
                        productId,
                        environment,
                        productReleaseId);
                    if (model == null)
                    {
                        return null;
                    }

                    var configuration = model.Build(this.dependencyProvider);
                    return configuration;
                }
                catch (ErrorException ex) when (ex.Error.Code.Equals(schemaAssertionFailedError.Code))
                {
                    var cachingResolver = this.dependencyProvider.GetService<ICachingResolver>();
                    var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
                    var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(tenantId, productId);
                    var errorData = new JObject
                    {
                        { "tenantAlias", tenantAlias },
                        { "productAlias", productAlias },
                        { "environment", environment.Humanize().Camelize() },
                        { "failingAutomationConfiguration", ex.Error.Data.SelectToken("failingAutomationConfiguration") },
                    };
                    throw new ErrorException(Errors.Automation.InvalidAutomationConfiguration(errorData));
                }
            }
        }

        /// <inheritdoc/>
        public Task<AutomationsConfiguration> GetAutomationConfigurationOrThrow(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            Guid? productReleaseId)
        {
            var model = this.modelProvider.GetAutomationConfigurationOrThrow(
                tenantId,
                productId,
                environment,
                productReleaseId);
            var configuration = model.Build(this.dependencyProvider);
            return Task.FromResult(configuration);
        }

        /// <inheritdoc/>
        public AutomationsConfiguration GetAutomationConfiguration(AutomationsConfigurationModel model)
        {
            using (MiniProfiler.Current.Step(nameof(AutomationConfigurationProvider) + "." + nameof(this.GetAutomationConfiguration)))
            {
                var configuration = model.Build(this.dependencyProvider);
                return configuration;
            }
        }
    }
}
