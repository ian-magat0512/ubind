// <copyright file="DynamicListBuilder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using System.Collections.Generic;
    using UBind.Application.Automation.Providers.Object;

    /// <summary>
    /// Builder which builds a provider from a dynamic object list value.
    /// This is used to set properties of config models from the automations config where the list is declared directly.
    /// </summary>
    public class DynamicListBuilder : IBuilder<IDataListProvider<object>>
    {
        public IDataList<object> Value { get; set; }

        public IDataListProvider<object> Build(IServiceProvider dependencyProvider)
        {
            List<object> objs = new List<object>();
            if (this.Value.GetType() == typeof(GenericDataList<object>))
            {
                foreach (var item in this.Value)
                {
                    if (item.GetType() == typeof(DynamicObjectProviderConfigModel))
                    {
                        DynamicObjectProviderConfigModel configModel = (DynamicObjectProviderConfigModel)item;
                        var objProvider = configModel.Build(dependencyProvider);
                        objs.Add(objProvider);
                    }
                    else
                    {
                        objs.Add(item);
                    }
                }

                return new DynamicListProvider(new GenericDataList<object>(objs));
            }

            return new DynamicListProvider(new GenericDataList<object>(objs));
        }
    }
}
