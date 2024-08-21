// <copyright file="RemoveFilesEntityCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Drop files table.
    /// </summary>
    public class RemoveFilesEntityCommandHandler : ICommandHandler<RemoveFilesEntityCommand, Unit>
    {
        private readonly IUBindDbContext dbContext;

        public RemoveFilesEntityCommandHandler(
            IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<Unit> Handle(RemoveFilesEntityCommand request, CancellationToken cancellationToken)
        {
            this.dbContext.ExecuteSqlScript("Drop Table dbo.Files");
            return Task.FromResult(Unit.Value);
        }
    }
}
