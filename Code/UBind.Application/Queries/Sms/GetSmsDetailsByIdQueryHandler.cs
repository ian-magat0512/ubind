// <copyright file="GetSmsDetailsByIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Sms
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Sms;
    using UBind.Domain.Repositories;

    public class GetSmsDetailsByIdQueryHandler : IQueryHandler<GetSmsDetailsByIdQuery, ISmsDetails>
    {
        private readonly ISmsRepository smsRepository;

        public GetSmsDetailsByIdQueryHandler(ISmsRepository smsRepository)
        {
            this.smsRepository = smsRepository;
        }

        public Task<ISmsDetails> Handle(GetSmsDetailsByIdQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var smsDetails = this.smsRepository.GetById(request.TenantId, request.SmsId);
            return Task.FromResult(smsDetails);
        }
    }
}
