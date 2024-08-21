// <copyright file="AutomationsConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Triggers;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Represents the configuration for automations.
    /// </summary>
    public class AutomationsConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationsConfiguration"/> class.
        /// </summary>
        /// <param name="automations">An array of automations which can be run.</param>
        public AutomationsConfiguration(IEnumerable<Automation> automations)
        {
            this.Automations = automations;
        }

        /// <summary>
        /// Gets the automations list.
        /// </summary>
        public IEnumerable<Automation> Automations { get; }

        /// <summary>
        /// Retrieves an automation and it's trigger that most closely matches the trigger data in the automation context.
        /// </summary>
        /// <param name="dataContext">The automation data request.</param>
        /// <returns>A result containing the autmoation and the trigger that matched, or null if no match was found.</returns>
        public async Task<(Automation, HttpTrigger)> GetClosestMatchingHttpTrigger(AutomationData dataContext)
        {
            var automationTriggers = new List<(Automation, HttpTrigger)>();
            foreach (var automation in this.Automations)
            {
                var matchingTriggers = await automation.GetMatchingTriggers(dataContext);
                if (matchingTriggers == null || matchingTriggers.None())
                {
                    continue;
                }

                var closestMatchingTrigger = matchingTriggers
                    .OfType<HttpTrigger>()
                    .OrderByDescending(t => t.MatchScore)
                    .FirstOrDefault();
                if (closestMatchingTrigger != null)
                {
                    automationTriggers.Add((automation, closestMatchingTrigger));
                }
            }

            var bestMatch = automationTriggers
                .OrderByDescending(t => t.Item2.MatchScore)
                .FirstOrDefault();

            return bestMatch;
        }
    }
}
