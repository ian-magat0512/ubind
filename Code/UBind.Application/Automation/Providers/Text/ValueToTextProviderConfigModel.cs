// <copyright file="ValueToTextProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Text;

public class ValueToTextProviderConfigModel : IBuilder<IProvider<Data<string?>>>
{
    public IBuilder<IProvider<IData>>? Value { get; set; }

    public IProvider<Data<string?>> Build(IServiceProvider dependencyProvider)
    {
        return new ValueToTextProvider(this.Value?.Build(dependencyProvider));
    }
}
