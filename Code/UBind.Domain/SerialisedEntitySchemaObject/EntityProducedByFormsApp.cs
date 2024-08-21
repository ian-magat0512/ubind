// <copyright file="EntityProducedByFormsApp.cs" company="uBind">
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
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;

    /// <summary>
    /// This class is needed because we need to have a base class for all entities that was produced from forms app.
    /// schema objects.
    /// </summary>
    public abstract class EntityProducedByFormsApp<TDomainEntity> : EntitySupportingAdditionalProperties<TDomainEntity>
    {
        private readonly IFormDataPrettifier formDataPrettifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityProducedByFormsApp{TDomainEntity}"/> class.
        /// </summary>
        public EntityProducedByFormsApp(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityProducedByFormsApp{TDomainEntity}"/> class.
        /// </summary>
        /// <param name="id">The entity Id.</param>
        /// <param name="createdTicksSinceEpock">The created ticks since epoch.</param>
        /// <param name="lastModifiedTicksSinceEpoch">The last modified ticks since epoch.</param>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        public EntityProducedByFormsApp(
            Guid id,
            long createdTicksSinceEpock,
            long? lastModifiedTicksSinceEpoch,
            IFormDataPrettifier formDataPrettifier,
            IEnumerable<string> includedProperties)
            : base(id, createdTicksSinceEpock, lastModifiedTicksSinceEpoch)
        {
            this.formDataPrettifier = formDataPrettifier;
            this.IncludedProperties = includedProperties;
            this.SupportsFileAttachment = true;
        }

        [JsonConstructor]
        protected EntityProducedByFormsApp()
        {
        }

        /// <summary>
        /// Gets or sets the ids of the documents in the entity.
        /// </summary>
        [JsonProperty(PropertyName = "documentIds", Order = 92)]
        public List<string> DocumentIds { get; set; }

        /// <summary>
        /// Gets or sets the documents in the entity.
        /// </summary>
        [JsonProperty(PropertyName = "documents", Order = 93)]
        public List<Document> Documents { get; set; }

        /// <summary>
        /// Gets or sets form data.
        /// </summary>
        [JsonProperty(PropertyName = "formData", Order = 94)]
        public JObject FormData { get; set; }

        /// <summary>
        /// Gets the flattened and formatted version of the form data.
        /// </summary>
        [JsonProperty(PropertyName = "formDataFormatted", Order = 95)]
        public JObject FormDataFormatted { get; private set; }

        [JsonIgnore]
        public IEnumerable<string> IncludedProperties { get; set; }

        /// <summary>
        /// Sets the form data and formatted form data properties.
        /// </summary>
        /// <param name="latestFormData">the latest form data in string form.</param>
        public void SetFormData(string latestFormData)
        {
            if (this.IncludedProperties != null && this.IncludedProperties.Any(x => x.EqualsIgnoreCase(nameof(this.FormData))))
            {
                this.FormData = this.ParseFormModel(latestFormData);
            }
        }

        public void SetFormDataFormatted(
            string serialisedFormData,
            IFormDataSchema formDataSchema,
            string entityType,
            ProductContext context)
        {
            if (this.IncludedProperties == null || !this.IncludedProperties.Any(x => x.EqualsIgnoreCase(nameof(this.FormDataFormatted))))
            {
                return;
            }

            var formDataObject = this.ParseFormModel(serialisedFormData);
            this.FormDataFormatted = new JObject(formDataObject);
            if (formDataSchema == null)
            {
                return;
            }

            this.formDataPrettifier.Prettify(
                formDataSchema.GetQuestionMetaData(),
                this.FormDataFormatted,
                this.Id,
                entityType,
                context);
        }

        private JObject ParseFormModel(string serialisedFormData)
        {
            return !string.IsNullOrWhiteSpace(serialisedFormData) ?
                JObject.Parse(serialisedFormData).SelectToken("formModel").ToObject<JObject>()
                : JObject.Parse("{}");
        }
    }
}
