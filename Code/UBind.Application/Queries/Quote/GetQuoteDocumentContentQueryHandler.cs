// <copyright file="GetQuoteDocumentContentQueryHandler.cs" company="uBind">
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

    public class GetQuoteDocumentContentQueryHandler
        : IQueryHandler<GetQuoteDocumentContentQuery, IFileContentReadModel>
    {
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly IQuoteDocumentReadModelRepository quoteDocumentReadModelRepository;

        public GetQuoteDocumentContentQueryHandler(
            IQuoteReadModelRepository quoteReadModelRepository,
            IQuoteDocumentReadModelRepository quoteDocumentReadModelRepository)
        {
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.quoteDocumentReadModelRepository = quoteDocumentReadModelRepository;
        }

        public Task<IFileContentReadModel> Handle(GetQuoteDocumentContentQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quote = this.quoteReadModelRepository
                .GetQuoteDetails(request.TenantId, request.QuoteId);
            if (quote == null)
            {
                throw new ErrorException(Errors.Quote.NotFound(request.QuoteId));
            }

            return Task.FromResult(this.quoteDocumentReadModelRepository
                .GetDocumentContent(request.TenantId, request.DocumentId));
        }
    }
}
