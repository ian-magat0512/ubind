// <copyright file="SessionTimeoutException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph
{
    using System;
    using System.Runtime.Serialization;
    using NodaTime;

    /// <summary>
    /// Exception for MS Graph Excel session timeouts.
    /// </summary>
    [Serializable]
    public class SessionTimeoutException : Exception
    {
        private const string SessionIdKey = "SessionId";
        private const string TimeoutTimeKey = "TimeoutTime";
        private const string LastActivityKey = "LastActivity";

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionTimeoutException"/> class.
        /// </summary>
        /// <param name="sessionId">The ID of the session that timed out.</param>
        /// <param name="timeoutTime">The time of the timeout.</param>
        /// <param name="lastActivity">The time the session was last active.</param>
        /// <param name="innerException">The exception that caused this exception.</param>
        public SessionTimeoutException(string sessionId, Instant timeoutTime, Instant lastActivity, Exception innerException)
            : base(
                  $"Session {sessionId} timed out after {(timeoutTime - lastActivity).TotalSeconds} seconds of inactivity.",
                  innerException)
        {
            this.SessionId = sessionId;
            this.TimeoutTimeInUnixTimeSeconds = timeoutTime.ToUnixTimeSeconds();
            this.LastActivityTimeInUnixTimeSeconds = lastActivity.ToUnixTimeSeconds();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionTimeoutException"/> class.
        /// </summary>
        /// <param name="info">Serialized object data.</param>
        /// <param name="context">Contextual information about source or destination.</param>
        protected SessionTimeoutException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context)
        {
            this.SessionId = info.GetString(SessionIdKey);
            this.TimeoutTimeInUnixTimeSeconds = info.GetInt64(TimeoutTimeKey);
            this.LastActivityTimeInUnixTimeSeconds = info.GetInt64(LastActivityKey);
        }

        /// <summary>
        /// Gets the ID of the session that timed out.
        /// </summary>
        public string SessionId { get; }

        /// <summary>
        /// Gets the time of the timeout in Unix Time (seconds).
        /// </summary>
        public long TimeoutTimeInUnixTimeSeconds { get; }

        /// <summary>
        /// Gets the time the session was last active in Unix Time (seconds).
        /// </summary>
        public long LastActivityTimeInUnixTimeSeconds { get; }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue(SessionIdKey, this.SessionId);
            info.AddValue(TimeoutTimeKey, this.TimeoutTimeInUnixTimeSeconds);
            info.AddValue(LastActivityKey, this.LastActivityTimeInUnixTimeSeconds);
            base.GetObjectData(info, context);
        }
    }
}
