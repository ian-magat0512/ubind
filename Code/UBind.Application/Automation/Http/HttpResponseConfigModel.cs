// <copyright file="HttpResponseConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Model for building an <see cref="HttpResponse"/>.
    /// </summary>
    public class HttpResponseConfigModel : IBuilder<HttpResponse>
    {
        /// <summary>
        /// Gets or sets the HTTP status code to be set for the HTTP response.
        /// </summary>
        public IBuilder<IProvider<Data<long>>>? HttpStatusCode { get; set; }

        /// <summary>
        /// Gets or sets the headers in the HTTP response.
        /// </summary>
        public IEnumerable<IBuilder<IProvider<KeyValuePair<string, IEnumerable<string>>>>>? Headers { get; set; }

        /// <summary>
        /// Gets or sets the content-type of the HTTP response.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? ContentType { get; set; }

        /// <summary>
        /// Gets or sets the content of the HTTP response. It should match the content-type.
        /// </summary>
        public IBuilder<ContentProvider>? Content { get; set; }

        /// <inheritdoc/>
        public HttpResponse Build(IServiceProvider dependencyProvider)
        {
            var headers = this.Headers?.Select(h => h.Build(dependencyProvider));
            return new HttpResponse(
                this.HttpStatusCode?.Build(dependencyProvider),
                headers,
                this.ContentType?.Build(dependencyProvider),
                this.Content?.Build(dependencyProvider));
        }
    }
}
