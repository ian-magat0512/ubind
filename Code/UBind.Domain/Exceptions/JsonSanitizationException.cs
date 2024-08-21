// <copyright file="JsonSanitizationException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Thrown when there is an error sanitizing json.
    /// </summary>
    [Serializable]
    public class JsonSanitizationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSanitizationException"/> class.
        /// </summary>
        /// <param name="originalJson">The original json to be sanitized.</param>
        /// <param name="partiallySanitizedJson">The result of sanitizing the json.</param>
        /// <param name="error">The first error that still exists in the partially sanitized json.</param>
        /// <param name="innerException">The inner exception that revealed the error.</param>
        public JsonSanitizationException(string originalJson, string partiallySanitizedJson, string error, Exception innerException)
            : base(
                  $@"Cannot fully sanitize json.
Original:
{originalJson}

Partially sanitized:
{partiallySanitizedJson}

Error:
{innerException.Message}",
                  innerException)
        {
            this.OriginalJson = originalJson;
            this.PartiallySanitizedJson = partiallySanitizedJson;
            this.Error = error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSanitizationException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object
        /// data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual
        /// information about the source or destination.</param>
        protected JsonSanitizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.OriginalJson = info.GetString(nameof(this.OriginalJson));
            this.PartiallySanitizedJson = info.GetString(nameof(this.PartiallySanitizedJson));
            this.Error = info.GetString(nameof(this.Error));
        }

        /// <summary>
        /// Gets the original json that was to be sanitized.
        /// </summary>
        public string OriginalJson { get; private set; }

        /// <summary>
        /// Gets the partially sanitized json, that was the best the sanitizer could do.
        /// </summary>
        public string PartiallySanitizedJson { get; private set; }

        /// <summary>
        /// Gets the first error that still exists in the partially sanitized json.
        /// </summary>
        public string Error { get; private set; }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue(nameof(this.OriginalJson), this.OriginalJson);
            info.AddValue(nameof(this.PartiallySanitizedJson), this.PartiallySanitizedJson);
            info.AddValue(nameof(this.Error), this.Error);

            base.GetObjectData(info, context);
        }
    }
}
