// <copyright file="GetEmailSummariesByFilterQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Email
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    public class GetEmailSummariesByFilterQueryHandler : ICommandHandler<GetEmailSummariesByFilterQuery, IEnumerable<IEmailSummary>>
    {
        private readonly IEmailRepository emailRepository;

        public GetEmailSummariesByFilterQueryHandler(IEmailRepository emailRepository)
        {
            this.emailRepository = emailRepository;
        }

        public Task<IEnumerable<IEmailSummary>> Handle(GetEmailSummariesByFilterQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var emails = this.emailRepository.GetSummaries(request.TenantId, request.Filters).ToList();
            return Task.FromResult(emails.AsEnumerable());
        }
    }
}
