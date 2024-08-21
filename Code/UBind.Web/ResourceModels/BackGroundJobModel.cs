// <copyright file="BackGroundJobModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    /// <summary>
    /// Resource Model for acknowledging hangfire job.
    /// </summary>b
    public class BackGroundJobModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackGroundJobModel"/> class.
        /// </summary>
        /// <param name="backgroundJobId">The BackgroundJobId.</param>
        /// <param name="ticketId">The ticket id.</param>
        /// <param name="acknowledgementMessage">The acknowledgement message.</param>
        public BackGroundJobModel(string backgroundJobId, string ticketId, string acknowledgementMessage)
        {
            this.BackgroundJobId = backgroundJobId;
            this.TicketId = ticketId;
            this.AcknowledgementMessage = acknowledgementMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackGroundJobModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for JSON deserializer.
        /// </remarks>
        [JsonConstructor]
        public BackGroundJobModel()
        {
        }

        /// <summary>
        /// Gets or sets Hangfire job Id.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonProperty("BackgroundJobId", Required = Newtonsoft.Json.Required.Always)]
        public string BackgroundJobId { get; set; }

        /// <summary>
        /// Gets or sets Ticket Id.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonProperty]
        public string TicketId { get; set; }

        /// <summary>
        /// Gets or sets the Acknowledgement Message.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonProperty]
        public string AcknowledgementMessage { get; set; }
    }
}
