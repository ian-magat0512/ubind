// <copyright file="WebsiteAddressModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Person
{
    using UBind.Domain.Aggregates.Person.Fields;

    /// <summary>
    /// Read model for person's website address.
    /// </summary>
    public class WebsiteAddressModel : FieldResourceModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteAddressModel"/> class.
        /// </summary>
        /// <param name="websiteAddressField">The website field.</param>
        public WebsiteAddressModel(WebsiteAddressField websiteAddressField)
            : base(websiteAddressField.Id, websiteAddressField.Label, websiteAddressField.CustomLabel)
        {
            this.SequenceNo = websiteAddressField.SequenceNo;
            this.IsDefault = websiteAddressField.IsDefault;
            this.WebsiteAddress = websiteAddressField.WebsiteAddressValueObject.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteAddressModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// </remarks>
        protected WebsiteAddressModel()
        {
        }

        /// <summary>
        /// Gets or sets the website address.
        /// </summary>
        public string WebsiteAddress { get; set; }
    }
}
