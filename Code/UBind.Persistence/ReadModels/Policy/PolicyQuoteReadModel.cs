﻿// <copyright file="PolicyQuoteReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// For queries joining policy and quote read models.
    /// </summary>
    internal class PolicyQuoteReadModel
    {
        /// <summary>
        /// Gets or sets the policy.
        /// </summary>
        public PolicyReadModel Policy { get; set; }

        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        public NewQuoteReadModel Quote { get; set; }
    }
}
