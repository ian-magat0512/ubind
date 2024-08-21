// <copyright file="InitialDataSeederCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    public class InitialDataSeederCommandHandler
        : ICommandHandler<InitialDataSeederCommand, Unit>
    {
        private readonly IInitialDataSeeder initialDataSeeder;

        public InitialDataSeederCommandHandler(
            IInitialDataSeeder initialDataSeeder)
        {
            this.initialDataSeeder = initialDataSeeder;
        }

        public async Task<Unit> Handle(
            InitialDataSeederCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.initialDataSeeder.Seed();

            return Unit.Value;
        }
    }
}
