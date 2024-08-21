// <copyright file="IPolicyLatestRenewalEffectiveTimeMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.Migration
{
    public interface IPolicyLatestRenewalEffectiveTimeMigration
    {
        /// <summary>
        /// This is to process the udating of policy renewal effective date
        /// where the latest renewal effective date is null.
        /// Getting the latest effective date of the renewal transaction on policy transaction table.
        /// </summary>
        void ProcessUpdatingPolicyRenewalEffectiveDate();
    }
}
