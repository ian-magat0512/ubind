// <copyright file="LuceneIndexingException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Search
{
    using System;

    /// <summary>
    /// Exception to throw when an some error has occured duting creation/updates on Lucene Indexes.
    /// </summary>
    [Serializable]
    public class LuceneIndexingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuceneIndexingException"/> class.
        /// </summary>
        /// <param name="message">A message describing the reason for the exception.</param>
        public LuceneIndexingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuceneIndexingException"/> class.
        /// </summary>
        /// <param name="message">A message describing the reason for the exception.</param>
        /// <param name="inner">The exception that triggered this exception.</param>
        public LuceneIndexingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuceneIndexingException"/> class.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">The serialization context.</param>
        protected LuceneIndexingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
