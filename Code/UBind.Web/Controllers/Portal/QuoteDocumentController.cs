// <copyright file="QuoteDocumentController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Portal
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.Quote;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;

    /// <summary>
    /// Controller for handling portal-related quote requests.
    /// </summary>
    [MustBeLoggedIn]
    [Route("/api/v1/{environment}/")]
    public class QuoteDocumentController : Controller
    {
        private readonly ICqrsMediator cqrsMediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteDocumentController"/> class.
        /// </summary>
        /// <param name="mediator">The uBind mediator.</param>
        public QuoteDocumentController(
            ICqrsMediator mediator)
        {
            this.cqrsMediator = mediator;
        }

        /// <summary>
        /// Gets the content of a document owned by a quote.
        /// </summary>
        /// <param name="documentId">THe ID of the document to retrieve.</param>
        /// <param name="quoteId">The ID of the quote the document is for.</param>
        /// <returns>The document.</returns>
        [HttpGet]
        [Route("quote/document/{documentId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewQuotes, Permission.ViewAllQuotes, Permission.ViewAllQuotesFromAllOrganisations)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetQuoteDocument(Guid documentId, [FromQuery] Guid quoteId)
        {
            var fileContent = await this.cqrsMediator.Send(new GetQuoteDocumentContentQuery(
                this.User.GetTenantId(),
                quoteId,
                documentId));

            if (fileContent == null)
            {
                return Errors.General.NotFound("quote document", documentId).ToProblemJsonResult();
            }

            var fileContentResult = new FileContentResult(fileContent.FileContent, fileContent.ContentType);
            return fileContentResult;
        }

        /// <summary>
        /// Gets the content of a document owned by a quote version.
        /// </summary>
        /// <param name="documentId">THe ID of the document to retrieve.</param>
        /// <param name="quoteVersionId">The ID of the quote version the document is for.</param>
        /// <returns>The document.</returns>
        [HttpGet]
        [Route("quote-version/document/{documentId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [MustHaveOneOfPermissions(Permission.ViewQuotes, Permission.ViewAllQuotes, Permission.ViewAllQuotesFromAllOrganisations)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetQuoteVersionDocument(Guid documentId, [FromQuery] Guid quoteVersionId)
        {
            var fileContent = await this.cqrsMediator.Send(new GetQuoteVersionDocumentContentQuery(
                this.User.GetTenantId(),
                quoteVersionId,
                documentId));

            if (fileContent == null)
            {
                return Errors.General.NotFound("quote document", documentId).ToProblemJsonResult();
            }

            var fileContentResult = new FileContentResult(fileContent.FileContent, fileContent.ContentType);
            return fileContentResult;
        }
    }
}
