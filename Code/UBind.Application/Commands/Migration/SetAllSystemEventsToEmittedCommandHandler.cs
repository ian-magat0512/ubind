// <copyright file="SetAllSystemEventsToEmittedCommandHandler.cs" company="uBind">
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

    public class SetAllSystemEventsToEmittedCommandHandler : ICommandHandler<SetAllSystemEventsToEmittedCommand, Unit>
    {
        private readonly IUBindDbContext dbContext;

        public SetAllSystemEventsToEmittedCommandHandler(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Unit> Handle(SetAllSystemEventsToEmittedCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int batchSize = 1000;
            string sql = $@"IF COL_LENGTH('SystemEvents', 'IsEmitted') IS NOT NULL
                BEGIN
                    EXEC('UPDATE TOP ({batchSize}) dbo.SystemEvents SET IsEmitted = 1 WHERE IsEmitted = 0');
                END";
            int affectedRows;
            do
            {
                affectedRows = this.dbContext.ExecuteSqlScript(sql);
                await Task.Delay(1000, cancellationToken);
            }
            while (affectedRows == batchSize);
            return Unit.Value;
        }
    }
}
