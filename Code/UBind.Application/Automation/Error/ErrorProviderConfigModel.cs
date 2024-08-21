// <copyright file="ErrorProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Error
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;

    /// <summary>
    /// Model for building an instance of <see cref="ErrorProvider"/>.
    /// </summary>
    public class ErrorProviderConfigModel : IBuilder<IProvider<ConfiguredError>>
    {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Code { get; set; }

        /// <summary>
        /// Gets or sets the title for the message.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Title { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Message { get; set; }

        /// <summary>
        /// Gets or sets the http status code.
        /// </summary>
        public IBuilder<IProvider<Data<long>>> HttpStatusCode { get; set; }

        /// <summary>
        /// Gets or sets a collection of additional details, if any.
        /// </summary>
        public IEnumerable<IBuilder<IProvider<Data<string>>>> AdditionalDetails { get; set; } = Enumerable.Empty<IBuilder<IProvider<Data<string>>>>();

        /// <summary>
        /// Gets or sets any additional data which may be included to assist in error handling.
        /// </summary>
        public IBuilder<IObjectProvider> Data { get; set; }

        /// <inheritdoc/>
        public IProvider<ConfiguredError> Build(IServiceProvider dependencyProvider)
        {
            var details = this.AdditionalDetails.Select(ad => ad.Build(dependencyProvider));
            return new ErrorProvider(
                this.Code.Build(dependencyProvider),
                this.Title.Build(dependencyProvider),
                this.Message.Build(dependencyProvider),
                this.HttpStatusCode.Build(dependencyProvider),
                details,
                this.Data?.Build(dependencyProvider));
        }
    }
}
