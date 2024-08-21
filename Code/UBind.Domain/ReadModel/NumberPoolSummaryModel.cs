// <copyright file="NumberPoolSummaryModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel;

/// <summary>
/// Model for a number pool summary.
/// </summary>
public class NumberPoolSummaryModel
{
    public NumberPoolSummaryModel(
        Guid productId,
        DeploymentEnvironment deploymentEnvironment,
        IEnumerable<string> numbers)
    {
        this.ProductId = productId;
        this.Environment = deploymentEnvironment;
        this.Numbers = numbers;
    }

    public Guid ProductId { get; set; }

    public DeploymentEnvironment Environment { get; set; }

    public IEnumerable<string> Numbers { get; set; }
}
