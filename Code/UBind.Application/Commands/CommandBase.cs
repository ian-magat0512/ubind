// <copyright file="CommandBase.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands
{
    using System;
    using NodaTime;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// A base class to inherit from with default parameters.
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBase"/> class.
        /// </summary>
        /// <param name="performingUserId">ID of the user.</param>
        /// <param name="createdTimestamp">Creation instance.</param>
        protected CommandBase(Guid? performingUserId, Instant createdTimestamp)
        {
            this.PerformingUserId = performingUserId;
            this.CreatedTimestamp = createdTimestamp;
        }

        /// <summary>
        /// Gets the ID of the user.
        /// </summary>
        public Guid? PerformingUserId { get; }

        /// <summary>
        /// Gets the created timestamp.
        /// </summary>
        public Instant CreatedTimestamp { get; }
    }
}
