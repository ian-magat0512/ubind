// <copyright file="GraphException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph.Exceptions
{
    using System;

    /// <summary>
    /// Base class for exceptions thrown during Microsoft Graph API usage.
    /// </summary>
    [Serializable]
    public class GraphException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphException"/> class.
        /// </summary>
        public GraphException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public GraphException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception.</param>
        public GraphException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphException"/> class.
        /// </summary>
        /// <param name="info">Serialized object data about the exception being thrown.</param>
        /// <param name="context">Contextual information about the source or destination.</param>
        protected GraphException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
