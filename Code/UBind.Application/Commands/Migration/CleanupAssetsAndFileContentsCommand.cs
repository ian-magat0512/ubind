// <copyright file="CleanupAssetsAndFileContentsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for removing Assets.Content column and removing orphaned FileContents rows.
    /// This is needed to permanently remove identical Assets.Content
    /// and also to remove orphaned FileContent rows.
    /// </summary>
    public class CleanupAssetsAndFileContentsCommand : ICommand
    {
    }
}
