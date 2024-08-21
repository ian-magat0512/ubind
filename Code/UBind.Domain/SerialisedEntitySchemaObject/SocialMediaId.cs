// <copyright file="SocialMediaId.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel.Person.Fields;

    /// <summary>
    /// This class is needed because we need to generate json representation of a social media Id object that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class SocialMediaId : OrderedField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SocialMediaId"/> class.
        /// </summary>
        /// <param name="readModel">The social media Id read model.</param>
        public SocialMediaId(SocialMediaIdReadModel readModel)
        {
            this.SocialMediaIdentifier = readModel.SocialMediaId;
            this.IsDefault = readModel.IsDefault;
            this.SetLabel(readModel.Label, readModel.CustomLabel);
        }

        public SocialMediaId(SocialMediaId socialMediaId)
        {
            this.SocialMediaIdentifier = socialMediaId.SocialMediaIdentifier;
            this.IsDefault = socialMediaId.IsDefault;
            this.Label = socialMediaId.Label;
        }

        /// <summary>
        /// Gets or sets the social media Id.
        /// </summary>
        [JsonProperty("socialMediaId", Order = 1)]
        public string SocialMediaIdentifier { get; set; }
    }
}
