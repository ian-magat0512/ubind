// <copyright file="EntityFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.File
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Repositories;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// This class is needed because we need a provider that will retrieve file from entity and attach it to a email or entity.
    /// </summary>
    public class EntityFileProvider : IProvider<Data<FileInfo>>
    {
        private readonly IFileContentRepository fileContentRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFileProvider"/> class.
        /// </summary>
        /// <param name="outputFileName">The output file name.</param>
        /// <param name="fileName">The name of the file attached in the entity.</param>
        /// <param name="entity">The entity the file is attached.</param>
        /// <param name="fileContentRepository">The file content repository.</param>
        public EntityFileProvider(
            IProvider<Data<string>>? outputFileName,
            IProvider<Data<string>> fileName,
            BaseEntityProvider entity,
            IFileContentRepository fileContentRepository)
        {
            this.OutputFileName = outputFileName;
            this.FileName = fileName;
            this.Entity = entity;
            this.fileContentRepository = fileContentRepository;
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public IProvider<Data<string>>? OutputFileName { get; }

        /// <summary>
        /// Gets the name of the file attached in the entity.
        /// </summary>
        public IProvider<Data<string>> FileName { get; }

        /// <summary>
        /// Gets the product environment for the product from which the file should be loaded.
        /// </summary>
        public BaseEntityProvider Entity { get; }

        public string SchemaReferenceKey => "entityFile";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<FileInfo>>> Resolve(IProviderContext providerContext)
        {
            string? outputFileName = (await this.OutputFileName.ResolveValueIfNotNull(providerContext))?.DataValue;
            string documentName = (await this.FileName.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            this.Entity.IncludedProperties = new List<string>() { "documents" };
            var resolveEntity = await this.Entity.Resolve(providerContext);
            var entity = resolveEntity.GetValueOrThrowIfFailed().DataValue;
            var entityType = entity.GetType().Name;

            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            errorData.Merge(GenericErrorDataHelper.RetrieveErrorData(entity));
            errorData.Add("outputFilename", outputFileName);
            errorData.Add("filename", documentName);

            if (!entity.SupportsFileAttachment)
            {
                throw new ErrorException(Errors.Automation.Provider.Entity.AttachmentNotSupported(entityType, errorData));
            }

            try
            {
                var filename = new FileName(outputFileName ?? documentName);
                PropertyInfo? documentsPropertyInfo = entity.GetType().GetProperty("Documents", BindingFlags.Instance | BindingFlags.Public);
                if (documentsPropertyInfo == null)
                {
                    throw new ErrorException(Errors.Automation.Provider.Entity.AttachmentNotSupported(entityType, errorData));
                }

                var documents = (List<Document>?)documentsPropertyInfo.GetValue(entity);
                if (documents == null)
                {
                    throw new ErrorException(Errors.Automation.Provider.FileNotFound(errorData));
                }

                var document = documents
                    .OrderByDescending(c => c.CreatedTicksSinceEpoch)
                    .FirstOrDefault(c => c.FileName.EqualsIgnoreCase(documentName));

                if (document == null)
                {
                    throw new ErrorException(Errors.Automation.Provider.FileNotFound(errorData));
                }

                var fileContentId = new Guid(document.ContentId);
                var content = this.fileContentRepository.GetFileContentById(fileContentId);
                if (content == null)
                {
                    throw new ErrorException(Errors.Automation.Provider.FileNotFound(errorData));
                }
                return ProviderResult<Data<FileInfo>>.Success(new FileInfo(filename.ToString(), content.Content));
            }
            catch (ErrorException appException) when (appException.Error.Code.EqualsIgnoreCase("file.name.invalid"))
            {
                var additionalDetails = appException.Error.AdditionalDetails != null && appException.Error.AdditionalDetails.Any()
                    ? new List<string>() { appException.Error.AdditionalDetails.Last() }
                    : null;
                throw new ErrorException(Errors.Automation.Provider.ProductFileHasInvalidFileName(errorData, additionalDetails));
            }
        }
    }
}
