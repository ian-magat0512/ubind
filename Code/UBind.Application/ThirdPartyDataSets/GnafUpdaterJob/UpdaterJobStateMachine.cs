// <copyright file="UpdaterJobStateMachine.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets.GnafUpdaterJob
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Commands.ThirdPartyDataSets;
    using UBind.Application.Commands.ThirdPartyDataSets.Gnaf;
    using UBind.Application.ThirdPartyDataSets.ViewModel;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Repositories.FileSystem;
    using UBind.Domain.ThirdPartyDataSets;

    /// <summary>
    /// Provides updater state machines for Gnaf third party data sets updater job.
    /// </summary>
    public class UpdaterJobStateMachine : BaseUpdaterJob
    {
        private readonly ICqrsMediator mediator;
        private readonly IFileSystemService fileSystemService;
        private readonly IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration;
        private readonly ILogger<UpdaterJobStateMachine> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdaterJobStateMachine"/> class.
        /// </summary>
        /// <param name="mediator">The mediator service.</param>
        /// <param name="stateMachineJobRepository">The state machine job repository.</param>
        /// <param name="fileSystemService">The file system service.</param>
        /// <param name="thirdPartyDataSetsConfiguration">The third party dataset configuration.</param>
        public UpdaterJobStateMachine(
            ICqrsMediator mediator,
            IStateMachineJobsRepository stateMachineJobRepository,
            IFileSystemService fileSystemService,
            IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration,
            ILogger<UpdaterJobStateMachine> logger)
            : base(stateMachineJobRepository, UpdaterJobType.Gnaf)
        {
            this.mediator = mediator;
            this.fileSystemService = fileSystemService;
            this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public override StateMachineJob CreateAndSaveUpdaterJobStateMachine(Guid id, Instant createdTimestamp, string hangFireId, IUpdaterJobManifest updaterJobManifest)
        {
            var jsonStringUpdaterJobManifest = JsonConvert.SerializeObject(updaterJobManifest);

            return this.CreateAndSaveUpdaterJobStateMachine(
                id, createdTimestamp, hangFireId, UpdaterJobType.Gnaf, UpdaterJobState.Queued.Humanize(), updaterJobManifest.DownloadUrls?.FirstOrDefault().Url, jsonStringUpdaterJobManifest);
        }

        /// <inheritdoc />
        public override async Task ResumeUpdaterJob(Guid updaterJobId, CancellationToken cancellationToken)
        {
            var stateMachineJob = this.StateMachineJobRepository.GetById(updaterJobId);

            if (stateMachineJob == null)
            {
                return;
            }

            var stateMachineJobManifest = JsonConvert.DeserializeObject<UpdaterJobManifest>(stateMachineJob.StateMachineJobManifest);

            this.SetupStateMachineFromReentryState(
                stateMachineJob.State,
                async () =>
                    {
                        this.Configure(UpdaterJobState.Queued.Humanize())
                            .OnActivateAsync(this.QueueActivatedAsync)
                            .Permit(UpdaterJobTrigger.GnafUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                            .Permit(UpdaterJobTrigger.GnafUpdaterJobQueued.Humanize(), UpdaterJobState.Downloading.Humanize())
                            .Permit(UpdaterJobTrigger.GnafUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                        this.Configure(UpdaterJobState.Downloading.Humanize())
                            .OnEntryAsync(() => this.DownloadFilesAsync(stateMachineJobManifest, stateMachineJob.Id, cancellationToken))
                            .Permit(UpdaterJobTrigger.GnafUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                            .Permit(UpdaterJobTrigger.GnafUpdaterDownloadCompleted.Humanize(), UpdaterJobState.Extracting.Humanize())
                            .Permit(UpdaterJobTrigger.GnafUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                        this.Configure(UpdaterJobState.Extracting.Humanize())
                            .OnEntryAsync(() => this.ExtractArchiveAsync(stateMachineJobManifest, stateMachineJob.Id))
                            .Permit(UpdaterJobTrigger.GnafUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                            .Permit(UpdaterJobTrigger.GnafUpdaterExtractArchiveCompleted.Humanize(), UpdaterJobState.CreatingTablesAndSchema.Humanize())
                            .Permit(UpdaterJobTrigger.GnafUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                        this.Configure(UpdaterJobState.CreatingTablesAndSchema.Humanize())
                            .Permit(UpdaterJobTrigger.GnafUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                            .OnEntryAsync(() => this.GnafCreateTablesAndSchemaCommandAsync(stateMachineJobManifest, stateMachineJob.Id))
                            .Permit(UpdaterJobTrigger.GnafUpdaterCreateTablesAndSchemaInStagingCompleted.Humanize(), UpdaterJobState.BuildingForeignKeysAndIndexes.Humanize())
                            .Permit(UpdaterJobTrigger.GnafUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                        this.Configure(UpdaterJobState.BuildingForeignKeysAndIndexes.Humanize())
                           .Permit(UpdaterJobTrigger.GnafUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                           .OnEntryAsync(() => this.BuildForeignKeysAndIndexes(stateMachineJobManifest, stateMachineJob.Id))
                           .Permit(UpdaterJobTrigger.GnafBuildingForeignKeysAndIndexesCompleted.Humanize(), UpdaterJobState.ImportingDelimiterSeparatedValuesToGnafDatabase.Humanize())
                           .Permit(UpdaterJobTrigger.GnafUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                        this.Configure(UpdaterJobState.ImportingDelimiterSeparatedValuesToGnafDatabase.Humanize())
                            .Permit(UpdaterJobTrigger.GnafUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                            .OnEntryAsync(() => this.ImportDelimiterSeparatedValuesToGnafDatabase(stateMachineJobManifest, stateMachineJob.Id))
                            .Permit(UpdaterJobTrigger.GnafUpdaterImportDelimiterSeparatedValuesToGnafDatabaseCompleted.Humanize(), UpdaterJobState.BuildingAddressSearchIndex.Humanize())
                            .Permit(UpdaterJobTrigger.GnafUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                        this.Configure(UpdaterJobState.BuildingAddressSearchIndex.Humanize())
                            .Permit(UpdaterJobTrigger.GnafUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                            .OnEntryAsync(() => this.BuildAddressSearchIndex(stateMachineJob.Id))
                            .Permit(UpdaterJobTrigger.GnafBuildingSearchIndexCompleted.Humanize(), UpdaterJobState.Completed.Humanize())
                            .Permit(UpdaterJobTrigger.GnafUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                        this.Configure(UpdaterJobState.Completed.Humanize())
                            .Permit(UpdaterJobTrigger.GnafUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                            .Permit(UpdaterJobTrigger.GnafUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                        try
                        {
                            await this.ActivateAsync();
                        }
                        catch (Exception error)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                await this.FireAsync(RedBookUpdaterJob.UpdaterJobTrigger.RedBookUpdaterJobCancelled.Humanize());
                                this.logger.LogInformation($"{UpdaterJobState.Cancelled.Humanize()}...");
                                return;
                            }

                            var updaterJobForEdit = this.StateMachineJobRepository.GetById(updaterJobId);
                            updaterJobForEdit.UpdateSerializedError(JsonConvert.SerializeObject(error));
                            this.StateMachineJobRepository.SaveChanges();

                            await this.FireAsync(UpdaterJobTrigger.GnafUpdaterAborted.Humanize());
                            this.logger.LogInformation(error.Message);
                        }
                    },
                (state) =>
                {
                    this.StateMachineJobRepository.UpdateStateMachineCurrentState(updaterJobId, state);
                });

            await this.WaitForCompletion(cancellationToken, () => this.PersistentState == UpdaterJobState.Completed.Humanize() ||
                                                                  this.PersistentState == UpdaterJobState.Cancelled.Humanize() ||
                                                                  this.PersistentState == UpdaterJobState.Aborted.Humanize());
        }

        /// <inheritdoc />
        public override async Task<JobStatusResponse> CancelUpdaterJob(Guid updaterJobId)
        {
            var stateMachineJob = this.StateMachineJobRepository.GetById(updaterJobId);

            if (stateMachineJob == null)
            {
                throw new ErrorException(Errors.ThirdPartyDataSets.NotFound(updaterJobId));
            }

            stateMachineJob.State = RedBookUpdaterJob.UpdaterJobState.Cancelled.ToString();
            this.StateMachineJobRepository.SaveChanges();
            return await Task.FromResult(new JobStatusResponse(stateMachineJob));
        }

        private async Task QueueActivatedAsync()
        {
            this.logger.LogInformation($"{UpdaterJobState.Queued.Humanize()}...");
            await this.FireAsync(UpdaterJobTrigger.GnafUpdaterJobQueued.Humanize());
        }

        private async Task DownloadFilesAsync(IUpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.logger.LogInformation($"{UpdaterJobState.Downloading.Humanize()}...");
            var stateMachineJob = this.StateMachineJobRepository.GetById(jobId);
            stateMachineJob.DatasetUrl = updaterJobManifest.DownloadUrls?.FirstOrDefault().Url;
            this.StateMachineJobRepository.SaveChanges();

            var downloadedFiles = await this.mediator.Send(new DownloadFilesCommand(updaterJobManifest, jobId, UpdaterJobType.Gnaf), cancellationToken);

            if (!downloadedFiles.Any())
            {
                await this.FireAsync(UpdaterJobTrigger.GnafUpdaterDownloadAborted.Humanize());
                return;
            }

            var existingFilesHashes = downloadedFiles
                .Where(file =>
                    this.fileSystemService.File.Exists(
                        Path.Combine(
                            this.thirdPartyDataSetsConfiguration.FileHashesPath,
                            Path.GetFileName(file.FileName))))
                .ToList();

            if (existingFilesHashes.Any())
            {
                if (!updaterJobManifest.IsForceUpdate)
                {
                    await this.FireAsync(UpdaterJobTrigger.GnafUpdaterDownloadAborted.Humanize());
                    return;
                }
            }

            stateMachineJob.IsDownloaded = true;
            this.StateMachineJobRepository.SaveChanges();

            await this.FireAsync(UpdaterJobTrigger.GnafUpdaterDownloadCompleted.Humanize());
        }

        private async Task ExtractArchiveAsync(IUpdaterJobManifest updaterJobManifest, Guid jobId)
        {
            this.logger.LogInformation($"{UpdaterJobState.Extracting.Humanize()}...");
            await this.mediator.Send(new ExtractArchivesCommand(updaterJobManifest, jobId));

            var stateMachineJob = this.StateMachineJobRepository.GetById(jobId);
            stateMachineJob.IsExtracted = true;
            this.StateMachineJobRepository.SaveChanges();

            await this.FireAsync(UpdaterJobTrigger.GnafUpdaterExtractArchiveCompleted.Humanize());
        }

        private async Task GnafCreateTablesAndSchemaCommandAsync(IUpdaterJobManifest updaterJobManifest, Guid jobId)
        {
            this.logger.LogInformation($"{UpdaterJobState.CreatingTablesAndSchema.Humanize()}...");
            await this.mediator.Send(new CreateTablesAndSchemaCommand(updaterJobManifest, jobId));
            await this.FireAsync(UpdaterJobTrigger.GnafUpdaterCreateTablesAndSchemaInStagingCompleted.Humanize());
        }

        private async Task ImportDelimiterSeparatedValuesToGnafDatabase(IUpdaterJobManifest updaterJobManifest, Guid jobId)
        {
            this.logger.LogInformation($"{UpdaterJobState.ImportingDelimiterSeparatedValuesToGnafDatabase.Humanize()}...");
            await this.mediator.Send(new ImportDelimiterSeparatedValuesCommand(updaterJobManifest, jobId));
            await this.FireAsync(UpdaterJobTrigger.GnafUpdaterImportDelimiterSeparatedValuesToGnafDatabaseCompleted.Humanize());
        }

        private async Task BuildForeignKeysAndIndexes(IUpdaterJobManifest updaterJobManifest, Guid jobId)
        {
            this.logger.LogInformation($"{UpdaterJobState.BuildingForeignKeysAndIndexes.Humanize()}...");
            await this.mediator.Send(new CreateForeignKeysAndIndexesCommand(updaterJobManifest, jobId));
            await this.FireAsync(UpdaterJobTrigger.GnafBuildingForeignKeysAndIndexesCompleted.Humanize());
        }

        private async Task BuildAddressSearchIndex(Guid jobId)
        {
            this.logger.LogInformation($"{UpdaterJobState.BuildingAddressSearchIndex.Humanize()}...");
            await this.mediator.Send(new BuildAddressSearchIndexCommand(jobId, 1, 500000));
            await this.FireAsync(UpdaterJobTrigger.GnafBuildingSearchIndexCompleted.Humanize());
        }
    }
}
