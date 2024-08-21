// <copyright file="AttachDocumentsToQuoteCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Document
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    public class AttachDocumentsToQuoteCommandHandler : ICommandHandler<AttachDocumentsToQuoteCommand, Unit>
    {
        private readonly IAggregateLockingService aggregateLockingService;
        private readonly IApplicationDocumentService applicationDocumentService;

        public AttachDocumentsToQuoteCommandHandler(
            IApplicationDocumentService applicationDocumentService,
            IAggregateLockingService aggregateLockingService)
        {
            this.aggregateLockingService = aggregateLockingService;
            this.applicationDocumentService = applicationDocumentService;
        }

        public async Task<Unit> Handle(AttachDocumentsToQuoteCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (await this.aggregateLockingService.CreateLockOrThrow(command.ApplicationEvent.Aggregate.TenantId, command.ApplicationEvent.Aggregate.Id, AggregateType.Quote))
            {
                await this.applicationDocumentService.AttachDocumentsAsync(command.ApplicationEvent, command.Attachments.ToArray());
            }
            return Unit.Value;
        }
    }
}
