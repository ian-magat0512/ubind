// <copyright file="IPostcodeValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person.Fields
{
    using System.Collections.Generic;

    /// <summary>
    /// The interface for validating post codes.
    /// </summary>
    public interface IPostcodeValidator
    {
        /// <summary>
        /// Check if its a valid state.
        /// </summary>
        bool IsValidState(string state);

        /// <summary>
        /// Get list of valid states.
        /// </summary>
        List<string> GetValidStates();

        /// <summary>
        /// Check if the post code is a valid format.
        /// </summary>
        bool IsValidPostCode(string postCode);

        /// <summary>
        /// Check if the post code is valid for the state.
        /// </summary>
        bool IsValidPostCodeForTheState(string state, string postCode);
    }
}
