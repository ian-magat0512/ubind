// <copyright file="Trigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component
{
    using System.ComponentModel.DataAnnotations;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Represents the configuration for a trigger.
    /// A trigger when activated can cause a change to the workflow requiring, for example, a review or endorsement
    /// step to be undertaken.
    /// </summary>
    public class Trigger
    {
        private string key;

        /// <summary>
        /// Gets or sets the Name of the trigger.
        /// </summary>
        [Required]
        [WorkbookTableColumnName("Property")]
        [WorkbookTableColumnName("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the key for the trigger.
        /// </summary>
        [WorkbookTableColumnName("Key")]
        public string Key
        {
            get
            {
                if (this.key == null)
                {
                    return this.Name.ToCamelCase();
                }

                return this.key;
            }

            set
            {
                this.key = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the trigger. Standard trigger types include:
        /// - Review
        /// - Endorsement
        /// - Decline
        /// - Error.
        /// </summary>
        [Required]
        public string TypeName { get; set; }

        /// <summary>
        /// Gets the type of the trigger which is the TypeName in camel case.
        /// </summary>
        public string Type => this.TypeName.ToCamelCase();

        /// <summary>
        /// Gets or sets a summary for the trigger to be displayed to the customer if this trigger is activated.
        /// </summary>
        [WorkbookTableColumnName("Sidebar Header")]
        [WorkbookTableColumnName("Customer Summary")]
        public string CustomerSummary { get; set; }

        /// <summary>
        /// Gets or sets a summary for the trigger to be displayed to the agent who is filling out the form on behalf
        /// of the customer. If not set, the CustomerSummary is used.
        /// </summary>
        [WorkbookTableColumnName("Agent Summary")]
        public string AgentSummary { get; set; }

        /// <summary>
        /// Gets or sets the detailed message to be displayed to the customer when this trigger is activated.
        /// </summary>
        [WorkbookTableColumnName("Sidebar Message")]
        [WorkbookTableColumnName("Customer Message")]
        public string CustomerMessage { get; set; }

        /// <summary>
        /// Gets or sets the detailed message to be displayed to the agent when this trigger is activated,
        /// when the agent is filling out the form on behalf of the customer. If this is not set, the
        /// CustomerMessage is used.
        /// </summary>
        [WorkbookTableColumnName("Agent Message")]
        public string AgentMessage { get; set; }

        /// <summary>
        /// Gets or sets a title to be displayed above the message to the customer when this trigger is activated.
        /// If the price is set to be displayed, this title will precede the price, so it will be become the label
        /// for the price.
        /// If not set, the title or price label will not change when this trigger is actived.
        /// </summary>
        [WorkbookTableColumnName("Sidebar Title")]
        [WorkbookTableColumnName("Customer Title")]
        public string CustomerTitle { get; set; }

        /// <summary>
        /// Gets or sets a title to be displayed above the message to the agent when this trigger is activated and
        /// the agent is filling out the form on behalf of the customer.
        /// If the price is set to be displayed, this title will precede the price, so it will be become the label
        /// for the price.
        /// If not set, the CustomerTitle will be used.
        /// </summary>
        [WorkbookTableColumnName("Agent Title")]
        public string AgentTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the price should be displayed when this trigger is activated.
        /// </summary>
        [WorkbookTableColumnName("Display Price")]
        public bool DisplayPrice { get; set; }

        /// <summary>
        /// Gets or sets an explanation of the cause of the trigger to be shown to a person reviewing the trigger.
        /// </summary>
        [WorkbookTableColumnName("Description")]
        [WorkbookTableColumnName("Reviewer Explanation")]
        public string ReviewerExplanation { get; set; }
    }
}
