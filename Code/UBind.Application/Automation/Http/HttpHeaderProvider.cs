// <copyright file="HttpHeaderProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System.Collections.Generic;
    using System.Linq;
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Represents an instance of HttpHeader with a name and multiple values.
    /// </summary>
    public class HttpHeaderProvider : IProvider<KeyValuePair<string, IEnumerable<string>>>
    {
        private readonly IProvider<Data<string>> nameProvider;
        private readonly IEnumerable<IProvider<Data<string>>> valuesProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHeaderProvider"/> class.
        /// </summary>
        /// <param name="nameProvider">The string name provider.</param>
        /// <param name="valuesProvider">A collection of value providers.</param>
        public HttpHeaderProvider(
            IProvider<Data<string>> nameProvider,
            IEnumerable<IProvider<Data<string>>> valuesProvider)
        {
            this.nameProvider = nameProvider;
            this.valuesProvider = valuesProvider;
        }

        public string SchemaReferenceKey => "header";

        /// <summary>
        /// Resolves the key-value pairs necessary for headers.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The key-value pairs.</returns>
        public async ITask<IProviderResult<KeyValuePair<string, IEnumerable<string>>>> Resolve(IProviderContext providerContext)
        {
            var name = (await this.nameProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var resolveValues = await this.valuesProvider
                .SelectAsync(async v => (await v.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue);
            var values = resolveValues.Select(v => v.ToString());
            return ProviderResult<KeyValuePair<string, IEnumerable<string>>>.Success(
                new KeyValuePair<string, IEnumerable<string>>(name.ToString(), values));
        }
    }
}
