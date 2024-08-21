// <copyright file="IntegrationConfigurationException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    /// <summary>
    /// Expception thrown when the integration configuration contains an error.
    /// </summary>
    [System.Serializable]
    public class IntegrationConfigurationException : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationConfigurationException"/> class.
        /// </summary>
        public IntegrationConfigurationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationConfigurationException"/> class with a specified
        /// error.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public IntegrationConfigurationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationConfigurationException"/> class with a specified
        /// error message and a reference to the inner exception that is the cause of this exception..
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception.</param>
        public IntegrationConfigurationException(string message, System.Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationConfigurationException"/> class with serialized
        /// data.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual
        /// information about the source or destination.</param>
        protected IntegrationConfigurationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
