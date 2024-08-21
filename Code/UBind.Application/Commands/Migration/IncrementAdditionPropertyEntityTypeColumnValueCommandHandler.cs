// <copyright file="IncrementAdditionPropertyEntityTypeColumnValueCommandHandler.cs" company="uBind">
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
    using UBind.Domain.ReadModel;

    public class IncrementAdditionPropertyEntityTypeColumnValueCommandHandler : ICommandHandler<IncrementAdditionPropertyEntityTypeColumnValueCommand, Unit>
    {
        private readonly ITextAdditionalPropertyValueReadModelRepository textAdditionalPropertyValueReadModelRepository;

        public IncrementAdditionPropertyEntityTypeColumnValueCommandHandler(
             ITextAdditionalPropertyValueReadModelRepository textAdditionalPropertyValueReadModelRepository)
        {
            this.textAdditionalPropertyValueReadModelRepository = textAdditionalPropertyValueReadModelRepository;
        }

        public Task<Unit> Handle(IncrementAdditionPropertyEntityTypeColumnValueCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.textAdditionalPropertyValueReadModelRepository.IncrementAdditionalPropertyEntityTypeColumnValueMigration();
            return Task.FromResult(Unit.Value);
        }
    }
}
