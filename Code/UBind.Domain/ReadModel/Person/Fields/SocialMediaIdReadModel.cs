// <copyright file="SocialMediaIdReadModel.cs" company="uBind">
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
    /// Read model for social.
    /// </summary>
    public class SocialMediaIdReadModel : LabelledOrderedField, IReadModel<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SocialMediaIdReadModel"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="socialMediaIdField">The social field.</param>
        public SocialMediaIdReadModel(Guid tenantId, SocialMediaIdField socialMediaIdField)
            : base(
                  tenantId,
                  Guid.NewGuid(),
                  socialMediaIdField.Label,
                  socialMediaIdField.CustomLabel,
                  socialMediaIdField.SequenceNo,
                  socialMediaIdField.IsDefault)
        {
            this.SocialMediaId = socialMediaIdField.SocialMediaId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialMediaIdReadModel"/> class.
        /// Parameterless Constructor for WebAddressReadModel.
        /// </summary>
        protected SocialMediaIdReadModel()
        {
        }

        /// <summary>
        /// Gets or sets the social.
        /// </summary>
        [JsonProperty("socialMediaId")]
        public string SocialMediaId { get; set; }

        [JsonIgnore]
        protected override IEnumerable<string> LabelOptions => new List<string> { "linkedin", "twitter", "facebook", "instagram", "youtube", "other", };
    }
}
