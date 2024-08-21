// <copyright file="ReleaseDetailsChangeTracker.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories;

using UBind.Domain;

/// <summary>
/// Since we are not using entity framework, we need to track changes to the release details manually.
/// We will then use this to apply the changes to the database.
/// </summary>
public class ReleaseDetailsChangeTracker
{
    public ReleaseDetailsChangeTracker(WebFormAppType componentType)
    {
        this.ComponentType = componentType;
    }

    public WebFormAppType ComponentType { get; set; }

    public DevRelease DevRelease { get; set; }

    public ReleaseDetails ReleaseDetails { get; set; }

    public bool IsDevReleaseNew { get; set; }

    public bool IsReleaseDetailsNew { get; set; }

    public bool HasFormConfigurationJsonChanged { get; set; }

    public bool HasWorkflowJsonChanged { get; set; }

    public bool HasIntegrationsJsonChanged { get; set; }

    public bool HasAutomationsJsonChanged { get; set; }

    public bool HasPaymentJsonChanged { get; set; }

    public bool HasPaymentFormJsonChanged { get; set; }

    public bool HasFundingJsonChanged { get; set; }

    public bool HasProductJsonChanged { get; set; }

    public bool HasSpreadsheetChanged { get; set; }

    public List<Asset> AddedPrivateFiles { get; set; } = new List<Asset>();

    public List<Asset> RemovedPrivateFiles { get; set; } = new List<Asset>();

    public List<Asset> AddedPublicFiles { get; set; } = new List<Asset>();

    public List<Asset> RemovedPublicFiles { get; set; } = new List<Asset>();

    public bool HasPropertyChanges =>
        this.HasFormConfigurationJsonChanged ||
        this.HasWorkflowJsonChanged ||
        this.HasIntegrationsJsonChanged ||
        this.HasAutomationsJsonChanged ||
        this.HasPaymentJsonChanged ||
        this.HasPaymentFormJsonChanged ||
        this.HasFundingJsonChanged ||
        this.HasProductJsonChanged ||
        this.HasSpreadsheetChanged;

    public bool HasPrivateFileChanges =>
        this.AddedPrivateFiles.Any() ||
        this.RemovedPrivateFiles.Any();

    public bool HasPublicFileChanges =>
        this.AddedPublicFiles.Any() ||
        this.RemovedPublicFiles.Any();
}
