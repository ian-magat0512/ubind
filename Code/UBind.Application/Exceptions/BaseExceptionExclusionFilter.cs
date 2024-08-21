// <copyright file="BaseExceptionExclusionFilter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Exceptions
{
    using UBind.Application.Configuration;

    /// <summary>
    /// Provides the generic exception filtering operations used
    /// in order to exclude specific exceptions or excludes exceptions
    /// that contains specific error message or code from sentry.
    /// </summary>
    public abstract class BaseExceptionExclusionFilter
    {
        public virtual SentryExtrasConfiguration SentryExtrasConfig { get; set; }

        /// <summary>
        /// Determines if the exception is part of the excluded items defined
        /// on the SentryExtrasConfiguration.
        /// </summary>
        /// <param name="exception">Exception to be processed.</param>
        /// <returns></returns>
        public virtual bool IsExceptionToBeExcluded(Exception exception)
        {
            if (exception == null)
            {
                return false;
            }

            bool isExcluded = false;

            // Exclusion by exception type.
            this.SentryExtrasConfig.ExcludedExceptions.ForEach((exceptionName) =>
                {
                    if (exception.GetType().FullName.Equals(exceptionName, StringComparison.OrdinalIgnoreCase))
                    {
                        isExcluded = true;
                        return;
                    }
                });

            // Exclusion by error code.
            this.SentryExtrasConfig.ExcludedExceptionErrorCodes.ForEach((errorCode) =>
            {
                if (exception.Message.Contains(errorCode, StringComparison.OrdinalIgnoreCase))
                {
                    isExcluded = true;
                    return;
                }
            });

            return isExcluded;
        }
    }
}
