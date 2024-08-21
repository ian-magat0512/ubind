// <copyright file="IAutomationPeriodicTriggerScheduler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Product;

    public interface IAutomationPeriodicTriggerScheduler
    {
        Task RegisterPeriodicTriggerJobs();

        Task RegisterPeriodicTriggerJobs(Guid tenantId);

        Task RegisterPeriodicTriggerJobs(Guid tenantId, Guid productId);

        Task RegisterPeriodicTriggerJobs(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        Task RegisterPeriodicTriggerJobs(Guid tenantId, Guid productId, DeploymentEnvironment environment, ActiveDeployedRelease release);

        void RemovePeriodicTriggerJobs(Guid tenantId);

        void RemovePeriodicTriggerJobs(Product product);

        void RemovePeriodicTriggerJobs(Guid tenantId, Guid productId, DeploymentEnvironment environment);
    }
}
