// <copyright file="BackgroundJobParameter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// The Hangfire Job Parameters.
    /// </summary>
    public static class BackgroundJobParameter
    {
        /// <summary>
        /// The background job Id.
        /// </summary>
        public const string BackgroundJobId = "hangFireJobId";

        /// <summary>
        /// The Hangfire message.
        /// </summary>
        public const string HangFireMessage = "hangFireMessage";

        /// <summary>
        /// Indicator whether the hangfire job is acknowledged or not.
        /// </summary>
        public const string IsAcknowledged = "isAcknowledged";

        /// <summary>
        /// The user Id.
        /// </summary>
        public const string UserId = "userId";

        /// <summary>
        /// The Ticket Id.
        /// </summary>
        public const string TicketId = "ticketId";

        /// <summary>
        /// The Acknowledgment Message.
        /// </summary>
        public const string AcknowledgmentMessage = "acknowledgmentMessage";

        /// <summary>
        /// The name of the user who acknowledge the hangfire job.
        /// </summary>
        public const string AcknowledgeBy = "acknowledgeBy";

        /// <summary>
        /// The environment.
        /// </summary>
        public const string Environment = "environment";

        /// <summary>
        /// The created timestamp.
        /// </summary>
        public const string CreatedTimestamp = "createdTimestamp";

        /// <summary>
        /// The acknowledged timestamp.
        /// </summary>
        public const string AcknowledgedTimestamp = "acknowledgedTimestamp";

        /// <summary>
        /// The tenant Id.
        /// </summary>
        public const string Tenant = "tenant";

        /// <summary>
        /// The product alias.
        /// </summary>
        public const string Product = "product";

        /// <summary>
        /// The expiry timestamp
        /// </summary>
        public const string ExpireAt = "ExpireAt";

        /// <summary>
        /// The organisation alias.
        /// </summary>
        public const string Organisation = "organisation";

        /// <summary>
        /// The notified flag.
        /// </summary>
        public const string Notified = "notified";

        /// <summary>
        /// The notified timestamp.
        /// </summary>
        public const string NotifiedTimestamp = "notifiedTimestamp";
    }
}
