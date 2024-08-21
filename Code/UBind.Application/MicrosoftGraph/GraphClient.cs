// <copyright file="GraphClient.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Flurl;
    using Flurl.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.MicrosoftGraph.Exceptions;
    using UBind.Domain.Dto;
    using UBind.Domain.Extensions;
    using UBindFileInfo = UBind.Domain.Models.FileInfo;

    /// <inheritdoc/>
    public class GraphClient : IGraphClientFileRepository
    {
        private const string SharingLinkScope = "organization";
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(180);
        private readonly IGraphUrlProvider urlProvider;
        private readonly IMicrosoftGraphConfiguration configuration;
        private readonly ILogger<GraphClient> logger;
        private ICachingAuthenticationTokenProvider cachingAuthenticationTokenProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphClient"/> class.
        /// </summary>
        /// <param name="urlProvider">Helper for creating URLs for MS Graph web API.</param>
        /// <param name="configuration">Configuration file for MS Graph API.</param>
        /// <param name="cachingAuthenticationTokenProvider">A caching provider of the ms graph authentication token.</param>
        /// <param name="logger">A logger.</param>
        public GraphClient(
            IGraphUrlProvider urlProvider,
            IMicrosoftGraphConfiguration configuration,
            ICachingAuthenticationTokenProvider cachingAuthenticationTokenProvider,
            ILogger<GraphClient> logger)
        {
            this.urlProvider = urlProvider;
            this.configuration = configuration;
            this.cachingAuthenticationTokenProvider = cachingAuthenticationTokenProvider;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public bool IsConfigured()
        {
            return this.configuration.ClientId != null
                && this.configuration.Username != null
                && this.configuration.Password != null
                && this.configuration.ApplicationId != null
                && this.configuration.UBindFolderName != null;
        }

        /// <inheritdoc/>
        public async Task<string> GetAuthenticationToken()
        {
            AuthenticationToken tokenWithExpiry = await this.cachingAuthenticationTokenProvider.GetAuthenticationTokenAsync();
            return tokenWithExpiry.BearerToken;
        }

        /// <inheritdoc/>
        public async Task<string> GetFileStringContents(string filePath, string authenticationToken, TimeSpan timeout = default)
        {
            Url url = this.urlProvider.GetFileContentsPath(filePath);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.GetFileStringContents)} ({url})"))
            {
                return await this.ExecuteAndWrapExceptions(
                    async () =>
                    {
                        var response = await url
                            .WithOAuthBearerToken(authenticationToken)
                            .WithHeader("Accept", "text/plain")
                            .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                            .GetAsync();
                        var responseContent = await response.ResponseMessage.Content.ReadAsStringAsync();
                        return responseContent;
                    },
                    nameof(this.GetFileStringContents));
            }
        }

        /// <inheritdoc/>
        public async Task<Maybe<string>> TryGetFileStringContents(string filePath, string authenticationToken, TimeSpan timeout = default)
        {
            Url url = this.urlProvider.GetFileContentsPath(filePath);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.TryGetFileStringContents)} ({url})"))
            {
                return await this.ExecuteAndWrapExceptions(
                    async () =>
                    {
                        try
                        {
                            var response = await url
                                .WithOAuthBearerToken(authenticationToken)
                                .WithHeader("Accept", "text/plain")
                                .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                                .GetAsync();
                            var responseContent = await response.ResponseMessage.Content.ReadAsStringAsync();
                            return Maybe<string>.From(responseContent);
                        }
                        catch (FlurlHttpException ex) when (ex.Call.Completed && ex.Call.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                        {
                            return Maybe<string>.None;
                        }
                    },
                    nameof(this.TryGetFileStringContents));
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetFileContents(string filePath, string authenticationToken, TimeSpan timeout = default)
        {
            var url = this.urlProvider.GetFileContentsPath(filePath);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.GetFileContents)} ({url})"))
            {
                return await this.ExecuteAndWrapExceptions(
                async () =>
                {
                    var response = await url
                        .WithOAuthBearerToken(authenticationToken)
                        .WithHeader("Accept", ContentTypes.Stream)
                        .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                        .GetAsync();
                    var content = await response.ResponseMessage.Content.ReadAsByteArrayAsync();
                    return content;
                },
                nameof(this.GetFileContents));
            }
        }

        public async Task<UBindFileInfo?> GetFileInfo(string filePath, string authenticationToken, TimeSpan timeout = default)
        {
            var url = this.urlProvider.GetFileMetadataPath(filePath);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.GetFileInfo)} ({url})"))
            {
                return await this.ExecuteAndWrapExceptions(
                async () =>
                {
                    var metadata = await url
                        .WithOAuthBearerToken(authenticationToken)
                        .WithHeader("Accept", ContentTypes.Stream)
                        .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                        .GetJsonAsync<FileMetadata>();
                    return new UBindFileInfo
                    {
                        Path = filePath,
                        LastModifiedTimestamp = Instant.FromDateTimeOffset(metadata.LastModifiedDateTime),
                        CreatedTimestamp = Instant.FromDateTimeOffset(metadata.CreatedDateTime),
                    };
                },
                nameof(this.GetFileInfo));
            }
        }

        /// <inheritdoc/>
        public async Task RenameItem(string itemPath, string newName, string authenticationToken, TimeSpan timeout = default)
        {
            var url = this.urlProvider.GetUpdateItemUrl(itemPath);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.RenameItem)} ({url})"))
            {
                await this.ExecuteAndWrapExceptions(
                async () =>
                {
                    try
                    {
                        var response = await url
                        .WithOAuthBearerToken(authenticationToken)
                        .WithHeader("Content-Type", "application/json")
                        .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                        .PatchJsonAsync(new { name = newName });
                    }
                    catch (FlurlHttpException exception)
                    {
                        if (exception.Call.Completed)
                        {
                            var error = await exception.GetResponseStringAsync();
                            if (error.Contains("The name from body should match the name specified in the url"))
                            {
                                throw await GraphRequestNotFoundException.Create(exception);
                            }
                        }

                        throw;
                    }
                },
                nameof(this.RenameItem));
            }
        }

        /// <inheritdoc/>
        public async Task<bool> FolderExists(string folderPath, string authenticationToken, TimeSpan timeout = default)
        {
            var url = this.urlProvider.GetFolderPath(folderPath);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.CreateFolder)} ({url})"))
            {
                try
                {
                    await url
                        .WithOAuthBearerToken(authenticationToken)
                        .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                        .GetAsync();
                    return true;
                }
                catch (FlurlHttpException ex) when (ex.Call.HttpResponseMessage.StatusCode == HttpStatusCode.Conflict)
                {
                    return true;
                }
                catch (FlurlHttpException ex)
                {
                    if (ex.Call.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                    {
                        return false;
                    }
                }

                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> FileExists(string filePath, string authenticationToken, TimeSpan timeout = default)
        {
            var url = this.urlProvider.GetFileContentsPath(filePath);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.FileExists)} ({url})"))
            {
                try
                {
                    await url
                        .WithOAuthBearerToken(authenticationToken)
                        .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                        .GetAsync();
                    return true;
                }
                catch (FlurlHttpException ex) when (ex.Call.HttpResponseMessage.StatusCode == HttpStatusCode.Conflict)
                {
                    return true;
                }
                catch (FlurlHttpException ex)
                {
                    if (ex.Call.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                    {
                        return false;
                    }
                }
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task CreateFolder(string parentPath, string folderName, string authenticationToken, TimeSpan timeout = default)
        {
            var url = this.urlProvider.GetCreateFolderUrl(parentPath);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.CreateFolder)} ({url})"))
            {
                var payload = new
                {
                    name = folderName,
                    folder = new { },
                };

                var alreadyCreatedFolderHierarchy = false;
                Func<FlurlHttpException, bool> shouldCreateHierarchyAndRetry =
                    ex => ex.Call.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound
                        && !string.IsNullOrWhiteSpace(parentPath)
                        && !alreadyCreatedFolderHierarchy;

                while (true)
                {
                    try
                    {
                        await url
                            .WithOAuthBearerToken(authenticationToken)
                            .WithHeader("Content-Type", "application/json")
                            .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                            .PostJsonAsync(payload);
                        return;
                    }
                    catch (FlurlHttpException ex) when (ex.Call.HttpResponseMessage.StatusCode == HttpStatusCode.Conflict)
                    {
                        // Ignore conflict errors as it means the folder has already been created.
                        return;
                    }
                    catch (FlurlHttpException ex) when (shouldCreateHierarchyAndRetry(ex))
                    {
                        await this.CreateFolderHierarchy(parentPath, authenticationToken);
                        alreadyCreatedFolderHierarchy = true;
                    }
                    catch (FlurlHttpException ex) when (this.LogException(nameof(this.GetFileContents), ex))
                    {
                        throw await GraphExceptionMapper.Wrap(ex);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task CreateFolder(string path, string authenticationToken, TimeSpan timeout = default)
        {
            var url = this.urlProvider.GetCreateFolderUrl(path);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.CreateFolder)} ({url})"))
            {
                var payload = new
                {
                    folder = new { },
                };

                var alreadyCreatedFolderHierarchy = false;
                Func<FlurlHttpException, bool> shouldCreateHierarchyAndRetry =
                    ex => ex.Call.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound
                        && !string.IsNullOrWhiteSpace(path)
                        && !alreadyCreatedFolderHierarchy;

                while (true)
                {
                    try
                    {
                        await url
                            .WithOAuthBearerToken(authenticationToken)
                            .WithHeader("Content-Type", "application/json")
                            .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                            .PostJsonAsync(payload);
                        return;
                    }
                    catch (FlurlHttpException ex) when (ex.Call.HttpResponseMessage.StatusCode == HttpStatusCode.Conflict)
                    {
                        // Ignore conflict errors as it means the folder has already been created.
                        return;
                    }
                    catch (FlurlHttpException ex) when (shouldCreateHierarchyAndRetry(ex))
                    {
                        await this.CreateFolderHierarchy(path, authenticationToken);
                        alreadyCreatedFolderHierarchy = true;
                    }
                    catch (FlurlHttpException ex) when (this.LogException(nameof(this.GetFileContents), ex))
                    {
                        throw await GraphExceptionMapper.Wrap(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a path in the graph API hierarchically.
        /// </summary>
        /// <param name="parentPath">the path to create.</param>
        /// <param name="authenticationToken">the authorization token to use.</param>
        /// <returns>async.</returns>
        public async Task CreateFolderHierarchy(string parentPath, string authenticationToken)
        {
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.CreateFolderHierarchy)} ({parentPath})"))
            {
                // try to create the folder hierarchy
                var pathSplit = parentPath.Split('\\', '/');

                for (int i = 0; i < pathSplit.Length; i++)
                {
                    try
                    {
                        // if the next element doesnt exist
                        if (pathSplit.ElementAtOrDefault(i + 1).IsNullOrEmpty())
                        {
                            continue;
                        }

                        var combination = Flurl.Url.Combine(pathSplit.Take(i + 1).ToArray());

                        await this.CreateFolder(combination, pathSplit[i + 1], authenticationToken);
                    }
                    catch (FlurlHttpException ex) when (ex.Call.HttpResponseMessage.StatusCode == HttpStatusCode.Conflict)
                    {
                        // Ignore conflict errors as we assume folder has already been created.
                        continue;
                    }
                    catch (FlurlHttpException ex)
                    {
                        throw await GraphExceptionMapper.Wrap(ex);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task DeleteFolder(string folderPath, string authenticationToken, TimeSpan timeout = default)
        {
            var url = this.urlProvider.GetFolderPath(folderPath);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.DeleteFolder)} (url)"))
            {
                await this.ExecuteAndWrapExceptions(
                async () =>
                {
                    try
                    {
                        await url
                            .WithOAuthBearerToken(authenticationToken)
                            .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                            .DeleteAsync();
                    }
                    catch (FlurlHttpException ex) when (ex.Call.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                    {
                        // Folder no longer exists
                    }
                },
                nameof(this.DeleteFolder));
            }
        }

        /// <inheritdoc/>
        public async Task CopyItem(
            string itemPath, string newParentPath, string newName, string authenticationToken, TimeSpan timeout = default)
        {
            var url = this.urlProvider.GetCopyItemUrl(itemPath);

            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.CopyItem)} ({url})"))
            {
                var destinationFolderInfo = await this.GetDriveItemInfo(newParentPath, authenticationToken);

                var payload = new
                {
                    parentReference = new
                    {
                        driveId = destinationFolderInfo.ParentReference?.DriveId,
                        id = destinationFolderInfo.Id,
                    },
                    name = newName,
                };

                await this.ExecuteAndWrapExceptions(
                    async () =>
                    {
                        try
                        {
                            await url
                                .WithOAuthBearerToken(authenticationToken)
                                .WithHeader("Content-Type", "application/json")
                                .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                                .PostJsonAsync(payload);
                        }
                        catch (FlurlHttpException ex) when (ex.Call.HttpResponseMessage.StatusCode == HttpStatusCode.Conflict)
                        {
                            // Ignore conflict errors as we assume the file has already been copied.
                        }
                    },
                    nameof(this.CopyItem));
            }
        }

        /// <inheritdoc/>
        public async Task DeleteItem(string path, string authenticationToken, TimeSpan timeout = default)
        {
            var url = this.urlProvider.GetUpdateItemUrl(path);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.DeleteItem)} ({url})"))
            {
                await this.ExecuteAndWrapExceptions(
                    async () =>
                    {
                        try
                        {
                            await url
                                .WithOAuthBearerToken(authenticationToken)
                                .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                                .DeleteAsync();
                        }
                        catch (FlurlHttpException ex) when (ex.Call.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                        {
                            // Ignore notfound as we assume the file has already been removed.
                            // Note that the folowwing codes also used to be ignored. That functionality
                            // may need to be restored if testing shows it is required:
                            // - HttpStatusCode.Conflict
                            // - HttpStatusCode.BadRequest
                            return;
                        }
                    },
                    nameof(this.DeleteItem));
            }
        }

        /// <inheritdoc/>
        public async Task WriteFileContents(
            byte[] fileContent, string destinationPath, string authenticationToken, TimeSpan timeout = default)
        {
            Url url = this.urlProvider.GetUploadFileUrl(destinationPath);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.WriteFileContents)} ({url})"))
            {
                await this.ExecuteAndWrapExceptions(
                    async () =>
                    {
                        try
                        {
                            await url
                                .WithOAuthBearerToken(authenticationToken)
                                .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                                .PutFileAsync(fileContent);
                        }
                        catch (FlurlHttpException ex) when (ex.Call.HttpResponseMessage.StatusCode == HttpStatusCode.Conflict)
                        {
                            // Ignore conflict as we assume file was already uploaded.
                        }
                    },
                    nameof(this.WriteFileContents));
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListItemsInFolder(
            string folderPath, string authenticationToken, TimeSpan timeout = default)
        {
            return await this.ListFilesInFolder(folderPath, authenticationToken, childItem => true);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListFilesInFolder(
            string folderPath, string authenticationToken, TimeSpan timeout = default)
        {
            return await this.ListFilesInFolder(folderPath, authenticationToken, childItem => childItem.IsFile);
        }

        public async Task<IEnumerable<UBindFileInfo>> ListFilesInfoInFolder(
            string folderPath, string authenticationToken, TimeSpan timeout = default)
        {
            Url url = this.urlProvider.GetFolderChildrenPath(folderPath);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}{nameof(this.ListFilesInFolder)} ({url})"))
            {
                return await this.ExecuteAndWrapExceptions(
                async () =>
                {
                    var response = await url
                    .WithOAuthBearerToken(authenticationToken)
                    .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                    .GetJsonAsync<ChildrenResponse>();
                    var files = response.Children
                        .Select(child => new UBindFileInfo
                        {
                            Path = child.Name,
                            LastModifiedTimestamp = child.FileSystemInfo.LastModifiedDateTime.FromExtendedIso8601String(),
                            CreatedTimestamp = child.FileSystemInfo.CreatedDateTime.FromExtendedIso8601String(),
                        });
                    return files;
                },
                nameof(this.ListFilesInFolder));
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListSubfoldersInFolder(
            string folderPath, string authenticationToken, TimeSpan timeout = default)
        {
            return await this.ListFilesInFolder(folderPath, authenticationToken, childItem => childItem.IsFolder);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListOfFilesLastModifiedDateTimeInFolder(
            string folderPath, string authenticationToken, TimeSpan timeout = default)
        {
            Url url = this.urlProvider.GetFolderChildrenPath(folderPath);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.ListOfFilesLastModifiedDateTimeInFolder)} ({url})"))
            {
                return await this.ExecuteAndWrapExceptions(
                    async () =>
                    {
                        var response = await url
                            .WithOAuthBearerToken(authenticationToken)
                            .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                            .GetJsonAsync<ChildrenResponse>();
                        var files = response.Children
                            .Where(child => child.IsFile)
                            .Select(child => child.FileSystemInfo.LastModifiedDateTime);
                        return files;
                    },
                    nameof(this.ListOfFilesLastModifiedDateTimeInFolder));
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetFilesHashStringInFolders(
            List<string> folderPaths, string authenticationToken, TimeSpan timeout = default)
        {
            var url = this.urlProvider.GraphBatchOrigin;
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.GetFilesHashStringInFolders)} ({url})"))
            {
                List<string> files = new List<string>();
                string filesHashString = string.Empty;
                Dictionary<string, string> filesAndModifiedDate = new Dictionary<string, string>();

                BatchRequests batchRequests = new BatchRequests();
                List<BatchRequest> requests = new List<BatchRequest>();

                foreach (string folderPath in folderPaths)
                {
                    requests.Add(new BatchRequest
                    {
                        Id = folderPath.ToWebPath(),
                        Method = "GET",
                        Url = this.urlProvider.GetBatchRequestFolderChildrenUrl(folderPath),
                    });
                }

                batchRequests.Requests = requests;

                return await this.ExecuteAndWrapExceptions(
                    async () =>
                    {
                        var response = await url
                        .WithOAuthBearerToken(authenticationToken)
                        .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                        .PostJsonAsync(batchRequests)
                        .ReceiveJson<BatchResponses<ChildrenResponse>>();

                        foreach (var batchResponse in response.Responses)
                        {
                            if (batchResponse.Status != HttpStatusCode.NotFound)
                            {
                                var batchResponseFiles = batchResponse.Body.Children
                                    .Where(child => child.IsFile)
                                    .Select(child => new KeyValuePair<string, string>(
                                        Path.Combine(batchResponse.Id, child.Name).ToLocalPath(),
                                        child.FileSystemInfo.LastModifiedDateTime));

                                filesAndModifiedDate = filesAndModifiedDate.Concat(batchResponseFiles).ToDictionary(x => x.Key, x => x.Value);
                            }
                        }

                        filesAndModifiedDate = filesAndModifiedDate.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                        var jsonBatchResponseFiles = JsonConvert.SerializeObject(filesAndModifiedDate);
                        filesHashString = jsonBatchResponseFiles.GetHashString();
                        return filesHashString;
                    },
                    nameof(this.GetFilesHashStringInFolders));
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ConfigurationFileDto>> GetConfigurationFilesInFolder(
            string path, string authenticationToken, TimeSpan timeout = default)
        {
            Url url = this.urlProvider.GetFolderChildrenPath(path);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}.{nameof(this.GetConfigurationFilesInFolder)} ({url})"))
            {
                return await this.ExecuteAndWrapExceptions(
                    async () =>
                    {
                        IEnumerable<ConfigurationFileDto> files = new List<ConfigurationFileDto>();

                        try
                        {
                            var response = await url
                            .WithOAuthBearerToken(authenticationToken)
                            .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                            .GetJsonAsync<ConfigurationFileResponse>();
                            if (response.Files == null)
                            {
                                return files;
                            }

                            files = response.Files
                                .Where(file => file.Path.StartsWith("files/"));

                            return files;
                        }
                        catch (FlurlHttpException ex)
                        {
                            // Ignore exception if its not found.
                            if (ex.Call.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                            {
                                return files;
                            }

                            throw ex;
                        }
                    },
                    nameof(this.GetConfigurationFilesInFolder));
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetShareableLinkForFile(
            string itemId, string authenticationToken, string accessType, TimeSpan timeout = default)
        {
            Url url = this.urlProvider.GetCreateLinkFileUrl(itemId);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}{nameof(this.GetShareableLinkForFile)} ({url})"))
            {
                var model = new { type = accessType, scope = SharingLinkScope };
                return await this.ExecuteAndWrapExceptions(
                    async () =>
                    {
                        var response = await url
                        .WithOAuthBearerToken(authenticationToken)
                        .WithHeader("Content-type", "application/json")
                        .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                        .PostJsonAsync(model)
                        .ReceiveJson<ResourceUrlResponse>();

                        return response.ResourceUrl.Weburl;
                    },
                    nameof(this.GetShareableLinkForFile));
            }
        }

        /// <inheritdoc/>
        public Task MoveFolder(
            string sourcePath,
            string destinationPath,
            bool createTargetPathIfSourceDoesntExist,
            string authenticationToken,
            TimeSpan timeout = default)
        {
            // not implemented because we dont use graph client anymore.
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<string>> ListFilesInFolder(
            string folderPath,
            string authenticationToken,
            Predicate<ChildrenResponse.ChildItem> selector,
            TimeSpan timeout = default)
        {
            Url url = this.urlProvider.GetFolderChildrenPath(folderPath);
            using (MiniProfiler.Current.Step($"{nameof(GraphClient)}{nameof(this.ListFilesInFolder)} ({url})"))
            {
                return await this.ExecuteAndWrapExceptions(
                async () =>
                {
                    var response = await url
                    .WithOAuthBearerToken(authenticationToken)
                    .WithTimeout(timeout != default ? timeout : DefaultTimeout)
                    .GetJsonAsync<ChildrenResponse>();
                    var files = response.Children
                        .Where(child => selector(child))
                        .Select(child => child.Name);
                    return files;
                },
                nameof(this.ListFilesInFolder));
            }
        }

        private async Task ExecuteAndWrapExceptions(Func<Task> action, string methodName)
        {
            try
            {
                await action.Invoke();
            }
            catch (FlurlHttpException ex)
            {
                this.LogException(methodName, ex);
                throw await GraphExceptionMapper.Wrap(ex);
            }
        }

        private async Task<TResult> ExecuteAndWrapExceptions<TResult>(Func<Task<TResult>> action, string methodName)
        {
            try
            {
                return await action.Invoke();
            }
            catch (FlurlHttpException ex)
            {
                this.LogException(methodName, ex);
                throw await GraphExceptionMapper.Wrap(ex);
            }
        }

        private bool LogException(string methodName, FlurlHttpException ex)
        {
            string messageFormat = $"Exception thrown in {nameof(GraphClient)}.{{0}}: {{1}}";
            this.logger.LogError(ex, messageFormat, messageFormat, methodName, ex.Message);
            return false;
        }

        private async Task<DriveItemInfo> GetDriveItemInfo(string driveItemPath, string token)
        {
            return await this.ExecuteAndWrapExceptions(
                async () =>
                {
                    var url = this.urlProvider.GetFolderPath(driveItemPath);
                    return await url
                        .WithOAuthBearerToken(token)
                        .GetJsonAsync<DriveItemInfo>();
                },
                nameof(this.GetDriveItemInfo));
        }

        /// <summary>
        /// Class for interpreting the response to a OneDrive children request.
        /// </summary>
        private class ChildrenResponse
        {
            [JsonProperty("value")]
            public IEnumerable<ChildItem> Children { get; set; }

            public class ChildItem
            {
                public string Name { get; set; }

                public File File { get; set; }

                public dynamic Folder { get; set; }

                public dynamic Weburl { get; set; }

                public FileSystemInfo FileSystemInfo { get; set; }

                public bool IsFile => this.File != null;

                public bool IsFolder => this.Folder != null;
            }

            public class FileSystemInfo
            {
                public string CreatedDateTime { get; set; }

                public string LastModifiedDateTime { get; set; }
            }

            public class File
            {
                public string MimeType { get; set; }

                public Hash Hashes { get; set; }
            }

            public class Hash
            {
                public string QuickXorHash { get; set; }
            }
        }

        private class ConfigurationFileResponse
        {
            [JsonProperty("value")]
            public IEnumerable<ConfigurationFileDto> Files { get; set; }
        }

        private class ResourceUrlResponse
        {
            [JsonProperty("link")]
            public Resource ResourceUrl { get; set; }

            public class Resource
            {
                public dynamic Scope { get; set; }

                public dynamic Type { get; set; }

                public dynamic Weburl { get; set; }
            }
        }

        private class BatchRequest
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("method")]
            public string Method { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }
        }

        private class BatchRequests
        {
            [JsonProperty("requests")]
            public List<BatchRequest> Requests { get; set; }
        }

        private class BatchResponse<T>
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("status")]
            public HttpStatusCode Status { get; set; }

            [JsonProperty("body")]
            public T Body { get; set; }
        }

        private class BatchResponses<T>
        {
            [JsonProperty("responses")]
            public List<BatchResponse<T>> Responses { get; set; }
        }

        private class DriveItemInfo
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("parentReference")]
            public ParentReference ParentReference { get; set; }
        }

        private class ParentReference
        {
            [JsonProperty("driveId")]
            public string DriveId { get; set; }

            [JsonProperty("driveType")]
            public string DriveType { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("path")]
            public string Path { get; set; }
        }

        private class FileMetadata
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("createdDateTime")]
            public DateTimeOffset CreatedDateTime { get; set; }

            [JsonProperty("lastModifiedDateTime")]
            public DateTimeOffset LastModifiedDateTime { get; set; }

            // Include other properties as needed
        }
    }
}
