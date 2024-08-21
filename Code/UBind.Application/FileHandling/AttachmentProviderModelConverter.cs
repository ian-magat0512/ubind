// <copyright file="AttachmentProviderModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Export;

    /// <summary>
    /// For deserializing attachment provider models from json.
    /// </summary>
    /// <remarks>
    /// Attachments are interpreted as fixed attachment providers.
    /// .</remarks>
    public class AttachmentProviderModelConverter
        : GenericConverter<IExporterModel<IAttachmentProvider>>
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="AttachmentProviderModelConverter" /> class.
        /// </summary>
        /// <param name="typeMap">
        /// A map from "type" field values to concrete child types.
        /// .</param>
        public AttachmentProviderModelConverter(TypeMap typeMap)
            : base(typeMap)
        {
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null)
            {
                return new MsWordTemplateFileProviderModel
                {
                    TemplateName = new FixedTextProviderModel() { Text = value.ToString() },
                };
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
