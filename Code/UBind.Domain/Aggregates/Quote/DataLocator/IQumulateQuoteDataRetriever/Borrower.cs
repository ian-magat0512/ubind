// <copyright file="Borrower.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever
{
    using Newtonsoft.Json;

    /// <summary>
    /// A borrower's details for iQumulate premium funding.
    /// </summary>
    public class Borrower
    {
        /// <summary>
        /// Gets or sets the borrower's first name.
        /// string (maxlen=50)
        /// (required) First name of the individual borrower.
        /// </summary>
        [JsonProperty]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the borrower's surnname.
        /// string (maxlen=50)
        /// (required) Surname of the individual borrower.
        /// </summary>
        [JsonProperty]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the borrower's date of birth.
        /// date (format yyyy-mm-dd)
        /// (required) Individual borrower’s date of birth.Used for identification of borrower for credit check purposes.
        /// </summary>
        [JsonProperty]
        public string DateOfBirth { get; set; }

        /// <summary>
        /// Gets or sets the borrower's drivers license number.
        /// string (maxlen=20)
        /// (optional) Individual borrower’s driver licence number.Used for identification of borrower for credit check purposes.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DriverLicense { get; set; }
    }
}
