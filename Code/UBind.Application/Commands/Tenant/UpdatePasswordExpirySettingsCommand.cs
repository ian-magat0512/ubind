// <copyright file="UpdatePasswordExpirySettingsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Tenant
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for updating the tenant password expiry settings.
    /// </summary>
    [RetryOnDbException(5)]
    public class UpdatePasswordExpirySettingsCommand : ICommand<Tenant>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePasswordExpirySettingsCommand"/> class.
        /// </summary>
        public UpdatePasswordExpirySettingsCommand(
            Guid tenantId,
            bool passwordExpiryEnabled,
            decimal passwordExpiryDurationInMilliseconds)
        {
            this.TenantId = tenantId;
            this.PasswordExpiryEnabled = passwordExpiryEnabled;
            this.MaxPasswordAgeDays = passwordExpiryDurationInMilliseconds;
        }

        /// <summary>
        /// Gets the ID of the tenant.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets a value indicating whether gets the password expiry enabled config of the tenant.
        /// </summary>
        public bool PasswordExpiryEnabled { get; }

        /// <summary>
        /// Gets the max password age days.
        /// </summary>
        public decimal MaxPasswordAgeDays { get; }
    }
}
