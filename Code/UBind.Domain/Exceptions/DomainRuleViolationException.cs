// <copyright file="DomainRuleViolationException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Exceptions
{
    using System;

    /// <summary>
    /// Exception thrown when a command violates domain logic.
    /// </summary>
    [Serializable]
    public class DomainRuleViolationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainRuleViolationException"/> class.
        /// </summary>
        /// <param name="message">A message describing the violation.</param>
        public DomainRuleViolationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainRuleViolationException"/> class.
        /// </summary>
        /// <param name="message">A meesage describing the violation.</param>
        /// <param name="inner">The underlying exception that triggers this exception.</param>
        public DomainRuleViolationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainRuleViolationException"/> class.
        /// </summary>
        /// <param name="info">Serialized information about the exception.</param>
        /// <param name="context">Contextual information about the source.</param>
        protected DomainRuleViolationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
