// <copyright file="ContentProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Http
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents content for an HTTP request or response.
    /// May be a single content or an array of content (multi-part).
    /// </summary>
    public abstract class ContentProvider : IProvider<IData>
    {
        public abstract string SchemaReferenceKey { get; }

        /// <inheritdoc/>
        public abstract ITask<IProviderResult<IData>> Resolve(IProviderContext providerContext);
    }
}
