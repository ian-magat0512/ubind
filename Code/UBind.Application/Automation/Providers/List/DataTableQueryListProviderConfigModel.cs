// <copyright file="DataTableQueryListProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Domain.Repositories;

    public class DataTableQueryListProviderConfigModel : IBuilder<IDataListProvider<object>>
    {
        public IBuilder<BaseEntityProvider> Entity { get; set; }

        public IBuilder<IProvider<Data<string>>> DataTableAlias { get; set; }

        public IDataListProvider<object> Build(IServiceProvider dependencyProvider)
        {
            return new DataTableQueryListProvider(
                this.Entity.Build(dependencyProvider),
                this.DataTableAlias.Build(dependencyProvider),
                dependencyProvider.GetService<IDataTableDefinitionRepository>(),
                dependencyProvider.GetService<IDataTableContentRepository>());
        }
    }
}
