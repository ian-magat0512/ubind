// <copyright file="IPeriodicSummaryQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>
namespace UBind.Application;

using NodaTime;
using System.Collections.Generic;
using UBind.Domain.Enums;

/// <summary>
/// Interface used for the parameters of the GET request to the Periodic Summary endpoint.
/// </summary>
public interface IPeriodicSummaryQueryOptionsModel
{
    /// <summary>
    /// Gets the period by which the records will be summarized.
    /// </summary>
    SamplePeriodLength? SamplePeriodLength { get; }

    /// <summary>
    /// Gets or sets the start date by which the records will be filtered.
    /// </summary>
    string? FromDateTime { get; set; }

    /// <summary>
    /// Gets or sets the end date by which the records will be filtered.
    /// </summary>
    string? ToDateTime { get; set; }

    /// <summary>
    /// Gets or sets the list of properties to include in each result set.
    /// </summary>
    IEnumerable<string>? IncludeProperties { get; set; }

    /// <summary>
    /// Gets or sets the list of products by which the records will be filtered.
    /// </summary>
    IEnumerable<string>? Products { get; set; }

    /// <summary>
    /// Gets custom period by which the records will be summarized when SamplePeriodLength is custom.
    /// </summary>
    int? CustomSamplePeriodMinutes { get; }

    /// <summary>
    /// Gets or sets the timezone of the supplied from and to date.
    /// </summary>
    string? TimeZoneId { get; set; }

    void SetFromDateTime(Instant? dateTime);
}
