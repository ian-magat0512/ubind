// <copyright file="EmailPolicyViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using DotLiquid;
    using UBind.Domain.ReadModel.Email;

    /// <summary>
    /// Email's policy view mode for liquid template.
    /// </summary>
    public class EmailPolicyViewModel : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailPolicyViewModel"/> class.
        /// </summary>
        /// <param name="policy">The plicy data.</param>
        public EmailPolicyViewModel(PolicyData policy)
        {
            this.PolicyNumber = policy.PolicyNumber;
        }

        /// <summary>
        /// Gets the policy number.
        /// </summary>
        public string PolicyNumber { get; }
    }
}
