// <copyright file="EmailSummaryViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using DotLiquid;

    /// <summary>
    /// Email Summary view model providing data for use in liquid report templates.
    /// </summary>
    public class EmailSummaryViewModel : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSummaryViewModel"/> class.
        /// </summary>
        /// <param name="name">The name of the email summary.</param>
        /// <param name="description">The description of the email summary.</param>
        /// <param name="totalEmailsSent">The total number of emails sent.</param>
        /// <param name="totalEmailsSentToCustomer">The total number of emails sent to the customer.</param>
        /// <param name="totalEmailsSentToClient">The total number of emails sent to the client.</param>
        public EmailSummaryViewModel(
            string name,
            string description,
            int totalEmailsSent,
            int totalEmailsSentToCustomer,
            int totalEmailsSentToClient)
        {
            this.Name = name;
            this.Description = description;
            this.TotalEmailsSent = totalEmailsSent;
            this.TotalEmailsSentToCustomer = totalEmailsSentToCustomer;
            this.TotalEmailsSentToClient = totalEmailsSentToClient;
        }

        /// <summary>
        /// Gets the name of the email summary.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the email summary.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the total number of emails sent.
        /// </summary>
        public int TotalEmailsSent { get; }

        /// <summary>
        /// Gets the total number of emails sent to the customer.
        /// </summary>
        public int TotalEmailsSentToCustomer { get; }

        /// <summary>
        /// Gets the total number of emails sent to the client.
        /// </summary>
        public int TotalEmailsSentToClient { get; }
    }
}
