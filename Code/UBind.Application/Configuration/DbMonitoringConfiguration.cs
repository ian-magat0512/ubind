// <copyright file="DbMonitoringConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>
namespace UBind.Application.Configuration
{
    /// <summary>
    /// Job configuration for the db connection monitoring background service.
    /// </summary>
    public class DbMonitoringConfiguration
    {
        public int SqlDatabaseConnectionCountNotificationThreshold { get; set; }

        public int SqlDatabaseConnectionCountReviewIntervalMinutes { get; set; }
    }
}
