// <copyright file="IPatchService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.PolicyDataPatcher
{
    using System;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Hangfire.Server;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote.Commands;

    /// <summary>
    /// Represents the patch service.
    /// </summary>
    public interface IPatchService
    {
        /// <summary>
        /// Patches a policy's form data.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="aggregateId">The ID of the policy whose form data is to be patched.</param>
        /// <param name="command">The command with details of the patch.</param>
        /// <returns>An awaitable task of result.</returns>
        Task<Result> PatchPolicyDataAsync(Guid tenantId, Guid aggregateId, PolicyDataPatchCommand command);

        /// <summary>
        /// Queue patches from product form data in background thread.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant the product is in.</param>
        /// <param name="environment">The deployment environment the product is in.</param>
        /// <param name="organisationId">The Id of the organisation the product is in.</param>
        /// <param name="productId">The Id of the product.</param>
        /// <param name="model">The form data patch command model to use.</param>
        void QueuePatchProductFormData(
            Guid tenantId,
            DeploymentEnvironment environment,
            Guid organisationId,
            Guid productId,
            PolicyDataPatchCommandModel model);

        /// <summary>
        /// Patches from product form data in background thread.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the product is in.</param>
        /// <param name="environment">The deployment environment the product is in.</param>
        /// <param name="organisationId">The Id of the organisation the product is in.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="model">The form data patch command model to use.</param>
        /// <param name="context">The hangfire perform context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task PatchProductFormData(
            Guid tenantId,
            DeploymentEnvironment environment,
            Guid organisationId,
            Guid productId,
            PolicyDataPatchCommandModel model,
            PerformContext context);
    }
}
