// <copyright file="SmsSetModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Resource model for serving the list of SMS available for a product.
    /// </summary>
    public class SmsSetModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsSetModel"/> class.
        /// </summary>
        /// <param name="model">The SMS read model.</param>
        public SmsSetModel(Sms model)
        {
            this.Id = model.Id;
            this.To = model.To;
            this.From = model.From;
            this.Message = model.Message;
            this.CreatedDateTime = model.CreatedTimestamp.ToExtendedIso8601String();
        }

        /// <summary>
        /// Gets the ID of the SMS.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the SMS recipient.
        /// </summary>
        [JsonProperty]
        public string To { get; private set; }

        /// <summary>
        /// Gets the SMS sender.
        /// </summary>
        [JsonProperty]
        public string From { get; private set; }

        /// <summary>
        /// Gets the SMS message.
        /// </summary>
        [JsonProperty]
        public string Message { get; private set; }

        /// <summary>
        /// Gets the date and time the SMS was created.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; private set; }
    }
}
