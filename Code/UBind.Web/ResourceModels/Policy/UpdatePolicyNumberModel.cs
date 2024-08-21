// <copyright file="UpdatePolicyNumberModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Policy
{
    /// <summary>
    /// Resource model for updating a policy number.
    /// </summary>
    public class UpdatePolicyNumberModel
    {
        public UpdatePolicyNumberModel(Guid policyId, string policyNumber, bool returnOldPolicyNumberToPool)
        {
            this.PolicyId = policyId;
            this.PolicyNumber = policyNumber;
            this.ReturnOldPolicyNumberToPool = returnOldPolicyNumberToPool;
        }

        /// <summary>
        /// Gets or sets the policy id of the policy.
        /// </summary>
        public Guid PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the policy number of the policy.
        /// </summary>
        public string PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether old policy numbers can be used or not.
        /// </summary>
        public bool ReturnOldPolicyNumberToPool { get; set; }
    }
}
