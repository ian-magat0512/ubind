// <copyright file="TriggerRequestEndpoint.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Represents the definition of the API endpoint that the HTTP trigger will be invoked through.
    /// </summary>
    public class TriggerRequestEndpoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerRequestEndpoint"/> class.
        /// </summary>
        /// <param name="path">The path this endpoint listens to.</param>
        /// <param name="verb">The verb that this endpoint will respond to.</param>
        /// <param name="validationConditions">The validation rules.</param>
        public TriggerRequestEndpoint(
            string path,
            string verb,
            IEnumerable<ErrorCondition> validationConditions)
        {
            this.Path = path;
            this.HttpVerb = verb.ToUpper();
            this.RequestValidationErrorConditions = validationConditions;
        }

        /// <summary>
        /// Gets the relative path of the endpoint URL. This will be appended to the root path of the HTTP trigger endpoints.
        /// </summary>
        /// <remarks>This value must be unique per tenant and per HTTP verb.</remarks>
        public string Path { get; }

        /// <summary>
        /// Gets the HTTP verb that the endpoint will respond to. Defaults to GET.
        /// </summary>
        public string HttpVerb { get; }

        /// <summary>
        /// Gets a collection representing validation rules applied to requests made to the endpoint.
        /// If any fail, the response will be an error as defined.
        /// </summary>
        public IEnumerable<ErrorCondition> RequestValidationErrorConditions { get; } = Enumerable.Empty<ErrorCondition>();

        /// <summary>
        /// Evaluates the request validations, if any, and throws the accompanying error
        /// if any of the conditions evaluate to true.
        /// </summary>
        /// <param name="providerContext">The provider context.</param>
        public async Task ValidateAndThrowIfFail(IProviderContext providerContext)
        {
            foreach (var errorCondition in this.RequestValidationErrorConditions)
            {
                var error = await errorCondition.Evaluate(providerContext);
                if (error != null)
                {
                    throw new ErrorException(error.ToError());
                }
            }
        }
    }
}
