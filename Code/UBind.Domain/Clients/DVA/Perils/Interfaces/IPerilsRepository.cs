// <copyright file="IPerilsRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Clients.DVA.Perils.Interfaces
{
    using UBind.Domain.Clients.DVA.Perils.Entities;

    /// <summary>
    /// Repository for storing Perils records.
    /// </summary>
    public interface IPerilsRepository
    {
        /// <summary>
        /// Gets the latest and most up to date Peril details for a given Gnaf PID.
        /// Perils are stored by effective date, so this will return the latest record for a given GNAF property id.
        /// </summary>
        /// <param name="propertyId">The G-NAF property Id.</param>
        /// <returns>A G-NAF record details.</returns>
        Peril GetDetailsByPropertyId(string propertyId);

        /// <summary>
        /// Retrieve the applicable Peril details by GNAF Property ID for a policy start date.
        /// </summary>
        /// <param name="propertyId">The GNAF property Id.</param>
        /// <param name="policyStartDate">The policy start date to base the effectivity date on.</param>
        /// <returns>The Perils record details.</returns>
        Peril GetDetailsByPropertyIdForPolicyStartDate(string propertyId, string policyStartDate);
    }
}
