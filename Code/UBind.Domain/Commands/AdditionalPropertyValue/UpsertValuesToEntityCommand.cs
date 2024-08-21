// <copyright file="UpsertValuesToEntityCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Commands.AdditionalPropertyValue
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Notification model in persisting set values for entity.
    /// </summary>
    public class UpsertValuesToEntityCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpsertValuesToEntityCommand"/> class.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="entityId">The primary id of entity.</param>
        /// <param name="valueIdModels">Collection of <see cref="AdditionalPropertyValueUpsertModel"/>.
        /// </param>
        public UpsertValuesToEntityCommand(
            Guid tenantId, Guid entityId, List<AdditionalPropertyValueUpsertModel> valueIdModels)
        {
            this.ValueIdTypeModels = valueIdModels;
            this.EntityId = entityId;
            this.TenantId = tenantId;
        }

        /// <summary>
        /// Gets the value of id value models.
        /// </summary>
        public List<AdditionalPropertyValueUpsertModel> ValueIdTypeModels { get; }

        /// <summary>
        /// Gets the value of entity id.
        /// </summary>
        public Guid EntityId { get; }

        /// <summary>
        /// Gets the tenant's Id.
        /// </summary>
        public Guid TenantId { get; }
    }
}
