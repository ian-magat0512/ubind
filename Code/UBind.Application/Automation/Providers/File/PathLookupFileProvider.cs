// <copyright file="PathLookupFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.File
{
    using System;
    using System.Threading.Tasks;
    using MorseCode.ITask;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.PathLookup;
    using UBind.Application.Automation.Providers.File.Model;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Repositories;

    /// <summary>
    /// This provider will be used to load files that are referenced from within object structures.
    /// Schema reference key: #objectPathLookupFile.
    /// </summary>
    public class PathLookupFileProvider : IPathLookupFileProvider
    {
        private readonly IObjectPathLookupProvider pathLookup;
        private readonly IProvider<Data<FileInfo>> valueIfNotFound;
        private readonly IFileContentRepository fileContentRepository;
        private readonly IProvider<Data<bool>> raiseErrorIfNotFound;
        private readonly IProvider<Data<bool>> raiseErrorIfNull;
        private readonly IProvider<Data<FileInfo>> valueIfNull;
        private readonly IProvider<Data<bool>> raiseErrorIfTypeMismatch;
        private readonly IProvider<Data<FileInfo>> valueIfTypeMismatch;
        private readonly IProvider<Data<FileInfo>> defaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathLookupFileProvider"/> class.
        /// </summary>
        /// <param name="path">The JSON pointer indicating the location within the dataObject where the file reference should be found.</param>
        /// <param name="valueIfNotFound">The value that should be returned if no file reference was found in the specified location.</param>
        /// <param name="fileContentRepository">The file content repository.</param>
        public PathLookupFileProvider(
            IObjectPathLookupProvider pathLookup,
            IProvider<Data<FileInfo>> valueIfNotFound,
            IFileContentRepository fileContentRepository,
            IProvider<Data<bool>> raiseErrorIfNotFound,
            IProvider<Data<bool>> raiseErrorIfNull,
            IProvider<Data<FileInfo>> valueIfNull,
            IProvider<Data<bool>> raiseErrorIfTypeMismatch,
            IProvider<Data<FileInfo>> valueIfTypeMismatch,
            IProvider<Data<FileInfo>> defaultValue)
        {
            this.pathLookup = pathLookup;
            this.valueIfNotFound = valueIfNotFound;
            this.fileContentRepository = fileContentRepository;
            this.raiseErrorIfNotFound = raiseErrorIfNotFound;
            this.raiseErrorIfNull = raiseErrorIfNull;
            this.valueIfNull = valueIfNull;
            this.raiseErrorIfTypeMismatch = raiseErrorIfTypeMismatch;
            this.valueIfTypeMismatch = valueIfTypeMismatch;
            this.defaultValue = defaultValue;
        }

        public string SchemaReferenceKey => "objectPathLookupFile";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<FileInfo>>> Resolve(IProviderContext providerContext)
        {
            using (MiniProfiler.Current.Step($"{this.GetType().Name}.{nameof(this.Resolve)}"))
            {
                return ProviderResult<Data<FileInfo>>.Success(await this.GetFileAttachment(providerContext));
            }
        }

        private async Task<Data<FileInfo>?> GetFileAttachment(IProviderContext providerContext)
        {
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            IData? lookupData = null;

            var lookupResult = await this.pathLookup.Resolve(providerContext);
            if (lookupResult.IsFailure)
            {
                var valueIfNotFound = await PathLookupResolverHelper.ResolveValueOrThrowIfNotFound(
                    this.raiseErrorIfNotFound,
                    this.valueIfNotFound,
                    this.defaultValue,
                    providerContext,
                    this.SchemaReferenceKey,
                    lookupResult);

                if (valueIfNotFound != null)
                {
                    return valueIfNotFound.GetValueFromGeneric();
                }
            }
            else
            {
                lookupData = lookupResult.Value;
            }

            if (lookupData == null)
            {
                var valueIfNull = await PathLookupResolverHelper.ResolveValueOrThrowIfNull(
                    this.raiseErrorIfNull,
                    this.valueIfNull,
                    this.defaultValue,
                    providerContext,
                    this.SchemaReferenceKey,
                    lookupData);

                if (valueIfNull == null)
                {
                    return null;
                }

                return valueIfNull.GetValueFromGeneric();
            }

            try
            {
                var genericValue = lookupData.GetValueFromGeneric();
                if (genericValue is FileInfo fileInfo)
                {
                    return new Data<FileInfo>(fileInfo);
                }

                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                genericValue = genericValue is JToken jToken && jToken.Type == JTokenType.String ? jToken.Value<string>() : genericValue;
                if (genericValue is string attachmentMetadata)
                {
                    var attachment = await this.GetAttachmentInfo(attachmentMetadata, providerContext);
                    var fileContent = this.fileContentRepository.GetFileContent(tenantId, attachment.AttachmentId) ??
                        throw new ErrorException(Errors.Automation.Provider.PathLookupFileNotFound(errorData));
                    var content = fileContent.Content;
                    var attachmentFileInfo = new FileInfo(attachment.FileName, content);
                    return new Data<FileInfo>(attachmentFileInfo);
                }

                if (genericValue is JToken jTokenObj && jTokenObj.Type == JTokenType.Object)
                {
                    try
                    {
                        var fileInfoObject = jTokenObj.ToObject<FileInfo>();
                        if (fileInfoObject != null)
                        {
                            return new Data<FileInfo>(fileInfoObject);
                        }
                    }
                    catch (JsonSerializationException)
                    {
                        // fall through to the error below.
                    }
                }

                errorData.Add("path", lookupData.GetValueFromGeneric());
                throw new ErrorException(Errors.Automation.Provider.PathLookupFileImplementationNotSupported(errorData));
            }
            catch (Exception exception)
            {
                string typeName = TypeHelper.GetReadableTypeName(lookupData.GetValueFromGeneric());
                IData? valueIfTypeMismatch = await PathLookupResolverHelper.ResolveValueOrThrowIfTypeMismatch(
                    this.raiseErrorIfTypeMismatch,
                    this.valueIfTypeMismatch,
                    this.defaultValue,
                    providerContext,
                    typeName,
                    "file",
                    lookupData,
                    this.SchemaReferenceKey,
                    exception);

                if (valueIfTypeMismatch == null)
                {
                    return null;
                }

                return valueIfTypeMismatch.GetValueFromGeneric();
            }
        }

        private async Task<FileAttachment> GetAttachmentInfo(string attachmentMetadata, IProviderContext providerContext)
        {
            // We currently support FormData file attachments saved as {filename}:{filetype}:{attachmentId}
            var attachment = attachmentMetadata.Split(':');

            var hasAttachment = Guid.TryParse(attachment[2], out Guid attachmentId);

            if (attachment.Length < 3 || !hasAttachment)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add("attachmentMetadata", attachmentMetadata);
                throw new ErrorException(
                    Errors.Automation.Provider.PathLookupFileImplementationNotSupported(errorData));
            }

            return new FileAttachment()
            {
                FileName = attachment[0],
                AttachmentId = attachmentId,
            };
        }
    }
}
