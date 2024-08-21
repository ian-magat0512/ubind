// <copyright file="CreateTablesAndSchemaCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.GlassGuide;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UBind.Application.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;

/// <summary>
/// Represents the handler to create Glass's Guide tables and schema.
/// </summary>
public class CreateTablesAndSchemaCommandHandler : ICommandHandler<CreateTablesAndSchemaCommand, Unit>
{
    private readonly IGlassGuideRepository glassGuideRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTablesAndSchemaCommandHandler"/> class.
    /// </summary>
    /// <param name="glassGuideRepository">The Glass's Guide repository.</param>
    public CreateTablesAndSchemaCommandHandler(IGlassGuideRepository glassGuideRepository)
    {
        this.glassGuideRepository = glassGuideRepository;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(CreateTablesAndSchemaCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var currentSuffix = await this.glassGuideRepository.GetExistingTableIndex() ?? "00";
        var rollingNumber = new RollingNumber(99, currentSuffix, "D2");
        string nextIndex = rollingNumber.GetNext();
        await this.glassGuideRepository.CreateGlassGuideTablesAndSchema(nextIndex);
        await this.glassGuideRepository.CreateForeignKeysAndIndexes(nextIndex);
        return await Task.FromResult(Unit.Value);
    }
}