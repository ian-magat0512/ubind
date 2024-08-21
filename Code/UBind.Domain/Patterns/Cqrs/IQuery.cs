// <copyright file="IQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Patterns.Cqrs
{
    using MediatR;

    /// <summary>
    /// Marker interface to represent a query request with a response.
    /// </summary>
    /// <typeparam name="TResponse">The Response type .</typeparam>
    public interface IQuery<TResponse> : IRequest<TResponse>, ICqrsQuery
    {
    }
}
