// <copyright file="ConditionToTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    public class ConditionToTextProvider : IProvider<Data<string>>
    {
        private IProvider<Data<bool>> conditionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionToTextProvider"/> class.
        /// </summary>
        /// <param name="conditionProvider">The condition to be parsed.</param>
        public ConditionToTextProvider(IProvider<Data<bool>> conditionProvider)
        {
            this.conditionProvider = conditionProvider;
        }

        public string SchemaReferenceKey => "conditionToText";

        public async ITask<IProviderResult<Data<string>>> Resolve(IProviderContext providerContext)
        {
            var resolveCondition = (await this.conditionProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var condition = resolveCondition.GetValueFromGeneric();
            if (condition != null)
            {
                return ProviderResult<Data<string>>.Success(new Data<string>(condition.ToString().ToLower()));
            }

            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            errorData.Add(ErrorDataKey.ValueToParse, condition.Truncate(80, "..."));
            throw new ErrorException(Errors.Automation.ValueResolutionError(nameof(ConditionToTextProvider), errorData));
        }
    }
}
