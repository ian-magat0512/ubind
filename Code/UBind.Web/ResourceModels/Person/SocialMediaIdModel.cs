// <copyright file="SocialMediaIdModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Person
{
    using UBind.Domain.Aggregates.Person.Fields;

    /// <summary>
    /// Resource model for person's social media ID.
    /// </summary>
    public class SocialMediaIdModel : FieldResourceModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SocialMediaIdModel"/> class.
        /// </summary>
        /// <param name="socialMediaIdField">The social media Id field.</param>
        public SocialMediaIdModel(SocialMediaIdField socialMediaIdField)
            : base(socialMediaIdField.Id, socialMediaIdField.Label, socialMediaIdField.CustomLabel)
        {
            this.SequenceNo = socialMediaIdField.SequenceNo;
            this.IsDefault = socialMediaIdField.IsDefault;
            this.SocialMediaId = socialMediaIdField.SocialMediaId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialMediaIdModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for social media Id model.
        /// </remarks>
        protected SocialMediaIdModel()
        {
        }

        /// <summary>
        /// Gets or sets the social.
        /// </summary>
        public string SocialMediaId { get; set; }
    }
}
