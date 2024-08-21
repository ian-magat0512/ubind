// <copyright file="RawColumnsRecode.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob.ColumnMaps;

/// <summary>
/// Represents the 0-based column mapping of Glass's Guide's REC data component files.
/// </summary>
public enum RawColumnsRecode
{
    OldCode = 0,
    NewCode,
    Date,
    Nvic,
    Segment,
}