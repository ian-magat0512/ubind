// <copyright file="QuoteSummaryViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using DotLiquid;

    /// <summary>
    /// Quote Summary view model providing data for use in liquid report templates.
    /// </summary>
    public class QuoteSummaryViewModel : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteSummaryViewModel"/> class.
        /// </summary>
        /// <param name="name">The name of the quote summary.</param>
        /// <param name="description">The description of the quote summary.</param>
        /// <param name="totalIncompleteQuotes">The total number of incomplete quotes.</param>
        /// <param name="totalCompletedQuotes">The total number completed quotes.</param>
        /// <param name="totalQuotes">The total number of quotes.</param>
        /// <param name="totalBasePremiumOfCompleteQuotes">The total 'base premium' of the completed quotes.</param>
        /// <param name="totalTotalPremiumOfCompleteQuotes">The total 'total premium' of the completed quotes.</param>
        public QuoteSummaryViewModel(
            string name,
            string description,
            int totalIncompleteQuotes,
            int totalCompletedQuotes,
            int totalQuotes,
            string totalBasePremiumOfCompleteQuotes,
            string totalTotalPremiumOfCompleteQuotes)
        {
            this.Name = name;
            this.Description = description;
            this.TotalIncompleteQuotes = totalIncompleteQuotes;
            this.TotalCompleteQuotes = totalCompletedQuotes;
            this.TotalQuotes = totalQuotes;
            this.TotalBasePremiumOfCompleteQuotes = totalBasePremiumOfCompleteQuotes;
            this.TotalTotalPremiumOfCompleteQuotes = totalTotalPremiumOfCompleteQuotes;
        }

        /// <summary>
        /// Gets the name of the quote summary.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the quote summary.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the total number of quotes.
        /// </summary>
        public int TotalQuotes { get; }

        /// <summary>
        /// Gets the total number of incomplete quotes.
        /// </summary>
        public int TotalIncompleteQuotes { get; }

        /// <summary>
        /// Gets the total number of completed quotes.
        /// </summary>
        public int TotalCompleteQuotes { get; }

        /// <summary>
        /// Gets the total of 'base premium' of the completed quotes.
        /// </summary>
        public string TotalBasePremiumOfCompleteQuotes { get; }

        /// <summary>
        /// Gets the total of 'total premium' of the completed quotes.
        /// </summary>
        public string TotalTotalPremiumOfCompleteQuotes { get; }
    }
}
