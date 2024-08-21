﻿// <copyright file="SelectedOptionRequest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    /// <summary>
    /// Configuration for a request to load a single option from the API on page load to present the data
    /// for a selected option. You need this to get the label for the selected option, and any additional
    /// properties.
    /// </summary>
    public class SelectedOptionRequest
    {
        /// <summary>
        /// Gets or sets a uBind expression which evaluates the API endpoing URL where the option can be fetched from.
        /// </summary>
        [Required]
        public string UrlExpression { get; set; }

        /// <summary>
        /// Gets or sets the HTTP verb to use when requesting data from the API (GET or POST). Defaults to GET if not
        /// set.
        /// </summary>
        public string HttpVerb { get; set; } = "GET";

        /// <summary>
        /// Gets or sets a uBind expression which generates some data to send as part of the option request.
        /// This could JSON or XML. This should only be used If the HTTP verb is POST. If using GET, encode the
        /// data in the URL instead.
        /// </summary>
        public string PayloadExpression { get; set; }

        /// <summary>
        /// Gets or sets the maximum age in seconds for caching search select.
        /// This property allows you to specify the maximum age, in seconds, for which search select can be cached.
        /// When search select is cached, it can be reused for a certain duration specified by this property
        /// before it expires and needs to be refreshed.
        /// </summary>
        public int? AllowCachingWithMaxAgeSeconds { get; set; }
    }
}
