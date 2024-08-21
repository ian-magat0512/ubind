// <copyright file="IDashboardSummaryModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ReadModel;

using NodaTime;

/// <summary>
/// This model represents each record, i.e. quote, policy transaction or claim.
/// The timestamp property is used to group the records by period depending on
/// its value. The group could be based on creation date of the record
/// or some other date.
/// </summary>
public interface IDashboardSummaryModel
{
    /// <summary>
    /// Gets the timestamp of the record.
    /// This can be the creation date of the record or some other date.
    /// </summary>
    Instant Timestamp { get; }
}