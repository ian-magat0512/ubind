﻿// <copyright file="DeleteDataTableDefinitionAndContentCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.DataTable
{
    using System;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Delete the data table definition entry and its associated database table.
    /// </summary>
    public class DeleteDataTableDefinitionAndContentCommand : ICommand<Unit>
    {
        public DeleteDataTableDefinitionAndContentCommand(Guid tenantId, Guid definitionId)
        {
            this.TenantId = tenantId;
            this.DefinitionId = definitionId;
        }

        public Guid TenantId { get; }

        public Guid DefinitionId { get; }
    }
}
