// <copyright file="AssignNewUserTypeToUserReadModelsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Assign new user type to user read model ( from ClientAdmin/Agent User Type to Client,
    /// from UbindAdmin to Master)
    /// This migration is needed when implementing changes for UB-4685 refactor on older user records.
    /// </summary>
    public class AssignNewUserTypeToUserReadModelsCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssignNewUserTypeToUserReadModelsCommand"/> class.
        /// </summary>
        public AssignNewUserTypeToUserReadModelsCommand()
        {
        }
    }
}
