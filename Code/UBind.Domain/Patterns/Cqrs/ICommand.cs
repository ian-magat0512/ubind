// <copyright file="ICommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Patterns.Cqrs
{
    using MediatR;

    /// <summary>
    /// Marker interface to represent a command request with a response.
    /// </summary>
    /// <typeparam name="T">The Response type .</typeparam>
    public interface ICommand<T> : IRequest<T>, ICqrsCommand
    {
    }

    /// <summary>
    /// Marker interface to represent a command request with no response.
    /// </summary>
    public interface ICommand : ICommand<Unit>
    {
    }
}
