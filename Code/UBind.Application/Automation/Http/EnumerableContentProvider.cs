// <copyright file="EnumerableContentProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System.Collections.Generic;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Represents a list of mime-type and content pairs, to be used when this content is multi-part.
    /// </summary>
    public class EnumerableContentProvider : ContentProvider
    {
        private IEnumerable<MultipartContentProperty> contents;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableContentProvider"/> class.
        /// </summary>
        /// <param name="contents">A list of mime-type/content pairs.</param>
        public EnumerableContentProvider(IEnumerable<MultipartContentProperty> contents)
        {
            this.contents = contents;
        }

        public override string SchemaReferenceKey => "contentList";

        /// <inheritdoc/>
        public override async ITask<IProviderResult<IData>> Resolve(IProviderContext providerContext)
        {
            var contentConfig = await this.contents.SelectAsync(async x => new
            {
                Content = (await x.Content.Resolve(providerContext)).GetValueOrThrowIfFailed(),
                ContentType = (await x.ContentType.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
            });

            var multipartContentConfig = contentConfig.
                Select(x => new ContentPart(x.Content.GetValueFromGeneric(), x.ContentType))
                .ToList();
            return ProviderResult<IData>.Success(new Data<IEnumerable<ContentPart>>(multipartContentConfig));
        }
    }
}
