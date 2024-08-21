// <copyright file="IPeriodicSummaryResourceModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.ResourceModels
{
    /// <summary>
    /// Interface used for the resource models in Periodic Summary API.
    /// </summary>
    public interface IPeriodicSummaryResourceModel
    {
        /// <summary>
        /// Gets the label of each periodic summary.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Gets the date of the periodic summary if the sample period is Day.
        /// </summary>
        string Date { get; }

        /// <summary>
        /// Gets the fromDate of the periodic summary if the sample period is any other than Day.
        /// </summary>
        string FromDate { get; }

        /// <summary>
        /// Gets the toDate of the periodic summary if the sample period is any other than Day.
        /// </summary>
        string ToDate { get; }

        /// <summary>
        /// Gets the count of quotes created per periodic summary.
        /// </summary>
        int? CreatedCount { get; }

        /// <summary>
        /// Gets the total of quotes created per periodic summary.
        /// </summary>
        decimal? CreatedTotalPremium { get; }

        /// <summary>
        /// Gets the count of quotes converted per periodic summary.
        /// </summary>
        int? ConvertedCount { get; }

        /// <summary>
        /// Gets the count of quotes abandoned per periodic summary.
        /// </summary>
        int? AbandonedCount { get; }
    }
}
