﻿// <copyright file="OrCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System.Collections.Generic;
    using System.Linq;
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Returns a boolean true if any of the conditions in the array are true.
    /// </summary>
    public class OrCondition : IProvider<Data<bool>>
    {
        private readonly IList<IProvider<Data<bool>>> conditions;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrCondition"/> class.
        /// </summary>
        /// <param name="conditions">The conditions to be evaluated.</param>
        public OrCondition(IList<IProvider<Data<bool>>> conditions)
        {
            this.conditions = conditions;
        }

        public string SchemaReferenceKey => "orCondition";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<bool>>> Resolve(IProviderContext providerContext)
        {
            var conditions = await this.conditions.SelectAsync(async c => await c.Resolve(providerContext));
            return ProviderResult<Data<bool>>.Success(conditions.Any(c => c.GetValueOrThrowIfFailed().DataValue));
        }
    }
}
