// <copyright file="InvoiceViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using UBind.Domain.Extensions;

    /// <summary>
    /// Invoice view model for Razor Templates to use.
    /// </summary>
    public class InvoiceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceViewModel"/> class.
        /// </summary>
        /// <param name="invoice">The invoice to present.</param>
        public InvoiceViewModel(Domain.Aggregates.Quote.Invoice invoice)
        {
            var createdTimestamp = invoice.CreatedTimestamp;
            this.Number = invoice.InvoiceNumber;
            this.Date = createdTimestamp.ToRfc5322DateStringInAet();
            this.CreationDate = this.Date;
            this.CreationTime = createdTimestamp.To12HourClockTimeInAet();
        }

        /// <summary>
        /// Gets the invoice number.
        /// </summary>
        public string Number { get; private set; }

        /// <summary>
        /// Gets the invoice created date (for backward compatibility).
        /// </summary>
        public string Date { get; private set; }

        /// <summary>
        /// Gets the invoice created date.
        /// </summary>
        public string CreationDate { get; private set; }

        /// <summary>
        /// Gets the invoice created time.
        /// </summary>
        public string CreationTime { get; private set; }
    }
}
