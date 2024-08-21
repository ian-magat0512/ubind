// <copyright file="RenameProductDirectoryCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Tenant
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.MicrosoftGraph;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Renames product directories for the tenant and all its products.
    /// </summary>
    public class RenameProductDirectoryCommandHandler :
        ICommandHandler<RenameProductDirectoryCommand, Unit>
    {
        private readonly IFilesystemStoragePathService pathService;
        private readonly ICachingAuthenticationTokenProvider authenticator;
        private readonly IFilesystemFileRepository fileRepository;
        private readonly ITenantRepository tenantRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenameProductDirectoryCommandHandler"/> class.
        /// </summary>
        public RenameProductDirectoryCommandHandler(
            ICachingAuthenticationTokenProvider authenticator,
            ITenantRepository tenantRepository,
            IFilesystemStoragePathService pathService,
            IFilesystemFileRepository fileRepository)
        {
            this.authenticator = authenticator;
            this.fileRepository = fileRepository;
            this.tenantRepository = tenantRepository;
            this.pathService = pathService;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(
            RenameProductDirectoryCommand command,
            CancellationToken cancellationToken)
        {
            string bearerToken = await this.GetBearerToken();
            var tenant = this.tenantRepository.GetTenantById(command.TenantId);
            string oldTenantAlias = command.OldTenantAlias;
            string newTenantAlias = command.NewTenantAlias;

            if (oldTenantAlias == newTenantAlias)
            {
                return Unit.Value;
            }

            var oldDevelopmentPath = this.pathService.DevelopmentFolderPath + "\\" + oldTenantAlias;
            var newDevelopmentPath = this.pathService.DevelopmentFolderPath + "\\" + newTenantAlias;

            var oldReleasePath = this.pathService.ReleasesFolderPath + "\\" + oldTenantAlias;
            var newReleasePath = this.pathService.ReleasesFolderPath + "\\" + newTenantAlias;

            void RevertChanges(string folderName, string errorMessage)
            {
                // revert changes.
                tenant.Details.UpdateAlias(oldTenantAlias);
                this.tenantRepository.SaveChanges();

                // throw error
                throw new ErrorException(Errors.Tenant.UnableToRenameProductDirectory(
                    oldTenantAlias,
                    newTenantAlias,
                    folderName,
                    errorMessage));
            }

            try
            {
                await this.MoveFolder(oldDevelopmentPath, newDevelopmentPath, bearerToken);
            }
            catch (IOException ioException)
            {
                // revert changes.
                RevertChanges(this.pathService.DevelopmentFolderName, ioException.Message);
            }

            try
            {
                await this.MoveFolder(oldReleasePath, newReleasePath, bearerToken);
            }
            catch (IOException ioException)
            {
                // revert changes.
                RevertChanges(this.pathService.ReleasesFolderName, ioException.Message);
            }

            return Unit.Value;
        }

        private async Task MoveFolder(string oldPath, string newPath, string bearerToken)
        {
            if (await this.fileRepository.FolderExists(oldPath, bearerToken))
            {
                await this.fileRepository.MoveFolder(oldPath, newPath, true, bearerToken);
            }
            else
            {
                if (!await this.fileRepository.FolderExists(newPath, bearerToken))
                {
                    await this.fileRepository.CreateFolder(newPath, bearerToken);
                }
            }
        }

        private async Task<string> GetBearerToken()
        {
            return await this.fileRepository.GetAuthenticationToken();
        }
    }
}
