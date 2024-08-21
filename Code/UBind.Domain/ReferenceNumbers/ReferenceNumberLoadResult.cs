// <copyright file="ReferenceNumberLoadResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// The result of load a reference number (policy number, invoice number etc.
    /// </summary>
    public struct ReferenceNumberLoadResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceNumberLoadResult"/> struct.
        /// </summary>
        /// <param name="addedReferenceNumbers">The reference number added.</param>
        /// <param name="duplicateReferenceNumbers">The duplicate reference numbers.</param>
        public ReferenceNumberLoadResult(List<string> addedReferenceNumbers, List<string> duplicateReferenceNumbers)
        {
            this.AddedReferenceNumbers = addedReferenceNumbers;
            this.DuplicateReferenceNumbers = duplicateReferenceNumbers;
        }

        /// <summary>
        /// Gets the reference number added.
        /// </summary>
        public List<string> AddedReferenceNumbers { get; }

        /// <summary>
        /// Gets the duplicate reference numbers.
        /// </summary>
        public List<string> DuplicateReferenceNumbers { get; }
    }
}
