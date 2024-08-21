// <copyright file="TextContentProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Http
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents a text content defined by a text provider.
    /// </summary>
    public class TextContentProvider : ContentProvider
    {
        private IProvider<Data<string>>? content;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextContentProvider"/> class.
        /// </summary>
        /// <param name="contentString">The provider for the string content.</param>
        public TextContentProvider(IProvider<Data<string>>? contentString)
        {
            this.content = contentString;
        }

        public override string SchemaReferenceKey => "contentText";

        /// <inheritdoc/>
        public override async ITask<IProviderResult<IData>> Resolve(IProviderContext providerContext)
        {
            var content = (await this.content.ResolveValueIfNotNull(providerContext))?.DataValue;
            return ProviderResult<IData>.Success(new Data<string?>(content));
        }
    }
}
