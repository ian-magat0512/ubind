﻿// <copyright file="UpdateUserCustomerIdAndEnvironmentCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for updating user customer Id and environment.
    /// </summary>
    public class UpdateUserCustomerIdAndEnvironmentCommand : ICommand<Unit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateUserCustomerIdAndEnvironmentCommand"/> class.
        /// </summary>
        public UpdateUserCustomerIdAndEnvironmentCommand()
        {
        }
    }
}
