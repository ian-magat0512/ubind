// <copyright file="Schema.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;

using System.ComponentModel;

/// <summary>
/// Provides the applicable values for Glass's Guide schema.
/// </summary>
public enum Schema
{
    /// <summary>
    /// The Glass's Guide staging schema.
    /// </summary>
    [Description("GlassGuideStaging")]
    GlassGuideStaging,

    /// <summary>
    /// The Glass's Guide schema.
    /// </summary>
    [Description("GlassGuide")]
    GlassGuide,
}