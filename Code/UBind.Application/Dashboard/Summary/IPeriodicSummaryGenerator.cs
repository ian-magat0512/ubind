// <copyright file="IPeriodicSummaryGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Dashboard.Summary;

using UBind.Application.Dashboard.Model;
using UBind.Domain.ReadModel;

/// <summary>
/// This interface defines the methods the summary generator should implement.
/// </summary>
/// <typeparam name="TRecord">The records to summarise.</typeparam>
/// <typeparam name="TSummary">The summary of a collection of records.</typeparam>
public interface IPeriodicSummaryGenerator<TRecord, TSummary>
    where TSummary : IPeriodicSummaryModel, new()
    where TRecord : IDashboardSummaryModel, new()
{
    /// <summary>
    /// Generates the list of periodic summaries for the given records.
    /// Each summary (<see cref="IPeriodicSummaryModel"/>) is generated
    /// from a collection of records (<see cref="IDashboardSummaryModel"/>).
    /// The generated list of summaries are ordered by period and for
    /// period with no records, a summary with zero values is used.
    /// For more information, see <see cref="IPeriodicSummaryGeneratorConfiguration"/>
    /// which is used to configure the generator.
    /// </summary>
    /// <param name="records">The records to summarise.</param>
    /// <returns>The list of periodic summaries.</returns>
    List<TSummary> GenerateSummary(IEnumerable<TRecord> records);
}