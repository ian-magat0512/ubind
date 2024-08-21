// <copyright file="ICqrsMediator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Patterns.Cqrs
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;

    /// <summary>
    /// Provides the service contract to be used by CQRS and Mediator service to runs a command request with retry feature.
    /// </summary>
    public interface ICqrsMediator : IPublisher
    {
        /// <summary>
        /// Asynchronously send and execute command requests.
        /// </summary>
        /// <typeparam name="TResponse">Response type.</typeparam>
        /// <param name="command">The command to be executed.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task that represents the send operation. The task result contains the handler response.</returns>
        Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously send and execute command requests.
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task that represents the send operation. The task result contains the handler response.</returns>
        Task Send(ICommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously send and execute command requests.
        /// </summary>
        /// <typeparam name="TResponse">Response type.</typeparam>
        /// <param name="query">The query to be executed.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task that represents the send operation. The task result contains the handler response.</returns>
        Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
    }
}
