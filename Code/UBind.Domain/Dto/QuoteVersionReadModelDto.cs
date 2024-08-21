// <copyright file="QuoteVersionReadModelDto.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Dto
{
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Data Transfer Object for the Quote Versions.
    /// </summary>
    public class QuoteVersionReadModelDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteVersionReadModelDto"/> class.
        /// </summary>
        public QuoteVersionReadModelDto()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteVersionReadModelDto"/> class using a quote version.
        /// </summary>
        /// <param name="quoteVersionDetails">The quote version read model.</param>
        public QuoteVersionReadModelDto(IQuoteVersionReadModelDetails quoteVersionDetails)
        {
            this.QuoteVersionNumber = quoteVersionDetails.QuoteVersionNumber;
            this.WorkflowStep = quoteVersionDetails.WorkflowStep;
            this.LatestFormDataJson = quoteVersionDetails.LatestFormData;
        }

        /// <summary>
        /// Gets or sets the Quote Version Number.
        /// </summary>
        public int QuoteVersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the Quote Version workflow step when it was created.
        /// </summary>
        public string WorkflowStep { get; set; }

        /// <summary>
        /// Gets or sets the latest form data in JSON format.
        /// </summary>
        public string LatestFormDataJson { get; set; }
    }
}
