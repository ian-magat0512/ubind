// <copyright file="ValueToIntegerProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Integer;

public class ValueToIntegerProviderConfigModel : IBuilder<IProvider<Data<long>>>
{
    public IBuilder<IProvider<IData>>? Value { get; set; }

    public IProvider<Data<long>> Build(IServiceProvider dependencyProvider)
    {
        return new ValueToIntegerProvider(this.Value?.Build(dependencyProvider));
    }
}
