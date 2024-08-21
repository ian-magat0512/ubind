// <copyright file="HttpTriggerData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using Newtonsoft.Json;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Http;

    /// <summary>
    /// Represents the data associated with a HTTP trigger.
    /// </summary>
    public class HttpTriggerData : TriggerData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTriggerData"/> class.
        /// </summary>
        /// <param name="request">The http request receied by the trigger.</param>
        public HttpTriggerData(TriggerRequest request)
            : base(TriggerType.HttpTrigger)
        {
            this.HttpRequest = request;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTriggerData"/> class.
        /// </summary>
        [JsonConstructor]
        public HttpTriggerData()
            : base(TriggerType.HttpTrigger)
        {
        }

        /// <summary>
        /// Gets the HTTP request received by the HTTP trigger.
        /// </summary>
        [JsonProperty("httpRequest", NullValueHandling = NullValueHandling.Ignore)]
        public TriggerRequest HttpRequest { get; private set; }

        /// <summary>
        /// Gets or sets the HTTP response to be returned by the HTTP trigger.
        /// </summary>
        [JsonProperty("httpResponse")]
        public Response HttpResponse { get; set; }
    }
}
