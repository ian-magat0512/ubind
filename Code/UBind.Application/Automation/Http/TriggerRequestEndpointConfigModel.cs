// <copyright file="TriggerRequestEndpointConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Helper;

    /// <summary>
    /// Model for creating an instance of <see cref="TriggerRequestEndpoint"/>.
    /// </summary>
    public class TriggerRequestEndpointConfigModel : IBuilder<TriggerRequestEndpoint>
    {
        /// <summary>
        /// Gets or sets the relative path of the endpoint URL. This will be appended to the root path of the HTTP trigger endpoints.
        /// </summary>
        /// <remarks>This value must be unique per tenant and per HTTP verb.</remarks>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the HTTP verb that the endpoint will respond to. Defaults to GET.
        /// </summary>
        public string HttpVerb { get; set; } = "GET";

        /// <summary>
        /// Gets or sets a collection representing validation rules applied to requests made to the endpoint.
        /// If any fail, the response will be an error as defined.
        /// </summary>
        public IEnumerable<ErrorConditionConfigModel> RequestValidationErrorConditions { get; set; } = Enumerable.Empty<ErrorConditionConfigModel>();

        /// <inheritdoc/>
        public TriggerRequestEndpoint Build(IServiceProvider dependencyProvider)
        {
            var conditions = this.RequestValidationErrorConditions.Select(rv => rv.Build(dependencyProvider));
            HttpHelper.ThrowIfHttpVerbInvalid(this.HttpVerb);
            return new TriggerRequestEndpoint(this.Path, this.HttpVerb, conditions);
        }
    }
}
