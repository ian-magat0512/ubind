// <copyright file="IPatchableDataHolder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Entities
{
    using System.Collections.Generic;
    using CSharpFunctionalExtensions;
    using UBind.Domain.Aggregates.Quote.Commands;

    /// <summary>
    /// Interface for entities that hold form data or calculation result data.
    /// </summary>
    public interface IPatchableDataHolder
    {
        /// <summary>
        /// Patch form data.
        /// </summary>
        /// <param name="patch">The patch to apply.</param>
        void ApplyPatch(PolicyDataPatch patch);

        /// <summary>
        /// Select the individual form data sets that are to be patched within the form data holder.
        /// </summary>
        /// <param name="command">The patch command.</param>
        /// <returns>A result with the targets to be patched, or the error if targets could not be found, or failed validation against the conditions.</returns>
        Result<IEnumerable<DataPatchTargetEntity>> SelectAndValidateFormDataPatchTargets(PolicyDataPatchCommand command);

        /// <summary>
        /// Select the individual form calculation results that are to be patched within the data holder.
        /// </summary>
        /// <param name="command">The patch command.</param>
        /// <returns>A result with the targets to be patched, or the error if targets could not be found, or failed validation against the conditions.</returns>
        Result<IEnumerable<DataPatchTargetEntity>> SelectAndValidateCalculationResultPatchTargets(PolicyDataPatchCommand command);
    }
}
