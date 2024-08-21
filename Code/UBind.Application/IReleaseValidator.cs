// <copyright file="IReleaseValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using UBind.Domain;

    /// <summary>
    /// Validates a release details, for example by checking that json files validate against their schema.
    /// </summary>
    public interface IReleaseValidator
    {
        /// <summary>
        /// Validates a release details for a given release, for example by checking that json files validate against their schema.
        /// If there is a validation error, an exception is thrown.
        /// </summary>
        /// <param name="quoteDetails">The release details pertaining to the quotes.</param>
        /// <param name="claimDetails">The release details pertaining to the claims.</param>
        void Validate(ReleaseDetails quoteDetails, ReleaseDetails claimDetails);

        public void ValidateQuoteDetails(ReleaseDetails quoteDetails);

        public void ValidateClaimDetails(ReleaseDetails claimDetails);
    }
}
