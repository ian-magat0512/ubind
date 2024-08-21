// <copyright file="LiquidTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Text
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using DotLiquid;
    using Humanizer;
    using MorseCode.ITask;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Generates a text output from a liquid template as well as an optional data object that will be exposed to the template.
    /// If no data object is passed, the entire automation data is passed.
    /// </summary>
    public class LiquidTextProvider : IProvider<Data<string>>
    {
        private readonly IProvider<Data<string>> liquidTemplateProvider;
        private readonly IObjectProvider dataObjectProvider;
        private readonly IEnumerable<IProvider<LiquidTextSnippet>> snippetProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiquidTextProvider"/> class.
        /// </summary>
        /// <param name="liquidTemplateProvider">A liquid template passed as a text provider.</param>
        /// <param name="dataObjectProvider">A data object that will be exposed to the liquid provider.</param>
        /// <param name="snippetProvider">The snippet provider.</param>
        public LiquidTextProvider(
            IProvider<Data<string>> liquidTemplateProvider,
            IObjectProvider dataObjectProvider,
            IEnumerable<IProvider<LiquidTextSnippet>> snippetProvider)
        {
            Contract.Assert(liquidTemplateProvider != null);
            Contract.Assert(dataObjectProvider != null);

            this.liquidTemplateProvider = liquidTemplateProvider;
            this.dataObjectProvider = dataObjectProvider;
            this.snippetProvider = snippetProvider;
        }

        public string SchemaReferenceKey => "liquidText";

        /// <summary>
        ///  Provides a text value from a liquid template as well as a data object that will be exposed to the template.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>A text value.</returns>
        public async ITask<IProviderResult<Data<string>>> Resolve(IProviderContext providerContext)
        {
            var liquidTemplate = (await this.liquidTemplateProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var liquidSnippets = this.snippetProvider != null
                    ? await this.snippetProvider.SelectAsync(async x => (await x.Resolve(providerContext)).GetValueOrThrowIfFailed())
                    : null;
            List<LiquidTextSnippet> snippets = liquidSnippets?.ToList() ?? new List<LiquidTextSnippet>();

            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            if (string.IsNullOrEmpty(liquidTemplate?.DataValue))
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "liquidTemplate",
                    this.SchemaReferenceKey));
            }

            var dataObject = (await this.dataObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;

            try
            {
                var dictionary = ObjectHelper.ToDictionary(dataObject);
                Hash dataHash = Hash.FromDictionary(dictionary);
                var template = Template.Parse(liquidTemplate.DataValue);

                var renderParam = new RenderParameters(CultureInfo.GetCultureInfo(Locales.en_AU));
                renderParam.LocalVariables = dataHash;
                renderParam.Registers = Hash.FromDictionary(ObjectHelper.ToDictionary(snippets.ToDictionary(x => "snippet:" + x.Alias, x => x.Template)));
                return ProviderResult<Data<string>>.Success(template.Render(renderParam));
            }
            catch (Exception ex)
            {
                var objectString = JsonConvert.SerializeObject(dataObject);
                errorData.Add(ErrorDataKey.ValueToParse, objectString.Truncate(80, "..."));
                errorData.Add(ErrorDataKey.ErrorMessage, ex.Message);
                throw new ErrorException(Errors.Automation.ValueResolutionError(this.SchemaReferenceKey, errorData), ex);
            }
        }
    }
}
