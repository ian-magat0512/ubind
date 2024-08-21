// <copyright file="UpdateRefundSettingsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System;
    using MediatR;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for updating refund policy.
    /// </summary>
    public class UpdateRefundSettingsCommand : ICommand<Unit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateProductFeatureRenewalSettingCommand"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productId">The product Id.</param>
        /// <param name="refundPolicy">The refund policy.</param>
        /// <param name="policyPeriodWithNoClaimsRaised">The policy period with no claims raise.</param>
        /// <param name="lastNumberOfYearsWhichNoClaimsMade">The last number of years, which no claims made.</param>
        public UpdateRefundSettingsCommand(
            Guid tenantId,
            Guid productId,
            RefundRule refundPolicy,
            PolicyPeriodCategory? policyPeriodWithNoClaimsRaised,
            int? lastNumberOfYearsWhichNoClaimsMade)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.RefundPolicy = refundPolicy;
            this.PolicyPeriodWithNoClaimsRaised = policyPeriodWithNoClaimsRaised;
            this.LastNumberOfYears = lastNumberOfYearsWhichNoClaimsMade;
        }

        /// <summary>
        /// Gets the tenant Id.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the product Id.
        /// </summary>
        public Guid ProductId { get; }

        /// <summary>
        /// Gets a refund policy.
        /// </summary>
        public RefundRule RefundPolicy { get; private set; }

        /// <summary>
        /// Gets a policy period with no claims raise.
        /// </summary>
        public PolicyPeriodCategory? PolicyPeriodWithNoClaimsRaised { get; private set; }

        /// <summary>
        /// Gets the last number of years, which no claims made.
        /// </summary>
        public int? LastNumberOfYears { get; private set; }
    }
}
