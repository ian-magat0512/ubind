// <copyright file="WebsiteAddressReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Person.Fields
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Person.Fields;

    /// <summary>
    /// Read model for web addresses.
    /// </summary>
    public class WebsiteAddressReadModel : LabelledOrderedField, IReadModel<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteAddressReadModel"/> class.
        /// Parameterless Constructor for EmailAddressReadModel.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="websiteAddressField">The website address field.</param>
        public WebsiteAddressReadModel(Guid tenantId, WebsiteAddressField websiteAddressField)
            : base(
                  tenantId,
                  Guid.NewGuid(),
                  websiteAddressField.Label,
                  websiteAddressField.CustomLabel,
                  websiteAddressField.SequenceNo,
                  websiteAddressField.IsDefault)
        {
            this.WebsiteAddress = websiteAddressField.WebsiteAddressValueObject.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteAddressReadModel"/> class.
        /// Parameterless Constructor for WebAddressReadModel.
        /// </summary>
        protected WebsiteAddressReadModel()
        {
        }

        /// <summary>
        /// Gets or sets the web address.
        /// </summary>
        [JsonProperty("websiteAddress")]
        public string WebsiteAddress { get; set; }

        [JsonIgnore]
        protected override IEnumerable<string> LabelOptions => new List<string> { "personal", "business", "other", };
    }
}
