// <copyright file="CreateDkimSettingsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.DkimSettings
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Entities;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for creating DKIM settings.
    /// </summary>
    public class CreateDkimSettingsCommand : ICommand<DkimSettings>
    {
        public CreateDkimSettingsCommand(
            Guid tenantId,
            Guid organisationId,
            string domainName,
            string privateKey,
            string dnsSelector,
            string agentOrUserIdentifier,
            List<string> applicableDomainNames)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.DomainName = domainName;
            this.PrivateKey = privateKey;
            this.DnsSelector = dnsSelector;
            this.ApplicableDomainNameList = applicableDomainNames;
            this.AgentOrUserIdentifier = agentOrUserIdentifier;
        }

        /// <summary>
        /// Gets the tenant Id.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the organisation Id.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the domain name.
        /// </summary>
        public string DomainName { get; private set; }

        /// <summary>
        /// Gets the private key.
        /// </summary>
        public string PrivateKey { get; private set; }

        /// <summary>
        /// Gets the DNS selector.
        /// </summary>
        public string DnsSelector { get; private set; }

        /// <summary>
        /// Gets the agent or user identifier.
        /// </summary>
        public string AgentOrUserIdentifier { get; private set; }

        /// <summary>
        /// Gets the applicable domain name list.
        /// </summary>
        public List<string> ApplicableDomainNameList { get; private set; }
    }
}
