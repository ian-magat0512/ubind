// <copyright file="AttachFilesToEntityCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Document
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    /// <summary>
    /// Command handler for attaching files to an entity.
    /// </summary>
    public class AttachFilesToEntityCommandHandler : ICommandHandler<AttachFilesToEntityCommand, Unit>
    {
        private readonly IAggregateLockingService aggregateLockingService;

        public AttachFilesToEntityCommandHandler(
            IAggregateLockingService aggregateLockingService)
        {
            this.aggregateLockingService = aggregateLockingService;
        }

        public async Task<Unit> Handle(AttachFilesToEntityCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var documentAttacher = command.DocumentAttacher;
            var aggregateType = documentAttacher.GetAggregateType();
            if (aggregateType != AggregateType.Quote)
            {
                await documentAttacher.AttachFiles(command.TenantId, command.Attachments);
                return Unit.Value;
            }
            else
            {
                var quoteAggregateId = documentAttacher.GetAggregateId();
                using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, aggregateType))
                {
                    await documentAttacher.AttachFiles(command.TenantId, command.Attachments);
                }
                return Unit.Value;
            }
        }
    }
}
