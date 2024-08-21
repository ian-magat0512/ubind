// <copyright file="RemoveDuplicateFileContentsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration;

using UBind.Domain.Patterns.Cqrs;

/// <summary>
/// Command to remove duplicate file contents from the database.
/// Some code introduced duplicate file contents in the database.
/// This command will remove these duplicates and update references to the correct file contents.
/// </summary>
public class RemoveDuplicateFileContentsCommand : ICommand
{
}
