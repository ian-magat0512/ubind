// <copyright file="GetQuoteVersionDocumentContentQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Quote
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class GetQuoteVersionDocumentContentQueryHandler
        : IQueryHandler<GetQuoteVersionDocumentContentQuery, IFileContentReadModel>
    {
        private readonly IQuoteVersionReadModelRepository quoteVersionRepository;
        private readonly IQuoteDocumentReadModelRepository quoteDocumentRepository;

        public GetQuoteVersionDocumentContentQueryHandler(
            IQuoteVersionReadModelRepository quoteVersionRepository,
            IQuoteDocumentReadModelRepository quoteDocumentRepository)
        {
            this.quoteVersionRepository = quoteVersionRepository;
            this.quoteDocumentRepository = quoteDocumentRepository;
        }

        public Task<IFileContentReadModel> Handle(GetQuoteVersionDocumentContentQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteVersion = this.quoteVersionRepository.GetVersionDetailsById(
                request.TenantId, request.QuoteVersionId);
            if (quoteVersion == null)
            {
                throw new ErrorException(Errors.General.NotFound("quote version", request.QuoteVersionId));
            }

            return Task.FromResult(this.quoteDocumentRepository
                .GetDocumentContent(request.TenantId, request.DocumentId));
        }
    }
}
