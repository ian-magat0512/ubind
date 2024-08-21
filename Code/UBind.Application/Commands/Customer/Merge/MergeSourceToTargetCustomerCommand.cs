// <copyright file="MergeSourceToTargetCustomerCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer.Merge
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for merging a source customer records to a target customer,
    /// deleting the source customer in the process.
    /// </summary>
    public class MergeSourceToTargetCustomerCommand : ICommand
    {
        public MergeSourceToTargetCustomerCommand(
            Guid tenantId,
            Guid sourceCustomerId,
            Guid targetCustomerId,
            Guid? performingUserId = null)
        {
            this.TenantId = tenantId;
            this.SourceCustomerId = sourceCustomerId;
            this.TargetCustomerId = targetCustomerId;
            this.PerformingUserId = performingUserId;
        }

        /// <summary>
        /// The tenant Id where both of the customer came from.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// The from or source customer Id of the merge.
        /// </summary>
        public Guid SourceCustomerId { get; }

        /// <summary>
        /// The to or target or destination customer Id of the merge.
        /// </summary>
        public Guid TargetCustomerId { get; }

        /// <summary>
        /// The performing user id where the authorisation will be based of. ( optional )
        /// </summary>
        public Guid? PerformingUserId { get; }

        /// <summary>
        /// The source customer display name, will be set later on execution of the command.
        /// </summary>
        public string SourceCustomerDisplayName { get; set; }

        /// <summary>
        /// The target customer display name, will be set later on execution of the command.
        /// </summary>
        public string TargetCustomerDisplayName { get; set; }
    }
}
