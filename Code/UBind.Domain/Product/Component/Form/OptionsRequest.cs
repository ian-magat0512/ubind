// <copyright file="OptionsRequest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents configuration associated with an options field, which
    /// provides a way to configure loading options from an external data source.
    /// </summary>
    public class OptionsRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether an options request is triggered from the start so that the
        /// options are populated and ready.
        /// </summary>
        public bool? AutoTrigger { get; set; }

        /// <summary>
        /// Gets or sets a uBind expression which when it evaluates to a different result, will cause
        /// the options to refresh themselves from the API.
        /// </summary>
        public string TriggerExpression { get; set; }

        /// <summary>
        /// Gets or sets a uBind expression which must evaluate to true before options will be fetched from the API.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets a uBind expression which evaluates the API endpoing URL where options can be fetched from.
        /// </summary>
        [Required]
        public string UrlExpression { get; set; }

        /// <summary>
        /// Gets or sets the HTTP verb to use when requesting data from the API (GET or POST). Defaults to GET if not set.
        /// </summary>
        public string HttpVerb { get; set; } = "GET";

        /// <summary>
        /// Gets or sets a uBind expression which generates some data to send as part of the options request.
        /// This could JSON or XML. This should only be used If the HTTP verb is POST. If using GET, encode the
        /// data in the URL instead.
        /// </summary>
        public string PayloadExpression { get; set; }

        /// <summary>
        /// Gets or sets a delay period during which we wait until there a no further triggers before making the
        /// request to the server. This stops flooding the server with too many API requests when someone is typing.
        /// </summary>
        public int DebounceTimeMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the maximum age in seconds for caching search select.
        /// This property allows you to specify the maximum age, in seconds, for which search select can be cached.
        /// When search select is cached, it can be reused for a certain duration specified by this property
        /// before it expires and needs to be refreshed.
        /// </summary>
        public int? AllowCachingWithMaxAgeSeconds { get; set; }
    }
}
