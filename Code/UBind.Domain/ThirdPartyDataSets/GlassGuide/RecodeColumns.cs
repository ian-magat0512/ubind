// <copyright file="RecodeColumns.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ThirdPartyDataSets.GlassGuide;

/// <summary>
/// Represents the 0-based column mapping of Glass's Guide's REC segment.
/// </summary>
public enum RecodeColumns
{
    OldCode = 0,
    NewCode,
    Date,
    Nvic,
    Segment,
}
