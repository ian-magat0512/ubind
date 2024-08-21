// <copyright file="IAccessTokenService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using UBind.Domain.Redis;

    /// <summary>
    /// Service generating JWT tokens for authenticated users.
    /// </summary>
    public interface IAccessTokenService
    {
        /// <summary>
        /// Creates an access token for the given user session model, and stores the model in Redis.
        /// This is used if you want to customise what's in the userSessionModel, for example if you
        /// are logging someone in with SAML and you need to store the SAML session data so the session
        /// can be matched in the case of a IdP initiated logout.
        /// </summary>
        Task<JwtSecurityToken> CreateAccessToken(UserSessionModel userSessionModel);
    }
}
