// <copyright file="NotCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;

    /// <summary>
    /// Returns the inverse boolean value of the input condition.
    /// </summary>
    public class NotCondition : IProvider<Data<bool>>
    {
        private readonly IProvider<Data<bool>> condition;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotCondition"/> class.
        /// </summary>
        /// <param name="condition">A condition clauses that must be evauluated.</param>
        public NotCondition(IProvider<Data<bool>> condition)
        {
            this.condition = condition;
        }

        public string SchemaReferenceKey => "notCondition";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<bool>>> Resolve(IProviderContext providerContext)
        {
            var result = (await this.condition.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            return ProviderResult<Data<bool>>.Success(!result);
        }
    }
}
