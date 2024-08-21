// <copyright file="UpdaterJobStateMachine.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets.RedBookUpdaterJob
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Commands.ThirdPartyDataSets;
    using UBind.Application.Commands.ThirdPartyDataSets.RedBook;
    using UBind.Application.Services.Email;
    using UBind.Application.ThirdPartyDataSets.ViewModel;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Repositories.FileSystem;
    using UBind.Domain.ThirdPartyDataSets;

    /// <summary>
    /// A state machine for RedBook updater jobs managing the job states and trigger.
    /// The class facilitates the workflow of the RedBook process such as
    /// 1) Downloading the zip file from RedBook FTP server.
    /// 2) Extracting the download zip file.
    /// 3) Creating the RedBook database tables and schema.
    /// 4) Importing the delimiter separated values to the RedBook database.
    /// 5) Archiving the downloaded files for future re-used via hash checker.
    /// 5) Cleaning-up updater job and free resources allocated like downloaded and extracted files.
    /// </summary>
    public class UpdaterJobStateMachine : BaseUpdaterJob
    {
        private readonly ICqrsMediator mediator;
        private readonly IFileSystemService fileSystemService;
        private readonly IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration;
        private readonly IErrorNotificationService errorNotificationService;
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
            IErrorNotificationService errorNotificationService,
            ILogger<UpdaterJobStateMachine> logger)
            : base(stateMachineJobRepository, UpdaterJobType.RedBook)
        {
            this.mediator = mediator;
            this.fileSystemService = fileSystemService;
            this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
            this.errorNotificationService = errorNotificationService;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override StateMachineJob CreateAndSaveUpdaterJobStateMachine(
            Guid id,
            Instant createdTimestamp,
            string hangFireId,
            IUpdaterJobManifest updaterJobManifest)
        {
            var jsonStringUpdaterJobManifest = JsonConvert.SerializeObject(updaterJobManifest);
            return this.CreateAndSaveUpdaterJobStateMachine(
                id,
                createdTimestamp,
                hangFireId,
                UpdaterJobType.RedBook,
                UpdaterJobState.Queued.Humanize(),
                updaterJobManifest.DownloadUrls?.FirstOrDefault().Url,
                jsonStringUpdaterJobManifest);
        }

        /// <inheritdoc/>
        public override async Task ResumeUpdaterJob(Guid updaterJobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var stateMachineJobs = this.StateMachineJobRepository.GetById(updaterJobId);

            if (stateMachineJobs == null)
            {
                return;
            }

            var stateMachineJobManifest = JsonConvert.DeserializeObject<UpdaterJobManifest>(stateMachineJobs.StateMachineJobManifest);

            this.SetupStateMachineFromReentryState(
                stateMachineJobs.State,
                async () =>
                {
                    this.Configure(UpdaterJobState.Queued.Humanize())
                        .OnActivateAsync(this.QueuedActivatedAsync)
                        .Permit(UpdaterJobTrigger.RedBookUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookUpdaterJobQueued.Humanize(), UpdaterJobState.Downloading.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                    this.Configure(UpdaterJobState.Downloading.Humanize())
                        .OnEntryAsync(() => this.DownloadFilesAsync(stateMachineJobManifest, stateMachineJobs.Id, cancellationToken))
                        .Permit(UpdaterJobTrigger.RedBookUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookUpdaterDownloadCompleted.Humanize(), UpdaterJobState.Extracting.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookUpdaterDownloadAborted.Humanize(), UpdaterJobState.Aborted.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                    this.Configure(UpdaterJobState.Extracting.Humanize())
                        .OnEntryAsync(() => this.ExtractArchiveAsync(stateMachineJobManifest, stateMachineJobs.Id, cancellationToken))
                        .Permit(UpdaterJobTrigger.RedBookUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookUpdaterExtractArchiveCompleted.Humanize(), UpdaterJobState.CreatingTablesAndSchema.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                    this.Configure(UpdaterJobState.CreatingTablesAndSchema.Humanize())
                        .OnEntryAsync(() => this.RedBookCreateTablesAndSchemaCommandAsync(stateMachineJobManifest, stateMachineJobs.Id, cancellationToken))
                        .Permit(UpdaterJobTrigger.RedBookUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookUpdaterCreateTablesAndSchemaInStagingCompleted.Humanize(), UpdaterJobState.ImportingDelimiterSeparatedValuesToRedBookDatabase.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                    this.Configure(UpdaterJobState.ImportingDelimiterSeparatedValuesToRedBookDatabase.Humanize())
                        .OnEntryAsync(() => this.ImportDelimiterSeparatedValuesToRedBookDatabase(stateMachineJobManifest, stateMachineJobs.Id, cancellationToken))
                        .Permit(UpdaterJobTrigger.RedBookUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookUpdaterImportDelimiterSeparatedValuesToRedBookDatabaseCompleted.Humanize(), UpdaterJobState.ArchiveDownloadedFiles.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                    this.Configure(UpdaterJobState.ArchiveDownloadedFiles.Humanize())
                        .OnEntryAsync(() => this.ArchiveDownloadedFilesAsync(stateMachineJobManifest, stateMachineJobs.Id, cancellationToken))
                        .Permit(UpdaterJobTrigger.RedBookUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookArchiveDownloadedFilesCompleted.Humanize(), UpdaterJobState.CleanUpUpdaterJob.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                    this.Configure(UpdaterJobState.CleanUpUpdaterJob.Humanize())
                        .OnEntryAsync(() => this.CleanUpUpdaterJobAsync(stateMachineJobManifest, stateMachineJobs.Id, cancellationToken))
                        .Permit(UpdaterJobTrigger.RedBookUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookCleanUpUpdaterJobCompleted.Humanize(), UpdaterJobState.Completed.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                    this.Configure(UpdaterJobState.Completed.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                        .Permit(UpdaterJobTrigger.RedBookUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                    try
                    {
                        await this.ActivateAsync();
                    }
                    catch (Exception error)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            this.logger.LogError(error, "Cancellation is requested on RedBookUpdaterJobStateMachine");
                        }

                        // This is added because sometimes the StateMachineJobRepository is already null or disposed when the cancellation has occured.
                        try
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                await this.FireAsync(UpdaterJobTrigger.RedBookUpdaterJobCancelled.Humanize());
                                return;
                            }

                            if (this.StateMachineJobRepository != null)
                            {
                                var updaterJobForEdit = this.StateMachineJobRepository.GetById(updaterJobId);
                                updaterJobForEdit.UpdateSerializedError(JsonConvert.SerializeObject(error));
                                this.StateMachineJobRepository.SaveChanges();
                            }

                            await this.FireAsync(UpdaterJobTrigger.RedBookUpdaterAborted.Humanize());
                            this.SendEmailForFailedRedbookUpdateJob(updaterJobId, error);
                        }
                        catch (Exception ex)
                        {
                            // This log is added to signify that the StateMachineJobRepository has nulled or disposed.
                            this.logger.LogError(ex, "RedBookUpdaterJobStateMachine update error occured when recieving cancellation request");
                        }
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
        public override async Task<JobStatusResponse> CancelUpdaterJob(Guid requestUpdateJobId)
        {
            var stateMachineJob = this.StateMachineJobRepository.GetById(requestUpdateJobId);

            if (stateMachineJob == null)
            {
                throw new ErrorException(Errors.ThirdPartyDataSets.NotFound(requestUpdateJobId));
            }

            stateMachineJob.State = UpdaterJobState.Cancelled.ToString();
            this.StateMachineJobRepository.SaveChanges();
            return await Task.FromResult(new JobStatusResponse(stateMachineJob));
        }

        private async Task QueuedActivatedAsync()
        {
            await this.FireAsync(UpdaterJobTrigger.RedBookUpdaterJobQueued.Humanize());
        }

        private async Task DownloadFilesAsync(IUpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var downloadedFiles = await this.mediator.Send(new DownloadFilesCommand(updaterJobManifest, jobId, UpdaterJobType.RedBook), cancellationToken);

            var stateMachineJob = this.StateMachineJobRepository.GetById(jobId);
            stateMachineJob.DatasetUrl = downloadedFiles?.FirstOrDefault().FileName;
            this.StateMachineJobRepository.SaveChanges();

            if (!downloadedFiles.Any())
            {
                await this.FireAsync(UpdaterJobTrigger.RedBookUpdaterDownloadAborted.Humanize());
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
                    await this.FireAsync(UpdaterJobTrigger.RedBookUpdaterDownloadAborted.Humanize());
                    return;
                }
            }

            stateMachineJob.IsDownloaded = true;
            this.StateMachineJobRepository.SaveChanges();

            await this.FireAsync(UpdaterJobTrigger.RedBookUpdaterDownloadCompleted.Humanize());
        }

        private async Task ExtractArchiveAsync(IUpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.mediator.Send(new ExtractArchivesCommand(updaterJobManifest, jobId), cancellationToken);

            var stateMachineJob = this.StateMachineJobRepository.GetById(jobId);
            stateMachineJob.IsExtracted = true;
            this.StateMachineJobRepository.SaveChanges();

            await this.FireAsync(UpdaterJobTrigger.RedBookUpdaterExtractArchiveCompleted.Humanize());
        }

        private async Task RedBookCreateTablesAndSchemaCommandAsync(IUpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.mediator.Send(new CreateTablesAndSchemaCommand(updaterJobManifest, jobId), cancellationToken);
            await this.FireAsync(UpdaterJobTrigger.RedBookUpdaterCreateTablesAndSchemaInStagingCompleted.Humanize());
        }

        private async Task ImportDelimiterSeparatedValuesToRedBookDatabase(IUpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.mediator.Send(new ImportDelimiterSeparatedValuesCommand(updaterJobManifest, jobId), cancellationToken);
            await this.FireAsync(UpdaterJobTrigger.RedBookUpdaterImportDelimiterSeparatedValuesToRedBookDatabaseCompleted.Humanize());
        }

        private async Task ArchiveDownloadedFilesAsync(IUpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.mediator.Send(new ArchiveDownloadedFilesCommand(updaterJobManifest, jobId), cancellationToken);
            await this.FireAsync(UpdaterJobTrigger.RedBookArchiveDownloadedFilesCompleted.Humanize());
        }

        private async Task CleanUpUpdaterJobAsync(IUpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.mediator.Send(new CleanUpUpdaterJobCommand(updaterJobManifest, jobId), cancellationToken);
            await this.FireAsync(UpdaterJobTrigger.RedBookCleanUpUpdaterJobCompleted.Humanize());
        }

        private void SendEmailForFailedRedbookUpdateJob(Guid jobId, Exception exception)
        {
            var message = new StringBuilder();
            message.AppendLine($"{this.GetType().Name} with an id of {jobId} has failed to complete.");
            message.AppendLine("<br/>");
            message.AppendLine("Please investigate the following exception and retry manually. "
                    + "The Redbook updater job can be started by going to the swagger page, "
                    + "scrolling down to the VehicleTypes section and triggering it manually. "
                    + "Before starting, please check that no other Redbook updater job is already running.");
            message.AppendLine("<br/>");
            message.AppendLine("<br/>");
            message.AppendLine("The following contains details of the exception:");
            message.AppendLine("<br/>");
            message.AppendLine("Exception:");
            message.AppendLine(exception.Message);
            message.AppendLine("<br/>");
            message.AppendLine("Stacktrace:");
            message.AppendLine(exception.StackTrace);
            if (exception.InnerException != null)
            {
                message.AppendLine("Inner Exception:");
                message.AppendLine(exception.InnerException.Message);
                message.AppendLine("<br/>");
                message.AppendLine("Inner Exception Stacktrace:");
                message.AppendLine(exception.InnerException.StackTrace);
            }

            this.errorNotificationService.SendSystemNotificationEmail(
                "uBind: The Redbook updater job has failed to complete.", message.ToString());
        }
    }
}
