// <copyright file="Report.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// This class is needed because we need to generated json representation of report entity that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class Report : BaseEntity<ReportReadModel>
    {
        public Report(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Report"/> class.
        /// </summary>
        /// <param name="model">The report read model with related entities.</param>
        public Report(ReportReadModel model, string baseApiUrl, ICachingResolver cachingResolver)
            : base(model.Id, model.CreatedTicksSinceEpoch, model.LastModifiedTicksSinceEpoch)
        {
            this.Name = model.Name;
            this.Description = model.Description;
            this.Products = model.Products?.Select(p => new Product(p, baseApiUrl, cachingResolver)).ToList();
            this.SourceData = !string.IsNullOrEmpty(model.SourceData) ?
                model.SourceData.Split(',').Where(s => s != string.Empty).ToList() : null;
            this.MimeType = model.MimeType;
            this.Filename = model.Filename;
            this.Body = model.Body;
        }

        /// <summary>
        /// Gets the name of the report.
        /// </summary>
        [JsonProperty(PropertyName = "name", Order = 21)]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the report.
        /// </summary>
        [JsonProperty(PropertyName = "description", Order = 22)]
        public string Description { get; private set; }

        /// <summary>
        /// Gets the products of the report.
        /// </summary>
        [JsonProperty(PropertyName = "products", Order = 23)]
        public List<Product> Products { get; private set; }

        /// <summary>
        /// Gets the source data of the report.
        /// </summary>
        [JsonProperty(PropertyName = "sourceData", Order = 24)]
        public List<string> SourceData { get; private set; }

        /// <summary>
        /// Gets the mime-type of the report.
        /// </summary>
        [JsonProperty(PropertyName = "mimeType", Order = 25)]
        public string MimeType { get; private set; }

        /// <summary>
        /// Gets the filename of the report.
        /// </summary>
        [JsonProperty(PropertyName = "filename", Order = 26)]
        public string Filename { get; private set; }

        /// <summary>
        /// Gets the body of the report.
        /// </summary>
        [JsonProperty(PropertyName = "body", Order = 27)]
        public string Body { get; private set; }
    }
}
