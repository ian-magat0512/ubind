// <copyright file="LiquidTextSnippetProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents an instance of HttpHeader with a name and multiple values.
    /// </summary>
    public class LiquidTextSnippetProvider : IProvider<LiquidTextSnippet>
    {
        private readonly IProvider<Data<string>> aliasProvider;
        private readonly IProvider<Data<string>> liquidTemplateProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiquidTextSnippetProvider"/> class.
        /// </summary>
        /// <param name="aliasProvider">The string name provider.</param>
        /// <param name="liquidTemplateProvider">A collection of value providers.</param>
        public LiquidTextSnippetProvider(
            IProvider<Data<string>> aliasProvider,
            IProvider<Data<string>> liquidTemplateProvider)
        {
            this.aliasProvider = aliasProvider;
            this.liquidTemplateProvider = liquidTemplateProvider;
        }

        public string SchemaReferenceKey => "snippets";

        /// <summary>
        /// Resolves the key-value pairs necessary for headers.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The key-value pairs.</returns>
        public async ITask<IProviderResult<LiquidTextSnippet>> Resolve(IProviderContext providerContext)
        {
            var alias = (await this.aliasProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var template = (await this.liquidTemplateProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            return ProviderResult<LiquidTextSnippet>.Success(new LiquidTextSnippet(
                alias.ToString(),
                template.ToString()));
        }
    }
}
