// <copyright file="InvitationType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

/// <summary>
/// Class that contains identifier for invitation type.
/// </summary>
public class InvitationType
{
    /// <summary>
    /// Gets the empty value (no activation nor password reset invitations).
    /// </summary>
    public static string Empty { get; } = "Empty";

    /// <summary>
    /// Gets the activation value.
    /// </summary>
    public static string Activation { get; } = "Activation";

    /// <summary>
    /// Gets the password reset value.
    /// </summary>
    public static string PasswordReset { get; } = "Password Reset";
}
