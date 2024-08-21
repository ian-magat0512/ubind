// <copyright file="AccessTokenService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;
    using NodaTime;
    using UBind.Application.Authentication;
    using UBind.Application.Infrastructure;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Redis;
    using UBind.Domain.Repositories.Redis;

    /// <inheritdoc />
    public class AccessTokenService : IAccessTokenService
    {
        private readonly IAuthConfiguration authConfiguration;
        private readonly IUserSessionRepository userSessionRepository;
        private readonly ICachedJwtKeyService cachedJwtKeyService;
        private readonly ICqrsMediator mediator;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessTokenService"/> class.
        /// </summary>
        /// <param name="authConfiguration">Access Token Service.</param>
        /// <param name="tokenSessionService">Token Access Service.</param>
        /// <param name="mediator">The mediator.</param>
        public AccessTokenService(
            IAuthConfiguration authConfiguration,
            IUserSessionRepository userSessionRepository,
            ICachedJwtKeyService cachedJwtKeyService,
            ICqrsMediator mediator,
            IClock clock)
        {
            this.authConfiguration = authConfiguration;
            this.userSessionRepository = userSessionRepository;
            this.cachedJwtKeyService = cachedJwtKeyService;
            this.mediator = mediator;
            this.clock = clock;
        }

        /// <inheritdoc />
        public async Task<JwtSecurityToken> CreateAccessToken(UserSessionModel userSessionModel)
        {
            await this.userSessionRepository.Upsert(userSessionModel.TenantId, userSessionModel);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userSessionModel.UserId.ToString()),
                new Claim(ClaimNames.TenantId, userSessionModel.TenantId.ToString()),
                new Claim(ClaimNames.OrganisationId, userSessionModel.OrganisationId.ToString()),
                new Claim(ClaimTypes.Role, userSessionModel.UserType),
                new Claim("SessionId", userSessionModel.Id.ToString()),
            };

            if (userSessionModel.CustomerId != null)
            {
                claims.Add(new Claim(ClaimNames.CustomerId, userSessionModel.CustomerId.Value.ToString()));
            }

            var latestKey = this.cachedJwtKeyService.GetLatestKey();
            var key = new SymmetricSecurityKey(Convert.FromBase64String(latestKey.KeyBase64));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: this.authConfiguration.TokenIssuer,
                audience: this.authConfiguration.TokenAudience,
                claims: claims,
                expires: DateTime.Now.AddDays(365),
                signingCredentials: creds);
            return token;
        }
    }
}
