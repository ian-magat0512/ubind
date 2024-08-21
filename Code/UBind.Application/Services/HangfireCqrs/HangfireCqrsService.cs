// <copyright file="HangfireCqrsService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.HangfireCqrs
{
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;

    /// <inheritdoc/>
    public class HangfireCqrsService : IHangfireCqrsService
    {
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireCqrsService"/> class.
        /// </summary>
        /// <param name="mediator">The mediator service.</param>
        public HangfireCqrsService(ICqrsMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <inheritdoc/>
        public async Task Send(ICommand requestCommand, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.mediator.Send(requestCommand, cancellationToken);
        }

        /// <inheritdoc/>
        [DisplayName("{0}")]
        public async Task Send(string jobName, ICommand requestCommand, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.mediator.Send(requestCommand, cancellationToken);
        }
    }
}
