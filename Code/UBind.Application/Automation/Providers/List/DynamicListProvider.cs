// <copyright file="DynamicListProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System.Collections.Generic;
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.Object;

    /// <summary>
    /// Provider for resolving dynamic object lists.
    /// </summary>
    public class DynamicListProvider : IDataListProvider<object>
    {
        private readonly IDataList<object> list;

        public DynamicListProvider(IDataList<object> listValue)
        {
            this.list = listValue;
        }

        public List<string> IncludedProperties { get; set; }

        public string SchemaReferenceKey => "#list";

        /// <inheritdoc/>
        public async ITask<IProviderResult<IDataList<object>>> Resolve(IProviderContext providerContext)
        {
            List<object> objects = new List<object>();
            foreach (var item in this.list)
            {
                if (item.GetType() == typeof(DynamicObjectProvider))
                {
                    var obj = (await ((DynamicObjectProvider)item).Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                    objects.Add(obj);
                }
                else
                {
                    objects.Add(item);
                }
            }

            return ProviderResult<IDataList<object>>.Success(new GenericDataList<object>(objects));
        }
    }
}
