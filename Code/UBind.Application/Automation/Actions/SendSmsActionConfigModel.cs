// <copyright file="SendSmsActionConfigModel.cs" company="uBind">
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
    using UBind.Application.Automation.Sms;
    using UBind.Application.Sms;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;

    public class SendSmsActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        [JsonConstructor]
        public SendSmsActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IEnumerable<ErrorConditionConfigModel> beforeRunConditions,
            IEnumerable<ErrorConditionConfigModel> afterRunConditions,
            IEnumerable<IBuilder<Action>> onErrorActions,
            SmsConfigurationConfigModel sms,
            IEnumerable<IBuilder<IProvider<Data<string>>>> tags,
            IEnumerable<RelationshipConfigModel> relationships)
            : base(
                  name,
                  alias,
                  description,
                  asynchronous,
                  runCondition,
                  beforeRunConditions,
                  afterRunConditions,
                  onErrorActions)
        {
            sms.ThrowIfArgumentNull(nameof(sms));
            this.Sms = sms;
            this.Tags = tags;
            this.Relationships = relationships;
        }

        [JsonProperty(Required = Required.Always)]
        public SmsConfigurationConfigModel Sms { get; private set; }

        public IEnumerable<IBuilder<IProvider<Data<string>>>> Tags { get; private set; } = Enumerable.Empty<IBuilder<IProvider<Data<string>>>>();

        public IEnumerable<RelationshipConfigModel> Relationships { get; private set; } = Enumerable.Empty<RelationshipConfigModel>();

        public override Action Build(IServiceProvider dependencyProvider)
        {
            var smsClient = dependencyProvider.GetService<ISmsClient>();
            var cachingResolver = dependencyProvider.GetService<ICachingResolver>();
            var mediator = dependencyProvider.GetService<ICqrsMediator>();
            var tags = this.Tags?.Select(oa => oa.Build(dependencyProvider)).ToList();
            var relationships = this.Relationships?.Select(r => r.Build(dependencyProvider)).ToList();
            var clock = dependencyProvider.GetRequiredService<IClock>();
            var logging = dependencyProvider.GetRequiredService<ILogger<SendSmsAction>>();
            return new SendSmsAction(
                this.Name,
                this.Alias,
                this.Description,
                this.Asynchronous,
                this.RunCondition?.Build(dependencyProvider),
                this.BeforeRunErrorConditions?.Select(c => c.Build(dependencyProvider)),
                this.AfterRunErrorConditions?.Select(c => c.Build(dependencyProvider)),
                this.OnErrorActions?.Select(c => c.Build(dependencyProvider)),
                this.Sms.Build(dependencyProvider),
                tags,
                relationships,
                smsClient,
                cachingResolver,
                logging,
                mediator,
                clock);
        }
    }
}
