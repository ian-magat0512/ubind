// <copyright file="ProductTemplateJObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Threading.Tasks;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Product;

    /// <summary>
    /// Product template JSON object provider.
    /// </summary>
    public class ProductTemplateJObjectProvider : TemplateJObjectProvider, IJObjectProvider
    {
        private IConfigurationService configurationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductTemplateJObjectProvider"/> class.
        /// </summary>
        /// <param name="configurationService">The release query service.</param>
        public ProductTemplateJObjectProvider(IConfigurationService configurationService)
        {
            this.configurationService = configurationService;
        }

        /// <inheritdoc/>
        public async Task CreateJsonObject(ApplicationEvent applicationEvent)
        {
            var releaseContext = new ReleaseContext(
                applicationEvent.Aggregate.TenantId,
                applicationEvent.Aggregate.ProductId,
                applicationEvent.Aggregate.Environment,
                applicationEvent.ProductReleaseId);
            IProductComponentConfiguration componentConfig =
                await this.configurationService.GetProductComponentConfiguration(releaseContext, WebFormAppType.Quote);
            if (componentConfig.IsVersion2OrGreater)
            {
                this.SetJsonObjectFromTextElementCategory(componentConfig.Component, "Product", "Product");
            }
            else
            {
                this.SetJsonObjectFromParsedConfigTextElement(
                    componentConfig.WebFormAppConfigurationJson,
                    "product",
                    "Product");
            }
        }
    }
}
