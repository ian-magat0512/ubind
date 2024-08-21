// <copyright file="ResultAssertions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using CSharpFunctionalExtensions;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using FluentAssertions.Primitives;

    /// <summary>
    /// Extension to FluentAssertions that provides assertions for the CSharpFunctionExtensions.Result type.
    /// </summary>
    /// <typeparam name="TValue">The value type from Result.</typeparam>
    /// <typeparam name="TError">The error type from Result.</typeparam>
    public class ResultAssertions<TValue, TError> : ReferenceTypeAssertions<Result, ResultAssertions<TValue, TError>>
    {
        private readonly Result<TValue, TError> result;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultAssertions{TValue, TError}"/> class.
        /// </summary>
        /// <param name="result">The Result instance.</param>
        public ResultAssertions(Result<TValue, TError> result)
            : base(default)
        {
            this.result = result;
        }

        /// <inheritdoc/>
        protected override string Identifier => "result";

        /// <summary>
        /// Gets a FluentAssertions AndConstraint and provides a Should().Succeed() method.
        /// </summary>
        /// <returns>An AndConstraint for chaining.</returns>
        public AndConstraint<ResultAssertions<TValue, TError>> Succeed()
        {
            if (this.result.IsFailure)
            {
                Execute.Assertion
                    .ForCondition(this.result.IsSuccess)
                    .FailWith(this.result.Error.ToString());
            }

            return new AndConstraint<ResultAssertions<TValue, TError>>(this);
        }

        /// <summary>
        /// Gets a FluentAssertions AndConstraint and provides a Should().Fail() method.
        /// </summary>
        /// <returns>An AndConstraint for chaining.</returns>
        public AndConstraint<ResultAssertions<TValue, TError>> Fail()
        {
            Execute.Assertion
                .ForCondition(this.result.IsFailure)
                .FailWith("Result was expected to fail but did not.");

            return new AndConstraint<ResultAssertions<TValue, TError>>(this);
        }
    }
}
