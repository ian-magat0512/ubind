// <copyright file="HttpRequestActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Http;

    /// <summary>
    /// Represents the data of an action of type <see cref="HttpRequestAction"/>.
    /// </summary>
    public class HttpRequestActionData : ActionData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestActionData"/> class.
        /// </summary>
        /// <param name="name">The name of the action for display purposes.</param>
        /// <param name="alias">The alias of the action this data is for.</param>
        /// <param name="clock">The clock for telling the time.</param>
        public HttpRequestActionData(string name, string alias, IClock clock)
            : base(name, alias, ActionType.HttpRequestAction, clock)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestActionData"/> class.
        /// </summary>
        [JsonConstructor]
        public HttpRequestActionData()
            : base(ActionType.HttpRequestAction)
        {
        }

        /// <summary>
        /// Gets or sets the HTTP request made by the HTTP request action.
        /// </summary>
        [JsonProperty("httpRequest")]
        public Request HttpRequest { get; set; }

        /// <summary>
        /// Gets or sets the response received for the request.
        /// </summary>
        [JsonProperty("httpResponse")]
        public Response HttpResponse { get; set; }
    }
}
