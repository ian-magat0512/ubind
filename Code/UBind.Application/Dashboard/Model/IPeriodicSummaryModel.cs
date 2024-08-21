// <copyright file="IPeriodicSummaryModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Dashboard.Model;

/// <summary>
/// This model represents the common properties of each periodic summary.
/// Depending on the period represented by <see cref="SamplePeriodLength"/>,
/// an instance of the derived class is created that use either date or
/// fromDate and toDate or use define a label for the summary.
/// </summary>
public interface IPeriodicSummaryModel
{
    string Label { get; set; }

    string FromDateTime { get; set; }

    string ToDateTime { get; set; }
}
