// <copyright file="ObjectContainsPropertyCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers.Object;

    /// <summary>
    /// Returns a boolean true if the data object has a property with a name that matches the property name value.
    /// </summary>
    public class ObjectContainsPropertyCondition : IProvider<Data<bool>>
    {
        private readonly IObjectProvider dataObjectProvider;
        private readonly IProvider<Data<string>> propertyNameTextProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectContainsPropertyCondition"/> class.
        /// </summary>
        /// <param name="dataObjectProvider">The data object that will be searched.</param>
        /// <param name="propertyNameTextProvider">The property name that will be searched for.</param>
        public ObjectContainsPropertyCondition(IObjectProvider dataObjectProvider, IProvider<Data<string>> propertyNameTextProvider)
        {
            this.dataObjectProvider = dataObjectProvider;
            this.propertyNameTextProvider = propertyNameTextProvider;
        }

        public string SchemaReferenceKey => "objectContainsPropertyCondition";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<bool>>> Resolve(IProviderContext providerContext)
        {
            var dataObject = (await this.dataObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            string propertyName = (await this.propertyNameTextProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            return ProviderResult<Data<bool>>.Success(DataObjectHelper.TryGetPropertyValue(dataObject, propertyName, out _));
        }
    }
}
