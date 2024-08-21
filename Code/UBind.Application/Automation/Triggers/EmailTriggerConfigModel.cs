// <copyright file="EmailTriggerConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Email;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Model for creating an email trigger.
    /// </summary>
    public class EmailTriggerConfigModel : IBuilder<Trigger>
    {
        [JsonConstructor]
        public EmailTriggerConfigModel(
            string name,
            string alias,
            string description,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IBuilder<IProvider<Data<string>>> context,
            EmailAccount emailAccount,
            EmailConfigurationConfigModel replyEmail)
        {
            this.Name = name;
            this.Alias = alias;
            this.Description = description;
            this.RunCondition = runCondition;
            this.Context = context;
            this.EmailAccount = emailAccount;
            this.ReplyEmail = replyEmail;
        }

        /// <summary>
        /// Gets the name of the trigger.
        /// </summary>
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the unique trigger ID or alias.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Alias { get; private set; }

        /// <summary>
        /// Gets the description of the trigger.
        /// </summary>
        [JsonProperty]
        public string Description { get; private set; }

        /// <summary>
        /// Gets the run condition.
        /// </summary>
        public IBuilder<IProvider<Data<bool>>> RunCondition { get; private set; }

        /// <summary>
        /// Gets an optional property that contains a list of references to be used as the automation context.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<string>>> Context { get; private set; }

        /// <summary>
        /// Gets the settings to be used for the incoming mail server to be polled.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public EmailAccount EmailAccount { get; private set; }

        /// <summary>
        /// Gets the reply email to be sent.
        /// </summary>
        [JsonProperty]
        public EmailConfigurationConfigModel ReplyEmail { get; private set; }

        /// <summary>
        /// Gets the custom outbound server to be used to send the reply email, otherwise otherwise the default SMTP server will be used .
        /// </summary>
        /// <remarks>TODO: Clarify what is the default SMTP server to be used, ie localhost, etc.</remarks>
        [JsonProperty]
        public IBuilder<IProvider<Data<string>>> OutboundEmailServerAlias { get; private set; }

        /// <inheritdoc/>
        public Trigger Build(IServiceProvider dependencyProvider)
        {
            return new EmailTrigger(
                this.Name,
                this.Alias,
                this.Description,
                this.RunCondition?.Build(dependencyProvider),
                this.Context?.Build(dependencyProvider),
                this.EmailAccount,
                this.ReplyEmail?.Build(dependencyProvider),
                this.OutboundEmailServerAlias?.Build(dependencyProvider));
        }
    }
}
