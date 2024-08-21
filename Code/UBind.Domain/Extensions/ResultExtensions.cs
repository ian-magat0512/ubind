// <copyright file="ResultExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System;
    using CSharpFunctionalExtensions;

    /// <summary>
    /// Method for calling an action in the event of a result being a failure.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Execute a given action in the event of a result being a failure.
        /// </summary>
        /// <param name="result">The result to test.</param>
        /// <param name="action">The action to execute on failure.</param>
        /// <returns>The result for call chaining.</returns>
        public static IResult OnFailure(this IResult result, Action action)
        {
            if (result.IsFailure)
            {
                action();
            }

            return result;
        }

        /// <summary>
        /// Execute a given action in the event of a result being a failure.
        /// </summary>
        /// <typeparam name="TValue">The type of the value that successful results contain.</typeparam>
        /// <typeparam name="TError">The type of the error that failure results contain.</typeparam>
        /// <param name="result">The result to test.</param>
        /// <param name="action">The action to execute on failure.</param>
        /// <returns>The result for call chaining.</returns>
        public static Result<TValue, TError> OnFailure<TValue, TError>(this Result<TValue, TError> result, Action action)
        {
            if (result.IsFailure)
            {
                action();
            }

            return result;
        }

        /// <summary>
        /// Gets a ResultAssertions instance for use in FluentAssertions.
        /// Adds a Should() method to Result for use as part of FluentAssertions.
        /// </summary>
        /// <param name="instance">the error instance.</param>
        /// <typeparam name="TValue">The result type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <returns>An instance of the ResultAssertions classs.</returns>
        public static ResultAssertions<TValue, TError> Should<TValue, TError>(this Result<TValue, TError> instance)
        {
            return new ResultAssertions<TValue, TError>(instance);
        }

        /// <summary>
        /// Returns a new Result with a failure status and error when the boolean value passed is true.
        /// </summary>
        /// <param name="result">The Result object we are operating on.</param>
        /// <param name="when">the boolean value that when true signifies failure.</param>
        /// <param name="error">the description of the error.</param>
        /// <returns>a new Result.</returns>
        public static Result FailWhen(this Result result, bool when, string error)
        {
            return
                result.IsFailure ?
                    result :
                    when ?
                        Result.Failure(error) :
                        result;
        }

        /// <summary>
        /// Returns a new Result with a failure status and error when the boolean value passed is true.
        /// </summary>
        /// <param name="result">The Result object we are operating on.</param>
        /// <param name="when">a delegate which returns a boolean value that when true signifies failure.</param>
        /// <param name="error">the description of the error.</param>
        /// <returns>a new Result.</returns>
        public static Result FailWhen(this Result result, Func<bool> when, string error)
        {
            return
                result.IsFailure ?
                    result :
                    when() ?
                        Result.Failure(error) :
                        result;
        }

        /// <summary>
        /// Returns a new Result with a failure status and error when the boolean value passed is true.
        /// </summary>
        /// <param name="result">The Result object we are operating on.</param>
        /// <param name="when">the boolean value that when true signifies failure.</param>
        /// <param name="error">the description of the error.</param>
        /// <typeparam name="TReturn">The return type for the function.</typeparam>
        /// <returns>a new Result.</returns>
        public static Result<TReturn> FailWhen<TReturn>(this Result<TReturn> result, bool when, string error)
        {
            return
                result.IsFailure ?
                    result :
                    when ?
                        Result.Failure<TReturn>(error) :
                        result;
        }

        /// <summary>
        /// Returns a new Result with a failure status and error when the boolean value passed is true.
        /// </summary>
        /// <param name="result">The Result object we are operating on.</param>
        /// <param name="when">a delegate which returns a boolean value that when true signifies failure.</param>
        /// <param name="error">the description of the error.</param>
        /// <typeparam name="TReturn">The return type for the function.</typeparam>
        /// <returns>a new Result.</returns>
        public static Result<TReturn> FailWhen<TReturn>(this Result<TReturn> result, Func<bool> when, string error)
        {
            return
                result.IsFailure ?
                    result :
                    when() ?
                        Result.Failure<TReturn>(error) :
                        result;
        }

        /// <summary>
        /// Returns a new Result with a failure status and error when the boolean value passed is true.
        /// </summary>
        /// <param name="result">The Result object we are operating on.</param>
        /// <param name="when">the boolean value that when true signifies failure.</param>
        /// <param name="error">the description of the error.</param>
        /// <typeparam name="TReturn">The return type for the function.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <returns>a new Result.</returns>
        public static Result<TReturn, TError> FailWhen<TReturn, TError>(this Result<TReturn, TError> result, bool when, TError error)
            where TError : class
        {
            return
                result.IsFailure ?
                    result :
                    when ?
                        Result.Failure<TReturn, TError>(error) :
                        result;
        }

        /// <summary>
        /// Returns a new Result with a failure status and error when the boolean value passed is true.
        /// </summary>
        /// <param name="result">The Result object we are operating on.</param>
        /// <param name="when">a delegate which returns a boolean value that when true signifies failure.</param>
        /// <param name="error">the description of the error.</param>
        /// <typeparam name="TReturn">The return type for the function.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <returns>a new Result.</returns>
        public static Result<TReturn, TError> FailWhen<TReturn, TError>(this Result<TReturn, TError> result, Func<bool> when, TError error)
            where TError : class
        {
            return
                result.IsFailure ?
                    result :
                    when() ?
                        Result.Failure<TReturn, TError>(error) :
                        result;
        }

        /// <summary>
        /// Convenience function to return OK with a value from an existing Result object, for using method chaining.
        /// </summary>
        /// <param name="result">the result object we are operating on.</param>
        /// <returns>a succesful result with no return value.</returns>
        public static Result Ok(this Result result)
        {
            return
                result.IsFailure ?
                    result :
                    Result.Success();
        }

        /// <summary>
        /// Convenience function to return OK with a value from an existing Result object, for using method chaining.
        /// </summary>
        /// <typeparam name="TReturn">The return type.</typeparam>
        /// <param name="result">the result object we are operating on.</param>
        /// <param name="value">the successful value to return.</param>
        /// <returns>a result containing the successful return value.</returns>
        public static Result<TReturn> Ok<TReturn>(this Result<TReturn> result, TReturn value)
        {
            return
                result.IsFailure ?
                    result :
                    Result.Success<TReturn>(value);
        }

        /// <summary>
        /// Convenience function to return OK with a value from an existing Result object, for using method chaining.
        /// </summary>
        /// <typeparam name="TReturn">The return type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">the result object we are operating on.</param>
        /// <param name="value">the successful value to return.</param>
        /// <returns>a result containing the successful return value.</returns>
        public static Result<TReturn, TError> Ok<TReturn, TError>(this Result<TReturn, TError> result, TReturn value)
            where TError : class
        {
            return
                result.IsFailure ?
                    result :
                    Result.Success<TReturn, TError>(value);
        }

        /// <summary>
        /// The begin.
        /// </summary>
        /// <typeparam name="TReturn">Return of Type T.</typeparam>
        /// <typeparam name="TError">Error of Type T.</typeparam>
        /// <returns>a new Result.</returns>
        public static Result<TReturn, TError> Begin<TReturn, TError>()
            where TError : class
        {
            return Result.SuccessIf<TReturn, TError>(true, (TReturn)new object(), (TError)new object());
        }

        /// <summary>
        /// The begin.
        /// </summary>
        /// <typeparam name="TReturn">Return of Type T.</typeparam>
        /// <typeparam name="TError">Error of Type T.</typeparam>
        /// <param name="result">the result object.</param>
        /// <returns>a new Result.</returns>
        public static Result<TReturn, TError> Begin<TReturn, TError>(this Result result)
            where TError : class
        {
            return Result.SuccessIf<TReturn, TError>(true, (TReturn)new object(), (TError)new object());
        }

        /// <summary>
        /// The begin.
        /// </summary>
        /// <typeparam name="TReturn">Return of Type T.</typeparam>
        /// <typeparam name="TError">Error of Type T.</typeparam>
        /// <param name="result">the result object.</param>
        /// <returns>a new Result.</returns>
        public static Result<TReturn, TError> Begin<TReturn, TError>(this Result<TReturn> result)
            where TError : class
        {
            return Result.SuccessIf<TReturn, TError>(true, (TReturn)new object(), (TError)new object());
        }

        /// <summary>
        /// The begin.
        /// </summary>
        /// <typeparam name="TReturn">Return of Type T.</typeparam>
        /// <typeparam name="TError">Error of Type T.</typeparam>
        /// <param name="result">the result object.</param>
        /// <returns>a new Result.</returns>
        public static Result<TReturn, TError> Begin<TReturn, TError>(this Result<TReturn, TError> result)
            where TError : class
        {
            return Result.SuccessIf<TReturn, TError>(true, (TReturn)new object(), (TError)new object());
        }

        /// <summary>
        /// The begin.
        /// </summary>
        /// <typeparam name="TReturn">Return of Type T.</typeparam>
        /// <returns>a new Result.</returns>
        public static Result<TReturn> Begin<TReturn>()
        {
            return Result.SuccessIf<TReturn>(true, (TReturn)new object(), null);
        }

        /// <summary>
        /// The begin.
        /// </summary>
        /// <param name="result">the result object.</param>
        /// <returns>a new Result.</returns>
        public static Result Begin(this Result result)
        {
            return Result.SuccessIf(true, null);
        }
    }
}
