// <copyright file="WebhookField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the configuration for a webhook field, which when triggered will send a request to an external URL
    /// and process it's response, optionally setting data in the form.
    /// </summary>
    [WorkbookFieldType("Webhook")]
    [JsonFieldType("webhook")]
    public class WebhookField : Field
    {
        /// <summary>
        /// Gets or sets an expression which evaluates to the URL the webhook should hit.
        /// </summary>
        [Required]
        public string UrlExpression { get; set; }

        /// <summary>
        /// Gets or sets an expression value which when it's resulting value changes, it will trigger the webhook request.
        /// </summary>
        public string TriggerExpression { get; set; }

        /// <summary>
        /// Gets or sets an expression which when it evaluates to true, it will allow webhook requests to be triggered.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets the HTTP verb to use when requesting data from the API (GET or POST). Defaults to POST if not
        /// set.
        /// </summary>
        public string HttpVerb { get; set; } = "POST";

        /// <summary>
        /// Gets or sets a uBind expression which generates some data to send as part of the option request.
        /// This could JSON or XML. This should only be used If the HTTP verb is POST. If using GET, encode the
        /// data in the URL instead.
        /// If not set, the webhook will send the current form model as the payload.
        /// </summary>
        public string PayloadExpression { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds of no activity to wait before triggering the webhook.
        ///
        /// If the triggerExpression result changes many times in succession, it can cause many webhook requests to trigger,
        /// flooding the API with requests. To stop this from happening after trigger, we have a delay period, during which
        /// we wait until there a no further triggers before making the request to the server. Only if there are no further
        /// triggers in this period, do we make the request. The default is 100 milliseconds.
        /// </summary>
        public int DebounceTimeMilliseconds { get; set; } = 100;

        /// <summary>
        /// Gets or sets a value indicating whether when the webhook returns a result, it should automatically populate
        /// any form field values which have matching names.
        /// </summary>
        public bool? AutoPopulateFormModel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether when the field is first rendered, whether it should automatically
        /// trigger. Defaults to false.
        /// </summary>
        public bool? AutoTrigger { get; set; }

        /// <summary>
        /// Gets or sets the maximum age in seconds for caching search select.
        /// This property allows you to specify the maximum age, in seconds, for which search select can be cached.
        /// When search select is cached, it can be reused for a certain duration specified by this property
        /// before it expires and needs to be refreshed.
        /// </summary>
        public int? AllowCachingWithMaxAgeSeconds { get; set; }
    }
}
