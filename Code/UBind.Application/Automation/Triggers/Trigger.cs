// <copyright file="Trigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using System.Threading.Tasks;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents an automation trigger.
    /// </summary>
    public abstract class Trigger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Trigger"/> class.
        /// </summary>
        /// <param name="name">The name of the trigger.</param>
        /// <param name="alias">The alias of thr trigger.</param>
        /// <param name="description">The description for the trigger, if any.</param>
        protected Trigger(string name, string alias, string description)
        {
            this.Name = name;
            this.Alias = alias;
            this.Description = description;
        }

        /// <summary>
        /// Gets the name of the trigger.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the alias of the trigger.
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// Gets the description of the trigger.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Verifies if this trigger matches with the trigger request in the data context.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <returns>Returns true if the trigger matches, otherwise false.</returns>
        public abstract Task<bool> DoesMatch(AutomationData dataContext);

        /// <summary>
        /// Generates completion responses for specific triggers.
        /// </summary>
        public virtual Task GenerateCompletionResponse(IProviderContext providerContext)
        {
            return Task.CompletedTask;
        }
    }
}
