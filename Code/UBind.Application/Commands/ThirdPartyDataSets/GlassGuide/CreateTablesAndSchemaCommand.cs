// <copyright file="CreateTablesAndSchemaCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.GlassGuide;

using UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;
using UBind.Domain.Patterns.Cqrs;

/// <summary>
/// Represents the command to create tables and schema to be used by Glass's Guide.
/// </summary>
public class CreateTablesAndSchemaCommand : ICommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTablesAndSchemaCommand"/> class.
    /// </summary>
    /// <param name="glassGuideSchema">The Glass's Guide schema.</param>
    public CreateTablesAndSchemaCommand(Schema glassGuideSchema)
    {
        this.GlassGuideSchema = glassGuideSchema;
    }

    /// <summary>
    /// Gets the Glass's Guide schema name.
    /// </summary>
    public Schema GlassGuideSchema { get; }
}