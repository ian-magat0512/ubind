// <copyright file="ICqrsRequestContext.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Patterns.Cqrs
{
    using UBind.Domain.Repositories;

    /// <summary>
    /// Maintains contextual information about the executing command.
    /// </summary>
    public interface ICqrsRequestContext
    {
        /// <summary>
        /// Gets or sets a value indicating whether a new scope has been created for a CqrsQuery or CqrsCommand.
        /// If it has then it will be re-used instead of creating a new scope.
        /// </summary>
        bool RequestScopeCreated { get; set; }

        /// <summary>
        /// Gets or sets the intent for this request (read or write).
        /// Allows you to set the intent of the current request or procedure to either read-only, or read-write,
        /// so that requests can be routed to the relevant database instance, for performance and spreading
        /// load.
        /// You can call this explicitly to ensure that the correct application intent is set before executing
        /// a request. This must be called before an instance of the UBindDbContext is created. If it's not
        /// been called, and CqrsMediator is not being used, then an exception will be generated.
        /// </summary>
        RequestIntent? RequestIntent { get; set; }

        /// <summary>
        /// Gets or sets the created db context, if one has actually been created.
        /// This is set by the framework so that we know if one has been created, so that if someone
        /// tries to change the RequestIntent after it's been created, we can generate an error.
        /// It's also used to check if there's an existing transaction in place before creating a new
        /// Dependency Injection scope, because if we create a new DI Scope (and therefore a new instance of
        /// IUBindDbContext) when we're already within a transaction, we'll get hard failure and exception.
        /// </summary>
        IUBindDbContext DbContext { get; set; }
    }
}
