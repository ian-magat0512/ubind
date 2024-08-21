// <copyright file="DeletedPolicyClaimsActionType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Enums;

/// <summary>
/// Enumeration of the actions to be performed on claims associated with a deleted policy.
/// </summary>
public enum DeletedPolicyClaimsActionType
{
    /// <summary>
    /// Throw an error if associated claim/s are found.
    /// </summary>
    Error,

    /// <summary>
    /// Disassociate claim/s from the policy.
    /// </summary>
    Disassociate,

    /// <summary>
    /// Delete the associated claim/s.
    /// </summary>
    Delete,
}
