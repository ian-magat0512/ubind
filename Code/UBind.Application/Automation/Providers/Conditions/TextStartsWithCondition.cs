// <copyright file="TextStartsWithCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System;
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;

    /// <summary>
    /// Returns a boolean true if the text value starts with the second text value.
    /// </summary>
    public class TextStartsWithCondition : IProvider<Data<bool>>
    {
        private readonly IProvider<Data<string>> textProvider;
        private readonly IProvider<Data<string>> startsWithProvider;
        private readonly IProvider<Data<bool>> ignoreCaseProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextStartsWithCondition"/> class.
        /// </summary>
        /// <param name="textProvider">The text value that will be tested.</param>
        /// <param name="startsWithProvider">The text value that will be used for testing.</param>
        /// <param name="ignoreCaseProvider">The boolean value for ignoring the case sensitive.</param>
        public TextStartsWithCondition(
            IProvider<Data<string>> textProvider,
            IProvider<Data<string>> startsWithProvider,
            IProvider<Data<bool>> ignoreCaseProvider)
        {
            this.textProvider = textProvider;
            this.startsWithProvider = startsWithProvider;
            this.ignoreCaseProvider = ignoreCaseProvider;
        }

        public string SchemaReferenceKey => "textStartsWithCondition";

        public async ITask<IProviderResult<Data<bool>>> Resolve(IProviderContext providerContext)
        {
            var text = (await this.textProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var startsWith = (await this.startsWithProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var ignoreCase = (await this.ignoreCaseProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (ignoreCase != null)
            {
                return ProviderResult<Data<bool>>.Success(
                    text.StartsWith(startsWith, StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                return ProviderResult<Data<bool>>.Success(text.StartsWith(startsWith));
            }
        }
    }
}
