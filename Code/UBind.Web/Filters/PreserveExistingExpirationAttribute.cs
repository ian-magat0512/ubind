// <copyright file="PreserveExistingExpirationAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Attributes;

using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

public class PreserveExistingExpirationAttribute : JobFilterAttribute, IApplyStateFilter
{
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        var datetimeNow = DateTime.Now;
        var expireIn = context.Connection.GetJobParameter(context.BackgroundJob.Id, BackgroundJobParameter.ExpireAt);
        if (!string.IsNullOrEmpty(expireIn) && TimeSpan.TryParse(expireIn, out TimeSpan timeSpan))
        {
            var originalExpireAt = (datetimeNow + timeSpan) - datetimeNow;
            var newExpiry = originalExpireAt == default ? TimeSpan.FromSeconds(1) : originalExpireAt;
            transaction.ExpireJob(context.BackgroundJob.Id, newExpiry);
            context.JobExpirationTimeout = newExpiry;
        }
        else
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(7);
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // do nothing here.
    }
}
