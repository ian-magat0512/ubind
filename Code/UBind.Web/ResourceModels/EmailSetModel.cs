// <copyright file="EmailSetModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Resource model for serving the list of emails available for a product.
    /// </summary>
    public class EmailSetModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSetModel"/> class.
        /// </summary>
        /// <param name="model">The email read model.</param>
        public EmailSetModel(IEmailSummary model)
        {
            this.Id = model.Id;
            this.Recipient = model.Recipient;
            this.Subject = model.Subject;
            this.CreatedDateTime = model.CreatedTimestamp.ToExtendedIso8601String();
            this.HasAttachment = model.HasAttachment;
        }

        /// <summary>
        /// Gets the ID of the email.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the email's recipient.
        /// </summary>
        [JsonProperty]
        public string Recipient { get; private set; }

        /// <summary>
        /// Gets the email's subject.
        /// </summary>
        [JsonProperty]
        public string Subject { get; private set; }

        /// <summary>
        /// Gets the time the email was created.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the email has an attachment.
        /// </summary>
        [JsonProperty]
        public bool HasAttachment { get; private set; }
    }
}
