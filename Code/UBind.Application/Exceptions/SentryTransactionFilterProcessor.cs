// <copyright file="SentryTransactionFilterProcessor.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Exceptions;

using Sentry;
using Sentry.Extensibility;
using System.Text.RegularExpressions;

/// <summary>
/// This filter will exclude health checks from the transaction sequence.
/// In case of an actual error that is being recorded, it will still show up on
/// the Issues view on sentry but not on trace (Performance view).
/// </summary>
public class SentryTransactionFilterProcessor : ISentryTransactionProcessor
{
    private Regex regexPattern;

    public SentryTransactionFilterProcessor()
    {
        this.regexPattern = new Regex(@"api\/v\d+\/health");
    }

    public Transaction? Process(Transaction transaction)
    {
        if (this.regexPattern.IsMatch(transaction?.Request?.Url))
        {
            // Discard the transaction by returning null
            return null;
        }

        return transaction;
    }
}
