// <copyright file="ProductComponentConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Releases
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Entities;
    using UBind.Domain.Product.Component;

    /// <summary>
    /// For holding configuration specific to product components.
    /// </summary>
    public class ProductComponentConfiguration : IProductComponentConfiguration
    {
        private string productConfigurationJson;
        private Maybe<FormDataSchema>? formDataSchema;
        private IProductConfiguration productConfiguration;
        private IFieldSerializationBinder fieldSerializationBinder;
        private Component component;
        private string version;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductComponentConfiguration"/> class.
        /// </summary>
        /// <param name="releaseDetails">The app-specific details from a release.</param>
        /// <param name="fieldSerializationBinder">The json serialization binder for field types.</param>
        public ProductComponentConfiguration(
            ReleaseDetails releaseDetails,
            IFieldSerializationBinder fieldSerializationBinder)
        {
            this.WebFormAppConfigurationJson = releaseDetails.ConfigurationJson;
            this.WorkbookData = releaseDetails.FlexCelWorkbook;
            this.productConfigurationJson = releaseDetails.ProductJson;
            this.Files = releaseDetails.Files.ToList();
            this.Assets = releaseDetails.Assets.ToList();
            this.fieldSerializationBinder = fieldSerializationBinder;
        }

        /// <summary>
        /// Gets the configuration json for the web form app.
        /// </summary>
        public string WebFormAppConfigurationJson { get; }

        /// <summary>
        /// Gets the contents of the workbook for the web form app.
        /// </summary>
        public byte[] WorkbookData { get; }

        /// <summary>
        /// Gets the files associated with the release.
        /// </summary>
        public IReadOnlyList<Asset> Files { get; }

        /// <summary>
        /// Gets the assets associated with the release.
        /// </summary>
        public IReadOnlyList<Asset> Assets { get; }

        /// <summary>
        /// Gets the product configuration.
        /// </summary>
        public IProductConfiguration ProductConfiguration => this.productConfiguration ??
            (this.productConfiguration = this.productConfigurationJson != null
                ? new ProductConfiguration(this.productConfigurationJson, this.FormDataSchema.GetValueOrDefault(null))
                : new DefaultProductConfiguration() as IProductConfiguration);

        /// <summary>
        /// Gets the component of the product, if available, otherwise null.
        /// The product component will only be available fo products with configurations
        /// of version 2 or later.
        /// </summary>
        public Component Component
        {
            get
            {
                if (this.component == null)
                {
                    if (this.IsVersion2OrGreater)
                    {
                        Component component = JsonConvert.DeserializeObject<Component>(
                            this.WebFormAppConfigurationJson,
                            new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.Auto,
                                SerializationBinder = this.fieldSerializationBinder,
                            });
                        component.PopulateReferences();
                        this.component = component;
                    }
                }

                return this.component;
            }
        }

        /// <summary>
        /// Gets the configuration version, e.g. "2.0.0".
        /// </summary>
        public string Version
        {
            get
            {
                if (this.version == null)
                {
                    if (this.WebFormAppConfigurationJson.Length < 30)
                    {
                        // we have an empty config - this might happen when running unit tests.
                        // we'll just assume a version
                        this.version = "2.0.0";
                    }
                    else
                    {
                        string startSubstring = this.WebFormAppConfigurationJson.Substring(0, 30);
                        var pattern = new Regex("\"version\":\\s?\"(.*?)\"");
                        Match match = pattern.Match(startSubstring);
                        if (match.Success & match.Groups.Count > 1)
                        {
                            this.version = match.Groups[1].Value;
                        }
                        else
                        {
                            this.version = "1.0.0";
                        }
                    }
                }

                return this.version;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the configuration version is 2 or greater.
        /// </summary>
        public bool IsVersion2OrGreater => !this.Version.StartsWith("1.");

        /// <summary>
        /// Gets a value indicating whether the configuration version is 1.
        /// </summary>
        public bool IsVersion1 => this.Version.StartsWith("1.");

        /// <summary>
        /// Gets the form data schema.
        /// </summary>
        public Maybe<FormDataSchema> FormDataSchema
        {
            get
            {
                if (!this.formDataSchema.HasValue)
                {
                    if (this.IsVersion2OrGreater)
                    {
                        this.formDataSchema = Maybe<FormDataSchema>.From(new FormDataSchema(this.Component));
                    }
                    else
                    {
                        JObject configurationJObject = JObject.Parse(this.WebFormAppConfigurationJson);
                        JToken baseConfigToken = configurationJObject.GetValue("baseConfiguration");
                        this.formDataSchema = baseConfigToken != null && baseConfigToken.Type == JTokenType.Object
                            ? Maybe<FormDataSchema>.From(new FormDataSchema(baseConfigToken as JObject))
                            : Maybe<FormDataSchema>.None;
                    }
                }

                return this.formDataSchema.Value;
            }
        }
    }
}
