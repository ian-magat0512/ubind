// <copyright file="ValueToNumberProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Number;

using System;
using UBind.Application.Automation.Providers;

public class ValueToNumberProviderConfigModel : IBuilder<IProvider<Data<decimal>>>
{
    public IBuilder<IProvider<IData>>? Value { get; set; }

    public IProvider<Data<decimal>> Build(IServiceProvider dependencyProvider)
    {
        return new ValueToNumberProvider(this.Value?.Build(dependencyProvider));
    }
}
