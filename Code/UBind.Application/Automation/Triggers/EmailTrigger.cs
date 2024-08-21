// <copyright file="EmailTrigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using System.Threading.Tasks;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Email;
    using UBind.Application.Automation.Providers;

    /// <summary>
    ///  A trigger that is invoked when an email arrives in a specific email account.
    /// </summary>
    public class EmailTrigger : ConditionalTrigger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailTrigger"/> class.
        /// </summary>
        /// <param name="name">The name of the trigger.</param>
        /// <param name="alias">The alias of the trigger.</param>
        /// <param name="description">The trigger description.</param>
        /// <param name="context">The automation data context to be used.</param>
        /// <param name="emailAccount">The email account to be used.</param>
        /// <param name="email">The reply email to be sent.</param>
        /// <param name="outboundEmailServerAlias">The outbound server alias, if existing.</param>
        public EmailTrigger(
            string name,
            string alias,
            string description,
            IProvider<Data<bool>> runCondition,
            IProvider<Data<string>> context,
            EmailAccount emailAccount,
            EmailConfiguration email,
            IProvider<Data<string>> outboundEmailServerAlias)
            : base(name, alias, description, runCondition)
        {
            this.Context = context;
            this.EmailAccount = emailAccount;
            this.ReplyEmail = email;
            this.OutboundEmailServerAlias = outboundEmailServerAlias;
        }

        /// <summary>
        /// Gets the context of the trigger.
        /// </summary>
        public IProvider<Data<string>> Context { get; }

        /// <summary>
        /// Gets the settings for the incoming mail server that will be polled.
        /// </summary>
        public EmailAccount EmailAccount { get; }

        /// <summary>
        /// Gets the reply email that will be sent.
        /// </summary>
        public EmailConfiguration ReplyEmail { get; }

        /// <summary>
        /// Gets the outbound email server alias of the trigger.
        /// </summary>
        public IProvider<Data<string>> OutboundEmailServerAlias { get; }

        /// <inheritdoc/>
        public override Task<bool> DoesMatch(AutomationData automationData)
        {
            // TODO
            throw new System.NotImplementedException();
        }
    }
}
