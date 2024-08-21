// <copyright file="CreditNoteViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Credit note view model for Razor Templates to use.
    /// </summary>
    public class CreditNoteViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreditNoteViewModel"/> class.
        /// </summary>
        /// <param name="creditNote">The credit note to present.</param>
        public CreditNoteViewModel(ObsoleteCreditNote creditNote)
        {
            this.Number = creditNote.CreditNoteNumber;
            this.Date = creditNote.CreatedTimestamp.ToRfc5322DateStringInAet();
            this.CreationDate = this.Date;
            this.CreationTime = creditNote.CreatedTimestamp.To12HourClockTimeInAet();
        }

        /// <summary>
        /// Gets the credit note number.
        /// </summary>
        public string Number { get; private set; }

        /// <summary>
        /// Gets the credit note created time.
        /// </summary>
        public string CreationTime { get; private set; }

        /// <summary>
        /// Gets the credit note created date ( for backward compatibility ).
        /// </summary>
        public string Date { get; private set; }

        /// <summary>
        /// Gets the credit note created date.
        /// </summary>
        public string CreationDate { get; private set; }
    }
}
