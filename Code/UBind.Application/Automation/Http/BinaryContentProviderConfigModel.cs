// <copyright file="BinaryContentProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Model for building a content object of type byte array.
    /// </summary>
    public class BinaryContentProviderConfigModel : IBuilder<ContentProvider>
    {
        public IBuilder<IProvider<Data<byte[]>>> Content { get; set; }

        public ContentProvider Build(IServiceProvider dependencyProvider)
        {
            return new BinaryContentProvider(this.Content.Build(dependencyProvider));
        }
    }
}
