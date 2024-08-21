// <copyright file="FundingServiceName.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Funding;

/// <summary>
/// Enum of supported payment gateways.
/// </summary>
public enum FundingServiceName
{
    /// <summary>
    /// Default value.
    /// </summary>
    None = 0,

    /// <summary>
    /// Iqumulate Premium Funding Online.
    /// </summary>
    Iqumulate,

    /// <summary>
    /// eFundExpress.
    /// </summary>
    EFundExpress,

    /// <summary>
    /// Premium Funding.
    /// </summary>
    PremiumFunding,

    /// <summary>
    /// IQumulate.
    /// </summary>
    IQumulate,

    /// <summary>
    /// Red Planet Premium Funding
    /// </summary>
    RedPlanet,
}
