﻿// <copyright file="WebServiceTextProviderModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.WebServiceTextProviders
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// For deserializing text provider models from json.
    /// </summary>
    /// <remarks>
    /// Text fields are interpreted as fixed text providers
    /// .</remarks>
    public class WebServiceTextProviderModelConverter : GenericConverter<IExporterModel<IWebServiceTextProvider>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceTextProviderModelConverter"/> class.
        /// </summary>
        /// <param name="typeMap">A map from "type" field values to cncrete child types.</param>
        public WebServiceTextProviderModelConverter(TypeMap typeMap)
            : base(typeMap)
        {
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null)
            {
                return new FixedTextWebServiceTextProviderModel { Text = value.ToString() };
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
