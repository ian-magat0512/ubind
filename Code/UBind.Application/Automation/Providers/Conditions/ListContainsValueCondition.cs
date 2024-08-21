// <copyright file="ListContainsValueCondition.cs" company="uBind">
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
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers.List;

    /// <summary>
    /// Returns a boolean true if the list contains the value provided.
    /// </summary>
    public class ListContainsValueCondition : IProvider<Data<bool>>
    {
        private readonly IDataListProvider<object> listCollection;
        private readonly IProvider<IData> valueNameProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListContainsValueCondition"/> class.
        /// </summary>
        /// <param name="dataListProvider">A collection of object.</param>
        /// <param name="valueNameProvider">The value need to check in the list.</param>
        public ListContainsValueCondition(IDataListProvider<object> dataListProvider, IProvider<IData> valueNameProvider)
        {
            this.listCollection = dataListProvider;
            this.valueNameProvider = valueNameProvider;
        }

        public string SchemaReferenceKey => "listContainsValueCondition";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<bool>>> Resolve(IProviderContext providerContext)
        {
            var list = (await this.listCollection.Resolve(providerContext)).GetValueOrThrowIfFailed() as IEnumerable<object>;
            var valueCheck = (await this.valueNameProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().GetValueFromGeneric();
            return ProviderResult<Data<bool>>.Success(list.Any(c => DataObjectHelper.IsEqual(c, valueCheck)));
        }
    }
}
