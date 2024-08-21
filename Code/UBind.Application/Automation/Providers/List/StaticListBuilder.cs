// <copyright file="StaticListBuilder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System;

    /// <summary>
    /// Builder which builds a provider from a static list value.
    /// This is used to set properties of config models from the automations config where the list is declared directly.
    /// </summary>
    /// <typeparam name="T">The type of the items in the list which will be provided.</typeparam>
    public class StaticListBuilder<T> : IBuilder<IDataListProvider<T>>
    {
        public IDataList<T> Value { get; set; }

        public IDataListProvider<T> Build(IServiceProvider dependencyProvider)
        {
            return new StaticListProvider<T>(this.Value, dependencyProvider);
        }
    }
}
