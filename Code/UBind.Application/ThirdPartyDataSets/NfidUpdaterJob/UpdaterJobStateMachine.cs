// <copyright file="UpdaterJobStateMachine.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets.NfidUpdaterJob
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Flurl.Http;
    using Humanizer;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Commands.ThirdPartyDataSets;
    using UBind.Application.Commands.ThirdPartyDataSets.Nfid;
    using UBind.Application.ThirdPartyDataSets.ViewModel;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Repositories.FileSystem;
    using UBind.Domain.ThirdPartyDataSets;

    public class UpdaterJobStateMachine : BaseUpdaterJob
    {
        private readonly ICqrsMediator mediator;
        private readonly IFileSystemService fileSystemService;
        private readonly IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration;
        private readonly IClock clock;
        private readonly ILogger<UpdaterJobStateMachine> logger;

        public UpdaterJobStateMachine(
            ICqrsMediator mediator,
            IFileSystemService fileSystemService,
            IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration,
            IStateMachineJobsRepository stateMachineJobRepository,
            IClock clock,
            ILogger<UpdaterJobStateMachine> logger)
            : base(stateMachineJobRepository, UpdaterJobType.Nfid)
        {
            this.mediator = mediator;
            this.fileSystemService = fileSystemService;
            this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
            this.clock = clock;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override StateMachineJob CreateAndSaveUpdaterJobStateMachine(Guid id, Instant createdTimestamp, string hangFireId, IUpdaterJobManifest updaterJobManifest)
        {
            var jsonStringUpdaterJobManifest = JsonConvert.SerializeObject(updaterJobManifest);
            return this.CreateAndSaveUpdaterJobStateMachine(
                id, createdTimestamp, hangFireId, UpdaterJobType.Nfid, UpdaterJobState.Queued.Humanize(), updaterJobManifest.DownloadUrls?.FirstOrDefault().Url, jsonStringUpdaterJobManifest);
        }

        /// <inheritdoc />
        public async override Task ResumeUpdaterJob(Guid updaterJobId, CancellationToken cancellationToken)
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
                        .OnActivateAsync(() => this.QueueActivatedAsync(stateMachineJobManifest, stateMachineJob.Id))
                        .Permit(UpdaterJobTrigger.NfidUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                        .Permit(UpdaterJobTrigger.NfidUpdaterJobQueued.Humanize(), UpdaterJobState.Downloading.Humanize())
                        .Permit(UpdaterJobTrigger.NfidUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                   this.Configure(UpdaterJobState.Downloading.Humanize())
                        .OnEntryAsync(() => this.DownloadFilesAsync(stateMachineJobManifest, stateMachineJob.Id, cancellationToken))
                        .Permit(UpdaterJobTrigger.NfidUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                        .Permit(UpdaterJobTrigger.NfidUpdaterDownloadCompleted.Humanize(), UpdaterJobState.Extracting.Humanize())
                        .Permit(UpdaterJobTrigger.NfidUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                   this.Configure(UpdaterJobState.Extracting.Humanize())
                        .OnEntryAsync(() => this.ExtractArchiveAsync(stateMachineJobManifest, stateMachineJob.Id, cancellationToken))
                        .Permit(UpdaterJobTrigger.NfidUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                        .Permit(UpdaterJobTrigger.NfidUpdaterExtractArchiveCompleted.Humanize(), UpdaterJobState.CreatingTablesAndSchema.Humanize())
                        .Permit(UpdaterJobTrigger.NfidUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                   this.Configure(UpdaterJobState.CreatingTablesAndSchema.Humanize())
                        .Permit(UpdaterJobTrigger.NfidUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                        .OnEntryAsync(() => this.NfidCreateTablesAndSchemaCommandAsync(stateMachineJobManifest, stateMachineJob.Id, cancellationToken))
                        .Permit(UpdaterJobTrigger.NfidUpdaterCreateTablesAndSchemaInStagingCompleted.Humanize(), UpdaterJobState.ImportingDelimiterSeparatedValuesToNfidDatabase.Humanize())
                        .Permit(UpdaterJobTrigger.NfidUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                   this.Configure(UpdaterJobState.ImportingDelimiterSeparatedValuesToNfidDatabase.Humanize())
                        .Permit(UpdaterJobTrigger.NfidUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                        .OnEntryAsync(() => this.ImportDelimiterSeparatedValuesToNfidDatabase(stateMachineJobManifest, stateMachineJob.Id, cancellationToken))
                        .Permit(UpdaterJobTrigger.NfidUpdaterImportDelimiterSeparatedValuesToNfidDatabaseCompleted.Humanize(), UpdaterJobState.BuildingNfidSearchIndex.Humanize())
                        .Permit(UpdaterJobTrigger.NfidUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                   this.Configure(UpdaterJobState.BuildingNfidSearchIndex.Humanize())
                        .Permit(UpdaterJobTrigger.NfidUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                        .OnEntryAsync(() => this.BuildNfidSearchIndex(stateMachineJob.Id))
                        .Permit(UpdaterJobTrigger.NfidBuildingSearchIndexCompleted.Humanize(), UpdaterJobState.ArchiveDownloadedFiles.Humanize())
                        .Permit(UpdaterJobTrigger.NfidUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                   this.Configure(UpdaterJobState.ArchiveDownloadedFiles.Humanize())
                         .OnEntryAsync(() => this.ArchiveDownloadedFilesAsync(stateMachineJobManifest, stateMachineJob.Id, cancellationToken))
                         .Permit(UpdaterJobTrigger.NfidUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                         .Permit(UpdaterJobTrigger.NfidArchiveDownloadedFilesCompleted.Humanize(), UpdaterJobState.CleanUpUpdaterJob.Humanize())
                         .Permit(UpdaterJobTrigger.NfidUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                   this.Configure(UpdaterJobState.CleanUpUpdaterJob.Humanize())
                       .OnEntryAsync(() => this.CleanUpUpdaterJobAsync(stateMachineJobManifest, stateMachineJob.Id, cancellationToken))
                       .Permit(UpdaterJobTrigger.NfidUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                       .Permit(UpdaterJobTrigger.NfidCleanUpUpdaterJobCompleted.Humanize(), UpdaterJobState.Completed.Humanize())
                       .Permit(UpdaterJobTrigger.NfidUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                   this.Configure(UpdaterJobState.Completed.Humanize())
                        .Permit(UpdaterJobTrigger.NfidUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                        .Permit(UpdaterJobTrigger.NfidUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                   try
                   {
                       await this.ActivateAsync();
                   }
                   catch (Exception error)
                   {
                       stateMachineJob.SetEndTime(this.clock.Now());
                       if (cancellationToken.IsCancellationRequested)
                       {
                           await this.FireAsync(UpdaterJobTrigger.NfidUpdaterJobCancelled.Humanize());
                           this.logger.LogInformation($"{UpdaterJobState.Cancelled.Humanize()}...");
                           return;
                       }

                       var updaterJobForEdit = this.StateMachineJobRepository.GetById(updaterJobId);
                       updaterJobForEdit.UpdateSerializedError(JsonConvert.SerializeObject(error));
                       this.StateMachineJobRepository.SaveChanges();

                       await this.FireAsync(UpdaterJobTrigger.NfidUpdaterAborted.Humanize());
                       this.logger.LogInformation(error.Message);
                   }
               },
               (state) =>
               {
                   this.StateMachineJobRepository.UpdateStateMachineCurrentState(updaterJobId, state);
               });

            await this.WaitForCompletion(
                cancellationToken,
                () => this.PersistentState == UpdaterJobState.Completed.Humanize()
                    || this.PersistentState == UpdaterJobState.Cancelled.Humanize()
                    || this.PersistentState == UpdaterJobState.Aborted.Humanize());
        }

        /// <inheritdoc />
        public async override Task<JobStatusResponse> CancelUpdaterJob(Guid updaterJobId)
        {
            var stateMachineJob = this.StateMachineJobRepository.GetById(updaterJobId);
            stateMachineJob.SetEndTime(this.clock.Now());

            if (stateMachineJob == null)
            {
                throw new ErrorException(Errors.ThirdPartyDataSets.NotFound(updaterJobId));
            }

            stateMachineJob.State = UpdaterJobState.Cancelled.ToString();
            this.StateMachineJobRepository.SaveChanges();
            return await Task.FromResult(new JobStatusResponse(stateMachineJob));
        }

        private async Task QueueActivatedAsync(UpdaterJobManifest updaterJobManifest, Guid jobId)
        {
            this.logger.LogInformation($"{UpdaterJobState.Queued.Humanize()}...");
            await this.FireAsync(UpdaterJobTrigger.NfidUpdaterJobQueued.Humanize());
        }

        private async Task DownloadFilesAsync(UpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.logger.LogInformation($"{UpdaterJobState.Downloading.Humanize()}...");
            var stateMachineJob = this.StateMachineJobRepository.GetById(jobId);

            IReadOnlyList<(string FileName, string FileHash)> downloadedFiles = null;

            try
            {
                downloadedFiles = await this.mediator.Send(new DownloadFilesCommand(updaterJobManifest, jobId, UpdaterJobType.Nfid), cancellationToken);

                if (!downloadedFiles.Any())
                {
                    await this.FireAsync(UpdaterJobTrigger.NfidUpdaterDownloadAborted.Humanize());
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
                        await this.FireAsync(UpdaterJobTrigger.NfidUpdaterDownloadAborted.Humanize());
                        return;
                    }
                }

                stateMachineJob.IsDownloaded = true;
                this.StateMachineJobRepository.SaveChanges();

                await this.FireAsync(UpdaterJobTrigger.NfidUpdaterDownloadCompleted.Humanize());
            }
            catch (FlurlHttpException ex)
            {
                var updaterJobForEdit = this.StateMachineJobRepository.GetById(jobId);
                updaterJobForEdit.SetEndTime(this.clock.Now());
                var error = Errors.ThirdPartyDataSets.Nfid.DataSetCannotBeDownloaded(stateMachineJob.DatasetUrl);
                updaterJobForEdit.UpdateSerializedError(JsonConvert.SerializeObject(error));
                this.StateMachineJobRepository.SaveChanges();

                await this.FireAsync(UpdaterJobTrigger.NfidUpdaterAborted.Humanize());
                this.logger.LogInformation(ex.Message);
            }
        }

        private async Task ExtractArchiveAsync(UpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.logger.LogInformation($"{UpdaterJobState.Extracting.Humanize()}...");
            await this.mediator.Send(new ExtractArchivesCommand(updaterJobManifest, jobId), cancellationToken);

            var stateMachineJob = this.StateMachineJobRepository.GetById(jobId);
            stateMachineJob.IsExtracted = true;
            this.StateMachineJobRepository.SaveChanges();

            await this.FireAsync(UpdaterJobTrigger.NfidUpdaterExtractArchiveCompleted.Humanize());
        }

        private async Task NfidCreateTablesAndSchemaCommandAsync(IUpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.logger.LogInformation($"{UpdaterJobState.CreatingTablesAndSchema.Humanize()}...");
            await this.mediator.Send(new CreateTablesAndSchemaCommand(updaterJobManifest, jobId), cancellationToken);
            await this.FireAsync(UpdaterJobTrigger.NfidUpdaterCreateTablesAndSchemaInStagingCompleted.Humanize());
        }

        private async Task ImportDelimiterSeparatedValuesToNfidDatabase(UpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await this.mediator.Send(new ImportDelimiterSeparatedValuesCommand(updaterJobManifest, jobId), cancellationToken);
                await this.FireAsync(UpdaterJobTrigger.NfidUpdaterImportDelimiterSeparatedValuesToNfidDatabaseCompleted.Humanize());
            }
            catch (ErrorException ex)
            {
                var updaterJobForEdit = this.StateMachineJobRepository.GetById(jobId);
                updaterJobForEdit.SetEndTime(this.clock.Now());
                updaterJobForEdit.UpdateSerializedError(JsonConvert.SerializeObject(ex.Error));
                this.StateMachineJobRepository.SaveChanges();

                await this.FireAsync(UpdaterJobTrigger.NfidUpdaterAborted.Humanize());
                this.logger.LogInformation(ex.Message);
            }
        }

        private async Task ArchiveDownloadedFilesAsync(UpdaterJobManifest stateMachineJobManifest, Guid jobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.mediator.Send(new ArchiveDownloadedFilesCommand(stateMachineJobManifest, jobId), cancellationToken);
            await this.FireAsync(UpdaterJobTrigger.NfidArchiveDownloadedFilesCompleted.Humanize());
        }

        private async Task CleanUpUpdaterJobAsync(UpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
        {
            await this.mediator.Send(new CleanUpUpdaterJobCommand(updaterJobManifest, jobId), cancellationToken);

            var stateMachineJob = this.StateMachineJobRepository.GetById(jobId);
            stateMachineJob.SetEndTime(this.clock.Now());
            this.StateMachineJobRepository.SaveChanges();

            await this.FireAsync(UpdaterJobTrigger.NfidCleanUpUpdaterJobCompleted.Humanize());
        }

        private async Task BuildNfidSearchIndex(Guid jobId)
        {
            this.logger.LogInformation($"{UpdaterJobState.BuildingNfidSearchIndex.Humanize()}...");
            await this.mediator.Send(new BuildAddressSearchIndexCommand(jobId, 1, 500000));
            await this.FireAsync(UpdaterJobTrigger.NfidBuildingSearchIndexCompleted.Humanize());
        }
    }
}
