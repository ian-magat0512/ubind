// <copyright file="ImportDelimiterSeparatedValuesCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.GlassGuide;

using System;
using UBind.Domain.Patterns.Cqrs;

/// <summary>
/// Represents the command to import delimiter separated values files into Glass's Guide database.
/// </summary>
public class ImportDelimiterSeparatedValuesCommand : ICommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImportDelimiterSeparatedValuesCommand"/> class.
    /// </summary>
    /// <param name="jobId">The updater job id.</param>
    public ImportDelimiterSeparatedValuesCommand(Guid jobId)
    {
        this.UpdaterJobId = jobId;
    }

    /// <summary>
    /// Gets the updater job id .
    /// </summary>
    public Guid UpdaterJobId { get; }
}