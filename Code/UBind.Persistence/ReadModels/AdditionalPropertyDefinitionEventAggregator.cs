// <copyright file="AdditionalPropertyDefinitionEventAggregator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using UBind.Domain.Aggregates.AdditionalPropertyDefinition;

    /// <summary>
    /// Addtional property definition event aggregator.
    /// </summary>
    public class AdditionalPropertyDefinitionEventAggregator : EventAggregator<AdditionalPropertyDefinition, Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionEventAggregator"/> class.
        /// </summary>
        /// <param name="additionalPropertyDefinitionReadModelWriter">A read model writer of additional property definition.</param>
        public AdditionalPropertyDefinitionEventAggregator(
            IAdditionalPropertyDefinitionReadModelWriter additionalPropertyDefinitionReadModelWriter)
            : base(additionalPropertyDefinitionReadModelWriter)
        {
        }
    }
}
