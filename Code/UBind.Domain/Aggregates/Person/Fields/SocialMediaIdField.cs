// <copyright file="SocialMediaIdField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person.Fields
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel.Person.Fields;

    /// <summary>
    /// The field for social media Id.
    /// </summary>
    public class SocialMediaIdField : LabelledOrderedField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SocialMediaIdField"/> class.
        /// </summary>
        /// <param name="label">The label of the social field.</param>
        /// <param name="customLabel">the custom label of the social.</param>
        /// <param name="socialMediaId">The social field value.</param>
        public SocialMediaIdField(string label, string customLabel, string socialMediaId)
            : base(label, customLabel)
        {
            this.SocialMediaId = socialMediaId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialMediaIdField"/> class.
        /// </summary>
        /// <param name="socialReadModel">The social read model.</param>
        public SocialMediaIdField(SocialMediaIdReadModel socialReadModel)
             : base(
                  socialReadModel.TenantId,
                  socialReadModel.Id,
                  socialReadModel.Label,
                  socialReadModel.CustomLabel,
                  socialReadModel.SequenceNo,
                  socialReadModel.IsDefault)
        {
            this.Id = socialReadModel.Id;
            this.SocialMediaId = socialReadModel.SocialMediaId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialMediaIdField"/> class.
        /// </summary>
        /// <remarks>
        /// Used for retrieving the aggregate events.
        /// </remarks>
        [JsonConstructor]
        public SocialMediaIdField()
        {
        }

        /// <summary>
        /// Gets or sets the value of the Social media ID field.
        /// </summary>
        [JsonProperty]
        public string SocialMediaId { get; set; }

        [JsonIgnore]
        protected override IEnumerable<string> LabelOptions => new List<string> { "linkedin", "twitter", "facebook", "instagram", "youtube", "other", };
    }
}
