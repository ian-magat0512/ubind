// <copyright file="IUserSessionRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories.Redis
{
    using UBind.Domain.Redis;

    public interface IUserSessionRepository
    {
        Task Upsert(Guid tenantId, UserSessionModel userSessionModel);

        Task<UserSessionModel?> Get(Guid tenantId, Guid userSessionId);

        /// <summary>
        /// Deletes a single session for a user.
        /// Users can actually have multiple sessions (for example, if they are logged into multiple devices)
        /// so this is usually called when a user logs out from one of them.
        /// </summary>
        Task Delete(Guid tenantId, Guid userId, Guid userSessionId);

        /// <summary>
        /// Finds and deletes a session that matches the saml session data.
        /// </summary>
        Task<UserSessionModel?> DeleteSamlSession(Guid tenantId, SamlSessionData samlSessionData);

        /// <summary>
        /// Deletes all sessions for a given user.
        /// </summary>
        /// <returns>True if user has session that has been deleted.</returns>
        Task<bool> DeleteAllSessionsForUser(Guid tenantId, Guid userId);

        /// <summary>
        /// Delets all sessions for users in the given tenancy.
        /// </summary>
        /// <returns>Id of users whose session has been deleted under the tenancy.</returns>
        Task<List<Guid>> DeleteAll(Guid tenantId);
    }
}
