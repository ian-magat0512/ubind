// <copyright file="ResultModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain;

    /// <summary>
    /// For responses representing the outcome of a request with no response payload.
    /// </summary>
    public class ResultModel
    {
        private ResultModel() => this.Errors = Enumerable.Empty<string>();

        private ResultModel(IEnumerable<string> errors) => this.Errors = errors;

        /// <summary>
        /// Gets a value indicating whether the result represents a success.
        /// </summary>
        [JsonProperty]
        public bool Success => !this.Errors.Any();

        /// <summary>
        /// Gets the errors for a failure result (will be empty for successes).
        /// </summary>
        [JsonProperty]
        public IEnumerable<string> Errors { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="ResultModel"/> representing a success.
        /// </summary>
        /// <returns>A new instance of <see cref="ResultModel"/> representing a success.</returns>
        public static ResultModel CreateSuccess() => new ResultModel();

        /// <summary>
        /// Creates a new instance of <see cref="ResultModel"/> representing a failure.
        /// </summary>
        /// <param name="errors">The errors that caused the failure.</param>
        /// <returns>A new instance of <see cref="ResultModel"/> representing a failure.</returns>
        public static ResultModel CreateFailure(params string[] errors) =>
            new ResultModel((IEnumerable<string>)errors);

        /// <summary>
        /// Creates a new instance of <see cref="ResultModel"/> representing a failure.
        /// </summary>
        /// <param name="errors">The errors that caused the failure.</param>
        /// <returns>A new instance of <see cref="ResultModel"/> representing a failure.</returns>
        public static ResultModel CreateFailure(IEnumerable<string> errors)
            => new ResultModel(errors);

        /// <summary>
        /// Creates a new instance of <see cref="ResultModel"/> for a given result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>A new instance of <see cref="ResultModel"/> for a given result.</returns>
        public static ResultModel FromResult(ResultOld result)
        {
            return result.Succeeded
                ? CreateSuccess()
                : CreateFailure(result.Errors);
        }
    }
}
