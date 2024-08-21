// <copyright file="ErrorCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Error
{
    using System.Threading.Tasks;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents an error condition and an associated error object that will be raised if the condition is true.
    /// </summary>
    public class ErrorCondition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorCondition"/> class.
        /// </summary>
        /// <param name="condition">The condition to be evaluated.</param>
        /// <param name="error">The error to be raised if condition is true.</param>
        public ErrorCondition(IProvider<Data<bool>> condition, IProvider<ConfiguredError> error)
        {
            this.Condition = condition;
            this.Error = error;
        }

        /// <summary>
        /// Gets or sets the condition to be evaluated.
        /// </summary>
        public IProvider<Data<bool>> Condition { get; set; }

        /// <summary>
        /// Gets or sets the error that will be raised if the condition is true.
        /// </summary>
        public IProvider<ConfiguredError> Error { get; set; }

        /// <summary>
        /// Evaluates the given condition, and if true, will returned the resolved error. Otherwise, null.
        /// </summary>
        /// <param name="providerContext">The provider context to be used.</param>
        /// <returns>The configured error, if condition is verified. Otherwise, null.</returns>
        public async Task<ConfiguredError> Evaluate(IProviderContext providerContext)
        {
            bool isSatisfied = (await this.Condition.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            if (isSatisfied)
            {
                return (await this.Error.Resolve(providerContext)).GetValueOrThrowIfFailed();
            }

            return null;
        }
    }
}
