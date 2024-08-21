// <copyright file="HttpRequestConfigurationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Model for building an instance of <see cref="HttpRequestConfiguration"/>.
    /// </summary>
    public class HttpRequestConfigurationConfigModel : IBuilder<HttpRequestConfiguration>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// Gets or sets the URL of http request.
        /// </summary>
        /// <remarks>This value must be unique per tenant and per HTTP verb.</remarks>
        public IBuilder<IProvider<Data<string>>> Url { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets the HTTP verb that the http request will respond to. Defaults to GET.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? HttpVerb { get; set; }

        /// <summary>
        /// Gets or sets the headers in the HTTP Request.
        /// </summary>
        public IEnumerable<IBuilder<IProvider<KeyValuePair<string, IEnumerable<string>>>>>? Headers { get; set; }

        /// <summary>
        /// Gets or sets the content-type of the HTTP request.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? ContentType { get; set; }

        /// <summary>
        /// Gets or sets the character set of the content, if applicable.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? CharacterSet { get; set; }

        /// <summary>
        /// Gets or sets the content of the HTTP request. It should match the content-type.
        /// </summary>
        public IBuilder<ContentProvider>? Content { get; set; }

        /// <summary>
        /// Gets or sets the client certificate of the HTTP request.
        /// </summary>
        public ClientCertificateConfigModel? ClientCertificate { get; set; }

        /// <inheritdoc/>
        public HttpRequestConfiguration Build(IServiceProvider dependencyProvider)
        {
            return new HttpRequestConfiguration(
              this.Url.Build(dependencyProvider),
              this.HttpVerb?.Build(dependencyProvider),
              this.Headers?.Select(head => head.Build(dependencyProvider)),
              this.ContentType?.Build(dependencyProvider),
              this.CharacterSet?.Build(dependencyProvider),
              this.Content?.Build(dependencyProvider),
              this.ClientCertificate?.Build(dependencyProvider));
        }
    }
}
