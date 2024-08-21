// <copyright file="IGraphClientFileRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph
{
    using System;
    using System.Threading.Tasks;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Client for performing file operations via Microsoft Graph.
    /// </summary>
    public interface IGraphClientFileRepository : IFilesystemFileRepository
    {
        /// <summary>
        /// Retrieves a shareable link for a file in OneDrive.
        /// </summary>
        /// <param name="itemId">The id of the file the link is for.</param>
        /// <param name="authenticationToken">An authentication token for the Graph web service.</param>
        /// <param name="accessType">The type of access the link is for.</param>
        /// <param name="timeout">The duration after which the request will timeout.</param>
        /// <returns>A task containing the link to the file.</returns>
        Task<string> GetShareableLinkForFile(string itemId, string authenticationToken, string accessType, TimeSpan timeout = default);
    }
}
