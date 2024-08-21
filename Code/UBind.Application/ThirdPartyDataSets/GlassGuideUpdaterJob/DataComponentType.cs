// <copyright file="DataComponentType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;

public enum DataComponentType
{
    /// <summary>
    /// The data segment file for basic vehicle information as well as the
    /// new vehicle price.
    /// </summary>
    N12,

    /// <summary>
    /// The data segment file for basic vehicle information as well as the
    /// used vehicle value.
    /// </summary>
    U12,

    /// <summary>
    /// The data segment file for "recode" change-log of vehicle information.
    /// </summary>
    Rec,

    /// <summary>
    /// The data segment file for the non-truncated values for the "body" field
    /// in the N12 and U12 datafiles.
    /// </summary>
    Bdy,

    /// <summary>
    /// The data segment file for the non-truncated values for the "engine" field
    /// in the N12 and U12 datafiles.
    /// </summary>
    Eng,

    /// <summary>
    /// The data segment file for the non-truncated values for the "make" field
    /// in the N12 and U12 datafiles.
    /// </summary>
    Mak,

    /// <summary>
    /// The data segment file for the non-truncated values for the "transmission" field
    /// in the N12 and U12 datafiles.
    /// </summary>
    Trn,
}
