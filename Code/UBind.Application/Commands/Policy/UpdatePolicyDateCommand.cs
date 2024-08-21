// <copyright file="UpdatePolicyDateCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Policy
{
    using System;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    public class UpdatePolicyDateCommand : ICommand<LocalDateTime>
    {
        public UpdatePolicyDateCommand(
            Guid tenantId,
            Guid policyId,
            Guid productReleaseId,
            Guid performingUserId,
            PolicyDateType dateType,
            LocalDate date,
            LocalTime? time)
        {
            this.TenantId = tenantId;
            this.PolicyId = policyId;
            this.ProductReleaseId = productReleaseId;
            this.PerformingUserId = performingUserId;
            this.DateType = dateType;
            this.Date = date;
            this.Time = time;
        }

        public Guid TenantId { get; }

        public Guid PolicyId { get; }

        public Guid ProductReleaseId { get; }

        public Guid PerformingUserId { get; }

        public PolicyDateType DateType { get; }

        public LocalDate Date { get; }

        public LocalTime? Time { get; }
    }
}
