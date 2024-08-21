// <copyright file="NumberPoolAddResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// The result of load a number pool number (policy number, invoice number etc.
    /// </summary>
    public struct NumberPoolAddResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberPoolAddResult"/> struct.
        /// </summary>
        /// <param name="addedNumbers">The numbers added.</param>
        /// <param name="duplicateNumbers">The duplicate numbers.</param>
        public NumberPoolAddResult(IEnumerable<string> addedNumbers, IEnumerable<string> duplicateNumbers)
        {
            this.AddedNumbers = addedNumbers;
            this.DuplicateNumbers = duplicateNumbers;
        }

        /// <summary>
        /// Gets the reference number added.
        /// </summary>
        public IEnumerable<string> AddedNumbers { get; }

        /// <summary>
        /// Gets the duplicate reference numbers.
        /// </summary>
        public IEnumerable<string> DuplicateNumbers { get; }
    }
}
