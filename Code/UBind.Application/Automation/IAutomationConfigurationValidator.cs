// <copyright file="IAutomationConfigurationValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    /// <summary>
    /// Validates automation configuration as best it can at sync time.
    /// Note that some checks will always need to be runtime checks as they depend on
    /// dynamic data / state.
    /// </summary>
    public interface IAutomationConfigurationValidator
    {
        void Validate(AutomationsConfigurationModel configModel);
    }
}
