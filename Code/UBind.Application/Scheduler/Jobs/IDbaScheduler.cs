// <copyright file="IDbaScheduler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>
namespace UBind.Application.Scheduler.Jobs;
using System.Data.Entity;
using UBind.Domain.ReadModels.Dba;

/// <summary>
/// Definition for the DBA tasks to be performed per database.
/// </summary>
public interface IDbaScheduler<T>
    where T : DbContext
{

    /// <summary>
    /// The repository instance of the Scheduler context.
    /// </summary>

    /// <summary>
    /// Performs the specific monitoring check of pools according to its settings per DB.
    /// </summary>
    void RegisterDbaMonitoring();

    /// <summary>
    /// Triggers the notification email with the details of the DB status.
    /// </summary>
    /// <param name="sysViewReadModel">Details of per connection status.</param>
    /// <param name="initialMessage">Initial information message.</param>
    void EscalateEmailNotification(List<SqlServerSysProcessViewModel> sysViewReadModel, string initialMessage);

    double CalculatePercentage(double value, double totalValue);
}