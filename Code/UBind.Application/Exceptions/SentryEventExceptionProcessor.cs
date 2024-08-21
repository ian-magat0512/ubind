// <copyright file="SentryEventExceptionProcessor.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Exceptions;

using Sentry;
using Sentry.Extensibility;
using Sentry.Protocol;
using System;

/// <summary>
/// This a sentry exception processor that processes the exception before sending it on sentry.
/// We need to filter those exceptions if handled or unhandled. The unhandled exception should be sent to sentry,
/// because that is an actual error that is not handled by our application.
/// </summary>
public class SentryEventExceptionProcessor : BaseExceptionExclusionFilter, ISentryEventExceptionProcessor
{
    public void Process(Exception exception, SentryEvent sentryEvent)
    {
        if (this.IsExceptionToBeExcluded(exception))
        {
            sentryEvent.SentryExceptions = null;
            return;
        }

        if (sentryEvent.SentryExceptions == null || !sentryEvent.SentryExceptions.Any())
        {
            return;
        }

        foreach (var ex in sentryEvent.SentryExceptions)
        {
            ex.Mechanism = new Mechanism
            {
                Type = "ExceptionHandlerMiddleware",
                Handled = false,
            };

            if (exception.Data != null)
            {
                ex.Mechanism.Data.Add("Additional Data", exception.Data);
            }
        }
    }
}
