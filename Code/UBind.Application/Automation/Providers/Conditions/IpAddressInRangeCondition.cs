// <copyright file="IpAddressInRangeCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System.Net;
    using MorseCode.ITask;
    using NetTools;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Evaluates if a given IP address falls within a specified IP address range.
    /// </summary>
    public class IpAddressInRangeCondition : IProvider<Data<bool>>
    {
        private readonly IProvider<Data<string>> ipAddressProvider;
        private readonly IProvider<Data<string>> isInRangeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="IpAddressInRangeCondition"/> class.
        /// </summary>
        /// <param name="ipAddressProvider">The text value that contains ip address that will be tested.</param>
        /// <param name="isInRangeProvider">The text value that contains valid ip-address block definition.</param>
        /// <param name="logLevelProvider">The int value that should work as it does for other providers.</param>
        public IpAddressInRangeCondition(
            IProvider<Data<string>> ipAddressProvider,
            IProvider<Data<string>> isInRangeProvider)
        {
            this.ipAddressProvider = ipAddressProvider;
            this.isInRangeProvider = isInRangeProvider;
        }

        public string SchemaReferenceKey => "ipAddressInRangeCondition";

        public async ITask<IProviderResult<Data<bool>>> Resolve(IProviderContext providerContext)
        {
            var ipAddress = (await this.ipAddressProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var isInRange = (await this.isInRangeProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            IPAddress parsedIpAddress;
            if (!IPAddress.TryParse(ipAddress, out parsedIpAddress))
            {
                throw new ErrorException(
                    Domain.Errors.Automation.IpAddressInRangeCondition
                        .IpAddressFormatError(ipAddress));
            }

            return ProviderResult<Data<bool>>.Success(ParseIpAddressRangeOrThrow(isInRange).Contains(parsedIpAddress));
        }

        private static IPAddressRange ParseIpAddressRangeOrThrow(string ipAddressRange)
        {
            string[] parts = ipAddressRange.Split('/');
            bool isValidIpAddress = IPAddress.TryParse(parts[0], out IPAddress ipAddress);
            if (!isValidIpAddress || parts.Length != 2)
            {
                throw new ErrorException(
                    Domain.Errors.Automation.IpAddressInRangeCondition
                        .IpAddressRangeFormatError(parts[0]));
            }

            bool isValidSubnetMask = int.TryParse(parts[1], out int prefixLength);
            if (!isValidSubnetMask || prefixLength < 0 || prefixLength > 32)
            {
                throw new ErrorException(
                    Domain.Errors.Automation.IpAddressInRangeCondition
                        .SubnetMaskInvalid(parts[1]));
            }

            return new IPAddressRange(ipAddress, prefixLength);
        }
    }
}
