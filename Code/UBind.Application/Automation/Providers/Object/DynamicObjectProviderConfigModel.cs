// <copyright file="DynamicObjectProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Object
{
    using System;
    using System.Collections.Generic;
    using LinqKit;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using UBind.Application.Automation.Object;

    /// <summary>
    /// Model for building an instance of <see cref="DynamicObjectProvider"/>.
    /// </summary>
    public class DynamicObjectProviderConfigModel : IBuilder<IObjectProvider>
    {
        /// <summary>
        /// Gets or sets the properties of the dynamic object.
        /// </summary>
        public IEnumerable<ObjectPropertyConfigModel>? Properties { get; set; }

        /// <inheritdoc/>
        public IObjectProvider Build(IServiceProvider dependencyProvider)
        {
            Dictionary<IProvider<Data<string>>, IProvider<IData>?> dynamicObject =
                new Dictionary<IProvider<Data<string>>, IProvider<IData>?>();
            this.Properties?.ForEach(item =>
            {
                dynamicObject.Add(
                    item.PropertyName.Build(dependencyProvider),
                    item.Value?.Build(dependencyProvider));
            });

            var logger = (ILogger<DynamicObjectProvider>)dependencyProvider.GetRequiredService(typeof(ILogger<DynamicObjectProvider>));
            return new DynamicObjectProvider(dynamicObject, logger, dependencyProvider);
        }
    }
}
