// <copyright file="RemoveDuplicateFileContentsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration;

using MediatR;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Services.Migration;

/// <summary>
/// Command to remove duplicate file contents from the database.
/// Some code introduced duplicate file contents in the database.
/// This command will remove these duplicates and update references to the correct file contents.
/// </summary>
public class RemoveDuplicateFileContentsCommandHandler : ICommandHandler<RemoveDuplicateFileContentsCommand, Unit>
{
    private readonly IRemoveDuplicateFileContentsMigration removeDuplicateFileContentsMigration;

    public RemoveDuplicateFileContentsCommandHandler(IRemoveDuplicateFileContentsMigration removeDuplicateFileContentsMigration)
    {
        this.removeDuplicateFileContentsMigration = removeDuplicateFileContentsMigration;
    }

    public async Task<Unit> Handle(RemoveDuplicateFileContentsCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await this.removeDuplicateFileContentsMigration.ProcessRemovingDuplicateFileContents(cancellationToken);
        return Unit.Value;
    }
}
