// <copyright file="BackgroundJobDto.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Dto
{
    using Newtonsoft.Json;

    /// <summary>
    /// Data transfer object for hangfire job.
    /// </summary>
    public class BackgroundJobDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundJobDto"/> class.
        /// </summary>
        /// <param name="hangFireJobId">The HangFire Job Id.</param>
        /// <param name="reason">The Hangfire Reason why it fail.</param>
        /// <param name="exceptionType">The exception type.</param>
        /// <param name="exceptionMessage">The exception message.</param>
        /// <param name="exceptionDetails">The exception details.</param>
        /// <param name="isAcknowledged">Indicating whether the hangfire job is acknowledge or not.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="ticketId">The ticket id.</param>
        /// <param name="message">The acknowledgment message.</param>
        /// <param name="acknowledgeBy">The name of the person who acknowledge the hangfire job.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productId">The product Id.</param>
        /// <param name="createdTimestamp">The created timestamp.</param>
        /// <param name="acknowledgedTimestamp">The acknowledge timestamp.</param>
        public BackgroundJobDto(
            string hangFireJobId,
            string reason,
            string exceptionType,
            string exceptionMessage,
            string exceptionDetails,
            bool? isAcknowledged,
            string userId,
            string ticketId,
            string message,
            string acknowledgeBy,
            string environment,
            string tenantId,
            string productId,
            string createdTimestamp,
            string acknowledgedTimestamp,
            string expiryTimestamp)
        {
            this.HangFireJobId = hangFireJobId;
            this.Reason = reason;
            this.ExceptionType = exceptionType;
            this.ExceptionMessage = exceptionMessage;
            this.ExceptionDetails = exceptionDetails;
            this.UserId = userId;
            this.IsAcknowledged = isAcknowledged;
            this.TicketId = ticketId;
            this.AcknowledgementMessage = message;
            this.AcknowledgedBy = acknowledgeBy;
            this.Environment = environment;
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.CreatedTimestamp = createdTimestamp;
            this.AcknowledgedTimestamp = acknowledgedTimestamp;
            this.ExpiryTimestamp = expiryTimestamp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundJobDto"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for JSON deserializer.
        /// .</remarks>
        [JsonConstructor]
        public BackgroundJobDto()
        {
        }

        /// <summary>
        /// Gets or sets The Hangfire Job Id.
        /// </summary>
        [JsonProperty]
        public string HangFireJobId { get; set; }

        /// <summary>
        /// Gets or sets The hangfire reason why it fail.
        /// </summary>
        [JsonProperty]
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets The exception type.
        /// </summary>
        [JsonProperty]
        public string ExceptionType { get; set; }

        /// <summary>
        /// Gets or sets The exception message.
        /// </summary>
        [JsonProperty]
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Gets or sets The exception details.
        /// </summary>
        [JsonProperty]
        public string ExceptionDetails { get; set; }

        /// <summary>
        /// Gets or sets the user Id of the person who acknowledged the job.
        /// </summary>
        [JsonProperty]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the Name of the person who acknowledged the job.
        /// </summary>
        [JsonProperty]
        public string AcknowledgedBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the hangfire job is acknowledged or not.
        /// </summary>
        [JsonProperty]
        public bool? IsAcknowledged { get; set; }

        /// <summary>
        /// Gets or sets Ticket Id.
        /// </summary>
        [JsonProperty]
        public string TicketId { get; set; }

        /// <summary>
        /// Gets or sets the acknowledgment message.
        /// </summary>
        [JsonProperty]
        public string AcknowledgementMessage { get; set; }

        /// <summary>
        /// Gets or sets Environment.
        /// </summary>
        [JsonProperty]
        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets Product ID.
        /// </summary>
        [JsonProperty]
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets Tenant ID.
        /// </summary>
        [JsonProperty]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the created timestamp.
        /// </summary>
        [JsonProperty]
        public string CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the acknowledged timestamp.
        /// </summary>
        [JsonProperty]
        public string AcknowledgedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the expiry timestamp.
        /// </summary>
        [JsonProperty]
        public string ExpiryTimestamp { get; set; }
    }
}
