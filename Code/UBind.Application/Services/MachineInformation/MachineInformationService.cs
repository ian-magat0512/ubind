// <copyright file="MachineInformationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.MachineInformation
{
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using UBind.Application.Services.Imports;

    /// <inheritdoc/>
    public class MachineInformationService : IMachineInformationService
    {
        private readonly IInternalUrlConfiguration internalUrlConfig;

        public MachineInformationService(
            IInternalUrlConfiguration internalUrlConfig)
        {
            this.internalUrlConfig = internalUrlConfig;
        }

        /// <inheritdoc/>
        public string GetHostname()
        {
            return Dns.GetHostName();
        }

        /// <inheritdoc/>
        public string GetIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            return ipAddress == null ? string.Empty : ipAddress.ToString();
        }

        public string GetServerDetails()
        {
            var details = new StringBuilder();
            details.AppendLine($"Server Details:");
            details.AppendLine($"Hostname: {this.GetHostname()}");
            details.AppendLine($"IP Address: {this.GetIPAddress()}");
            details.AppendLine($"Base Url: {this.internalUrlConfig.BaseApi}");
            return details.ToString();
        }
    }
}
