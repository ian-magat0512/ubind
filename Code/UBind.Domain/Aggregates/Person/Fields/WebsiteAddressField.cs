// <copyright file="WebsiteAddressField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person.Fields
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel.Person.Fields;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// The field for website address.
    /// </summary>
    public class WebsiteAddressField : LabelledOrderedField
    {
        private WebAddress webAddressValueObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteAddressField"/> class.
        /// </summary>
        /// <param name="label">The label of the website.</param>
        /// <param name="customLabel">the custom label of the website.</param>
        /// <param name="webAddress">The website address.</param>
        public WebsiteAddressField(string label, string customLabel, WebAddress webAddress)
            : base(label, customLabel)
        {
            this.webAddressValueObject = webAddress;
            this.WebsiteAddress = webAddress.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteAddressField"/> class.
        /// </summary>
        /// <param name="webAddressReadModel">The phone number read model.</param>
        public WebsiteAddressField(WebsiteAddressReadModel webAddressReadModel)
            : base(
                  webAddressReadModel.TenantId,
                  webAddressReadModel.Id,
                  webAddressReadModel.Label,
                  webAddressReadModel.CustomLabel,
                  webAddressReadModel.SequenceNo,
                  webAddressReadModel.IsDefault)
        {
            this.webAddressValueObject = new WebAddress(webAddressReadModel.WebsiteAddress);
            this.WebsiteAddress = this.webAddressValueObject.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteAddressField"/> class.
        /// </summary>
        /// <remarks>
        /// Used for retrieving the aggregate events.
        /// </remarks>
        [JsonConstructor]
        public WebsiteAddressField()
        {
        }

        /// <summary>
        /// Gets or sets the value of the website address.
        ///  this receives the value from the frontend.
        /// </summary>
        [JsonProperty]
        public string WebsiteAddress { get; set; }

        /// <summary>
        /// Gets the value of the web address field value object.
        /// </summary>
        [JsonIgnore]
        public WebAddress WebsiteAddressValueObject
        {
            get
            {
                if (this.webAddressValueObject != null)
                {
                    this.WebsiteAddress = this.webAddressValueObject.ToString();
                }
                else
                {
                    this.webAddressValueObject
                        = this.WebsiteAddress != null ? new WebAddress(this.WebsiteAddress) : null;
                }

                return this.webAddressValueObject;
            }
        }

        [JsonIgnore]
        protected override IEnumerable<string> LabelOptions => new List<string> { "personal", "business", "other", };
    }
}
