// <copyright file="DkimTestEmailModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is used as a resource model for sending DKIM test email.
    /// </summary>
    public class DkimTestEmailModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DkimTestEmailModel"/> class.
        /// </summary>
        /// <param name="dkimSettingsId">The DKIM settings Id.</param>
        /// <param name="organisationId">The organisationId.</param>
        /// <param name="recipientEmailAddress">The recipient email address.</param>
        /// <param name="senderEmailAddress">The sender email Address.</param>
        public DkimTestEmailModel(
            Guid dkimSettingsId,
            Guid organisationId,
            string recipientEmailAddress,
            string senderEmailAddress)
        {
            this.DkimSettingsId = dkimSettingsId;
            this.OrganisationId = organisationId;
            this.RecipientEmailAddress = recipientEmailAddress;
            this.SenderEmailAddress = senderEmailAddress;
        }

        /// <summary>
        /// Gets the tenant ID or alias.
        /// </summary>
        [JsonProperty]
        public string Tenant { get; private set; }

        /// <summary>
        /// Gets the DKIM settings Id.
        /// </summary>
        [JsonProperty]
        public Guid DkimSettingsId { get; private set; }

        /// <summary>
        /// Gets the organisation Id.
        /// </summary>
        [JsonProperty]
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the recipient email address.
        /// </summary>
        [JsonProperty]
        public string RecipientEmailAddress { get; private set; }

        /// <summary>
        /// Gets the sender email address.
        /// </summary>
        [JsonProperty]
        public string SenderEmailAddress { get; private set; }
    }
}
