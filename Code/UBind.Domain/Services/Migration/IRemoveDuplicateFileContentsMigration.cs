// <copyright file="IRemoveDuplicateFileContentsMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services.Migration;

/// <summary>
/// This migration will remove these duplicates and update other tables references to the correct file contents.
/// Some code introduced duplicate file contents in the database.
/// </summary>
public interface IRemoveDuplicateFileContentsMigration
{
    Task ProcessRemovingDuplicateFileContents(CancellationToken cancellationToken);
}
