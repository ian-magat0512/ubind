// <copyright file="SendEmailActionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Email;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Services;

    /// <summary>
    /// Model for send email action.
    /// </summary>
    public class SendEmailActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        [JsonConstructor]
        public SendEmailActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IEnumerable<ErrorConditionConfigModel> beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel> afterRunErrorConditions,
            IEnumerable<IBuilder<Action>> onErrorActions,
            IBuilder<IProvider<Data<string>>> outboundEmailServerAlias,
            EmailConfigurationConfigModel email,
            IEnumerable<IBuilder<IProvider<Data<string>>>> tags,
            IEnumerable<RelationshipConfigModel> relationships)
        : base(
              name,
              alias,
              description,
              asynchronous,
              runCondition,
              beforeRunErrorConditions,
              afterRunErrorConditions,
              onErrorActions)
        {
            email.ThrowIfArgumentNull(nameof(email));
            this.OutboundEmailServerAlias = outboundEmailServerAlias;
            this.Email = email;
            this.Tags = tags ?? Enumerable.Empty<IBuilder<IProvider<Data<string>>>>();
            this.Relationships = relationships;
        }

        /// <summary>
        /// Gets the outbound server alias.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> OutboundEmailServerAlias { get; private set; }

        /// <summary>
        /// Gets the email that the action will send.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public EmailConfigurationConfigModel Email { get; private set; }

        /// <summary>
        /// Gets a collection representing the list of tags to be attached to email entity.
        /// </summary>
        public IEnumerable<IBuilder<IProvider<Data<string>>>> Tags { get; private set; } = Enumerable.Empty<IBuilder<IProvider<Data<string>>>>();

        /// <summary>
        /// Gets a collection representing the list of relationships to be attached to email entity.
        /// </summary>
        public IEnumerable<RelationshipConfigModel> Relationships { get; private set; } = Enumerable.Empty<RelationshipConfigModel>();

        /// <inheritdoc/>
        public override Action Build(IServiceProvider dependencyProvider)
        {
            var beforeRunConditions = this.BeforeRunErrorConditions?.Select(br => br.Build(dependencyProvider));
            var afterRunConditions = this.AfterRunErrorConditions?.Select(ar => ar.Build(dependencyProvider));
            var errorActions = this.OnErrorActions?.Select(oa => oa.Build(dependencyProvider));
            var messageService = dependencyProvider.GetService<IMessagingService>();
            var tags = this.Tags?.Select(oa => oa.Build(dependencyProvider)).ToList();
            var relationships = this.Relationships?.Select(r => r.Build(dependencyProvider)).ToList();

            var emailService = dependencyProvider.GetService<IEmailService>();
            var organisationService = dependencyProvider.GetService<IOrganisationService>();
            var cachingResolver = dependencyProvider.GetService<ICachingResolver>();
            var clock = dependencyProvider.GetRequiredService<IClock>();
            var logging = dependencyProvider.GetRequiredService<ILogger<SendEmailAction>>();
            return new SendEmailAction(
                messageService,
                this.Name,
                this.Alias,
                this.Description,
                this.Asynchronous,
                this.RunCondition?.Build(dependencyProvider),
                beforeRunConditions,
                afterRunConditions,
                errorActions,
                this.OutboundEmailServerAlias?.Build(dependencyProvider),
                this.Email.Build(dependencyProvider),
                tags,
                relationships,
                emailService,
                cachingResolver,
                logging,
                clock);
        }
    }
}
