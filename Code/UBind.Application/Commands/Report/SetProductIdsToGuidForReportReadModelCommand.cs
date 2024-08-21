// <copyright file="SetProductIdsToGuidForReportReadModelCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Report
{
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Sets product ids of report read models to Guid.
    /// </summary>
    public class SetProductIdsToGuidForReportReadModelCommand : ICommand<Unit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetProductIdsToGuidForReportReadModelCommand"/> class.
        /// </summary>
        public SetProductIdsToGuidForReportReadModelCommand()
        {
        }
    }
}
