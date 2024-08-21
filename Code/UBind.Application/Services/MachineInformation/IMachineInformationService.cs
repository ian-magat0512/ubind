// <copyright file="IMachineInformationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.MachineInformation
{
    /// <summary>
    /// Interface for getting the information of the machine.
    /// </summary>
    public interface IMachineInformationService
    {
        /// <summary>
        /// Get the hostname of the machine.
        /// </summary>
        /// <returns>hostname.</returns>
        string GetHostname();

        /// <summary>
        /// Get the IP address of the machine.
        /// </summary>
        /// <returns>ip address.</returns>
        string GetIPAddress();

        string GetServerDetails();
    }
}
