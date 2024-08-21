// <copyright file="SaveSmsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Sms
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ValueTypes;

    public class SaveSmsCommand : ICommand
    {
        public SaveSmsCommand(
            Guid tenantId,
            List<PhoneNumber> to,
            PhoneNumber from,
            string message,
            Guid productId,
            Guid organisationId,
            DeploymentEnvironment environment,
            List<string> tags,
            List<Automation.Entities.Relationship> relationships)
        {
            this.To = to;
            this.From = from;
            this.Message = message;
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.OrganisationId = organisationId;
            this.Environment = environment;
            this.Tags = tags;
            this.Relationships = relationships;
        }

        public List<PhoneNumber> To { get; }

        public PhoneNumber From { get; }

        public string Message { get; }

        public Guid TenantId { get; }

        public Guid ProductId { get; }

        public Guid OrganisationId { get; }

        public DeploymentEnvironment Environment { get; }

        public List<string> Tags { get; }

        public List<Automation.Entities.Relationship> Relationships { get; }
    }
}
