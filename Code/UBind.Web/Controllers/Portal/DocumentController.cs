// <copyright file="DocumentController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Portal
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;

    /// <summary>
    /// Controller for handling portal-related quote requests.
    /// </summary>
    [MustBeLoggedIn]
    [Route("/api/v1/{environment}/document")]
    public class DocumentController : Controller
    {
        private readonly IDocumentService documentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentController"/> class.
        /// </summary>
        /// <param name="documentService">The document service.</param>
        public DocumentController(
            IDocumentService documentService)
        {
            this.documentService = documentService;
        }

        /// <summary>
        /// Gets the email document by document Id.
        /// </summary>
        /// <param name="documentId">THe ID of the document to retrieve.</param>
        /// <param name="quoteEmailId">THe ID of the email the document is from.</param>
        /// <returns>The document.</returns>
        [HttpGet]
        [Route("{documentId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustBeLoggedIn]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public ActionResult GetEmailDocument(Guid documentId, [FromQuery] Guid quoteEmailId)
        {
            // TODO: this needs some sort of permission, but what?
            var fileContent = this.documentService.GetEmailDocument(documentId, quoteEmailId);

            if (fileContent == null)
            {
                return Errors.General.NotFound("document", documentId).ToProblemJsonResult();
            }

            var fileContentResult = new FileContentResult(fileContent.FileContent, fileContent.ContentType);
            return fileContentResult;
        }
    }
}
