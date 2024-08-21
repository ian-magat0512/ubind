// <copyright file="IsUserPasswordExpiredQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class IsUserPasswordExpiredQueryHandler : IQueryHandler<IsUserPasswordExpiredQuery, bool>
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IClock clock;

        public IsUserPasswordExpiredQueryHandler(
            ICachingResolver cachingResolver,
            IUserReadModelRepository userReadModelRepository,
            IClock clock)
        {
            this.cachingResolver = cachingResolver;
            this.userReadModelRepository = userReadModelRepository;
            this.clock = clock;
        }

        public async Task<bool> Handle(IsUserPasswordExpiredQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Tenant tenant = await this.cachingResolver.GetTenantOrThrow(request.TenantId);

            var user = this.userReadModelRepository.GetUser(tenant.Id, request.UserId);

            if (user == null)
            {
                throw new ErrorException(Errors.User.ResetPassword.UserNotFound(request.UserId));
            }

            if (tenant.Details.PasswordExpiryEnabled)
            {
                var passwordExpiryTimeSpan = TimeSpan.FromDays((double)tenant.Details.MaxPasswordAgeDays);
                var expiryDuration = this.clock.Now() - user.PasswordLastChangedTimestamp;
                return expiryDuration.ToTimeSpan() > passwordExpiryTimeSpan;
            }

            return false;
        }
    }
}
