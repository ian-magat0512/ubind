// <copyright file="XmlTextToObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Xml;
    using Humanizer;
    using MorseCode.ITask;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Converter for XML Object Provider expecting an object of type text.
    /// </summary>
    public class XmlTextToObjectProvider : IObjectProvider
    {
        private readonly IProvider<Data<string>> textProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlTextToObjectProvider"/> class.
        /// </summary>
        /// <param name="textProvider">The text provider to be used.</param>
        public XmlTextToObjectProvider(IProvider<Data<string>> textProvider)
        {
            this.textProvider = textProvider;
        }

        public string SchemaReferenceKey => "xmlTextToObject";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<object>>> Resolve(IProviderContext providerContext)
        {
            var xmlString = (await this.textProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xmlString.ToString());
                var json = JsonConvert.SerializeXmlNode(doc);
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, new AutomationDictionaryConverter());
                return ProviderResult<Data<object>>.Success(new Data<object>(new ReadOnlyDictionary<string, object>(dictionary)));
            }
            catch (Exception ex)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.ValueToParse, xmlString.ToString().Truncate(80, "..."));
                errorData.Add(ErrorDataKey.ErrorMessage, ex.Message);

                if (ex.GetType() == typeof(XmlException))
                {
                    throw new ErrorException(Errors.Automation.InvalidXmlTextValue(this.SchemaReferenceKey, errorData));
                }
                else
                {
                    throw new ErrorException(Errors.Automation.ValueResolutionError(this.SchemaReferenceKey, errorData));
                }
            }
        }
    }
}
