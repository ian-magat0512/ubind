// <copyright file="XOrCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;

    /// <summary>
    /// Returns a boolean true if one of the input conditions is true and the other is false, otherwise it returns boolean false.
    /// </summary>
    public class XOrCondition : IProvider<Data<bool>>
    {
        private readonly IProvider<Data<bool>> firstCondition;
        private readonly IProvider<Data<bool>> secondCondition;

        /// <summary>
        /// Initializes a new instance of the <see cref="XOrCondition"/> class.
        /// </summary>
        /// <param name="firstCondition">A first condition clauses that must be evauluated.</param>
        /// <param name="secondCondition">A second condition clauses that must be evauluated.</param>
        public XOrCondition(IProvider<Data<bool>> firstCondition, IProvider<Data<bool>> secondCondition)
        {
            this.firstCondition = firstCondition;
            this.secondCondition = secondCondition;
        }

        public string SchemaReferenceKey => "xorCondition";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<bool>>> Resolve(IProviderContext providerContext)
        {
            var first = (await this.firstCondition.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var second = (await this.secondCondition.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            return ProviderResult<Data<bool>>.Success(first ^ second);
        }
    }
}
