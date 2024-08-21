// <copyright file="FlexCelWorkbookPool.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.FlexCel;

using System;
using Microsoft.Extensions.Logging;
using NodaTime;
using StackExchange.Profiling;
using UBind.Application.ResourcePool;
using UBind.Application.Services.Email;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.Product;

/// <summary>
/// Represents a pool of instances of the exact same excel file, stored in memory, ready for use.
/// </summary>
public class FlexCelWorkbookPool : ResourcePool
{
    private readonly byte[] workbookBytes;
    private readonly WebFormAppType webFormAppType;
    private readonly IErrorNotificationService errorNotificationService;

    private FlexCelWorkbookPool(
        ReleaseContext releaseContext,
        WebFormAppType webFormAppType,
        byte[] workbookBytes,
        Guid releaseId,
        IClock clock,
        ILogger<IResourcePool> logger,
        IErrorNotificationService errorNotificationService,
        IFlexCelWorkbook? seedWorkbook = null)
        : base(clock, logger, errorNotificationService)
    {
        this.ReleaseContext = releaseContext;
        this.webFormAppType = webFormAppType;
        this.workbookBytes = workbookBytes;
        this.ReleaseId = releaseId;
        if (seedWorkbook != null)
        {
            this.InsertResource(seedWorkbook);
        }

        this.errorNotificationService = errorNotificationService;
    }

    /// <summary>
    /// Gets the ID of the release the workbook pool is for.
    /// </summary>
    public Guid ReleaseId { get; }

    /// <summary>
    /// Gets the product context the workbook is for.
    /// </summary>
    public ReleaseContext ReleaseContext { get; private set; }

    /// <summary>
    /// Creates a new instance of the <see cref="FlexCelWorkbookPool"/> class.
    /// </summary>
    /// <param name="productContext">The product context the workbook is for.</param>
    /// <param name="webFormAppType">The app the workbook is for.</param>
    /// <param name="workbookBytes">The workbook file as bytes.</param>
    /// <param name="releaseId">The release ID.</param>
    /// <param name="clock">It's a clock.</param>
    /// <param name="logger">Logger for logging.</param>
    /// <returns>A new instance of the <see cref="FlexCelWorkbookPool"/> class.</returns>
    public static FlexCelWorkbookPool CreateEmptyPool(
        ReleaseContext productContext,
        WebFormAppType webFormAppType,
        byte[] workbookBytes,
        Guid releaseId,
        IClock clock,
        ILogger<IResourcePool> logger,
        IErrorNotificationService errorNotificationService) =>
        new FlexCelWorkbookPool(
            productContext,
            webFormAppType,
            workbookBytes,
            releaseId,
            clock,
            logger,
            errorNotificationService);

    /// <summary>
    /// Creates a new instance of the <see cref="FlexCelWorkbookPool"/> class and seeds it with a workbook.
    /// </summary>
    /// <param name="releaseContext">The release context the workbook is for.</param>
    /// <param name="webFormAppType">The app the workbook is for.</param>
    /// <param name="workbookBytes">The workbook file as bytes.</param>
    /// <param name="releaseId">The release ID.</param>
    /// <param name="seedWorkbook">A workbook to seed the pool with.</param>
    /// <param name="clock">It's a clock.</param>
    /// <param name="logger">Logger for logging.</param>
    /// <returns>A new instance of the <see cref="FlexCelWorkbookPool"/> class.</returns>
    public static FlexCelWorkbookPool CreatePoolWithSeed(
        ReleaseContext releaseContext,
        WebFormAppType webFormAppType,
        byte[] workbookBytes,
        Guid releaseId,
        IFlexCelWorkbook seedWorkbook,
        IClock clock,
        ILogger<IResourcePool> logger,
        IErrorNotificationService errorNotificationService) =>
        new FlexCelWorkbookPool(
            releaseContext,
            webFormAppType,
            workbookBytes,
            releaseId,
            clock,
            logger,
            errorNotificationService,
            seedWorkbook);

    /// <summary>
    /// Gets the number of bytes of memory used by this resource pool.
    /// </summary>
    /// <returns>The number of bytes used.</returns>
    public int GetMemoryUsedBytes()
    {
        return this.workbookBytes.Length * this.GetResourceCount();
    }

    /// <summary>
    /// Gets a string that identifies this resource pool for debugging purposes.
    /// </summary>
    /// <returns>A string that identifies this resource pool for debugging purposes.</returns>
    public override string GetDebugIdentifier()
    {
        return $"{this.GetType().Name} for tenant {this.ReleaseContext.TenantId}, product {this.ReleaseContext.ProductId}, environment {this.ReleaseContext.Environment}, release {this.ReleaseId} with a {this.webFormAppType} workbook";
    }

    /// <inheritdoc/>
    protected override IResourcePoolMember CreateResource()
    {
        using (MiniProfiler.Current.Step(nameof(FlexCelWorkbookPool) + "." + nameof(this.CreateResource)))
        {
            try
            {
                var flexCelWorkbook = new FlexCelWorkbook(this.workbookBytes, this.webFormAppType, this.Clock);
                if (flexCelWorkbook == null)
                {
                    var resourceDetails = this.GetResourceDetails();
                    this.LogCreationFailure(resourceDetails);
                    var exception = new ErrorException(
                        Errors.ResourcePool.FailedToCreateResource(
                            this.ReleaseId, this.webFormAppType, resourceDetails));
                    this.errorNotificationService.CaptureSentryException(
                        exception,
                        null,
                        resourceDetails);
                    throw exception;
                }

                return flexCelWorkbook;
            }
            catch (Exception ex)
            {
                var resourceDetails = this.GetResourceDetails();
                this.LogCreationFailure(resourceDetails, ex);
                var exception = new ErrorException(
                    Errors.ResourcePool.FailedToCreateResource(
                        this.ReleaseId, this.webFormAppType, resourceDetails), ex);
                this.errorNotificationService.CaptureSentryException(
                    exception,
                    null,
                    resourceDetails);
                throw exception;
            }
        }
    }

    private void LogCreationFailure(List<string> resourceDetails, Exception? exception = null)
    {
        this.Logger.LogInformation(string.Join(Environment.NewLine, resourceDetails));
        var errorMessage = $"Failed to create resource in {nameof(FlexCelWorkbookPool)}.{nameof(this.CreateResource)}";
        if (exception != null)
        {
            errorMessage += $": {exception.Message}";
        }

        this.Logger.LogError(errorMessage);
    }

    private List<string> GetResourceDetails()
    {
        var availableMemory = Helpers.MemoryHelper.FormatMemorySize(Helpers.MemoryHelper.GetAvailablePhysicalMemory());
        var usedMemory = Helpers.MemoryHelper.FormatMemorySize((ulong)this.GetMemoryUsedBytes());
        var resourceDetails = new List<string>
            {
                $"Tenant: {this.ReleaseContext.TenantId}",
                $"Product: {this.ReleaseContext.ProductId}",
                $"Environment: {this.ReleaseContext.Environment}",
                $"Pool: {this.GetDebugIdentifier()}",
                $"Resource Count: {this.GetResourceCount()}",
                $"Memory Used: {usedMemory}",
                $"Available Memory: {availableMemory}",
            };
        return resourceDetails;
    }
}