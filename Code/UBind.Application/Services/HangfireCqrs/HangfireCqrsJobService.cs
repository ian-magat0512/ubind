// <copyright file="HangfireCqrsJobService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.HangfireCqrs
{
    using System.Threading;
    using Hangfire;
    using UBind.Domain.Patterns.Cqrs;

    /// <inheritdoc/>
    public class HangfireCqrsJobService : IHangfireCqrsJobService
    {
        private readonly IBackgroundJobClient backgroundJobClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireCqrsJobService"/> class.
        /// </summary>
        /// <param name="backgroundJobClient">The Hangfire background job client.</param>
        public HangfireCqrsJobService(IBackgroundJobClient backgroundJobClient)
        {
            this.backgroundJobClient = backgroundJobClient;
        }

        /// <inheritdoc/>
        public string EnqueueRequest(string hangFireJobName, ICommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return this.backgroundJobClient.Enqueue<IHangfireCqrsService>(hangfireMediator => hangfireMediator.Send(hangFireJobName, request, cancellationToken));
        }

        /// <inheritdoc/>
        public string EnqueueRequest(ICommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return this.backgroundJobClient.Enqueue<IHangfireCqrsService>(hangfireMediator => hangfireMediator.Send(request, cancellationToken));
        }
    }
}
