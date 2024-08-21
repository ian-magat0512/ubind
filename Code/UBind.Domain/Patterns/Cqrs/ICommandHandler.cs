// <copyright file="ICommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Patterns.Cqrs
{
    using MediatR;

    /// <summary>
    /// Defines a handler for a command request.
    /// </summary>
    /// <typeparam name="TRequest">The type of command request being handled.</typeparam>
    /// <typeparam name="TResponse">The type of command response from the handler.</typeparam>
    public interface ICommandHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
    }

    /// <summary>
    /// Defines a handler for a command request.
    /// </summary>
    /// <typeparam name="TRequest">The type of command request being handled.</typeparam>
    public interface ICommandHandler<in TRequest> : ICommandHandler<TRequest, Unit>
        where TRequest : IRequest<Unit>
    {
    }
}
