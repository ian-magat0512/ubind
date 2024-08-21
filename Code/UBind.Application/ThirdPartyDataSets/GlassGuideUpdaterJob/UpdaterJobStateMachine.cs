// <copyright file="UpdaterJobStateMachine.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;

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
using UBind.Application.Commands.ThirdPartyDataSets.GlassGuide;
using UBind.Application.Services.Email;
using UBind.Application.ThirdPartyDataSets.ViewModel;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.Extensions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;
using UBind.Domain.Repositories.FileSystem;
using UBind.Domain.ThirdPartyDataSets;

/// <summary>
/// A state machine for Glass's Guide updater jobs managing the job states and trigger.
/// The class facilitates the workflow of the Glass's Guide process such as
/// 1) Downloading the zip file from Glass's Guide FTP server.
/// 2) Extracting the download zip file.
/// 3) Creating the Glass's Guide database tables and schema.
/// 4) Importing the delimiter separated values to the Glass's Guide database.
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
    private readonly IClock clock;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdaterJobStateMachine"/> class.
    /// </summary>
    /// <param name="mediator">The mediator service.</param>
    /// <param name="stateMachineJobRepository">The state machine job repository.</param>
    /// <param name="fileSystemService">The file system service.</param>
    /// <param name="thirdPartyDataSetsConfiguration">The third party dataset configuration.</param>
    /// <param name="errorNotificationService">The error notification service for sending mails.</param>
    /// <param name="logger">The generic logger.</param>
    /// <param name="clock">The clock instance.</param>
    public UpdaterJobStateMachine(
        ICqrsMediator mediator,
        IStateMachineJobsRepository stateMachineJobRepository,
        IFileSystemService fileSystemService,
        IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration,
        IErrorNotificationService errorNotificationService,
        ILogger<UpdaterJobStateMachine> logger,
        IClock clock)
        : base(stateMachineJobRepository, UpdaterJobType.GlassGuide)
    {
        this.mediator = mediator;
        this.fileSystemService = fileSystemService;
        this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
        this.errorNotificationService = errorNotificationService;
        this.logger = logger;
        this.clock = clock;
    }

    /// <inheritdoc />
    public override StateMachineJob CreateAndSaveUpdaterJobStateMachine(
        Guid id,
        Instant createdTimestamp,
        string hangFireId,
        IUpdaterJobManifest updaterJobManifest)
    {
        var jsonStringUpdaterJobManifest = JsonConvert.SerializeObject(updaterJobManifest);
        var url = updaterJobManifest.DownloadUrls?.FirstOrDefault().Url;

        return this.CreateAndSaveUpdaterJobStateMachine(
            id,
            createdTimestamp,
            hangFireId,
            UpdaterJobType.GlassGuide,
            UpdaterJobState.Queued.Humanize(),
            url ?? string.Empty,
            jsonStringUpdaterJobManifest);
    }

    /// <inheritdoc/>
    public override async Task ResumeUpdaterJob(Guid updaterJobId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var stateMachineJob = this.StateMachineJobRepository.GetById(updaterJobId);

        if (stateMachineJob == null)
        {
            this.logger.LogError("GlassGuideUpdaterJobStateMachine not found.");
            return;
        }

        var stateMachineJobManifest = JsonConvert.DeserializeObject<UpdaterJobManifest>(stateMachineJob.StateMachineJobManifest);

        if (stateMachineJobManifest == null)
        {
            this.logger.LogError("GlassGuideUpdaterJobStateMachine job manifest not found.");
            return;
        }

        this.SetupStateMachineFromReentryState(
            stateMachineJob.State,
            async () =>
            {
                this.Configure(UpdaterJobState.Queued.Humanize())
                    .OnActivateAsync(this.QueuedActivatedAsync)
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterJobQueued.Humanize(), UpdaterJobState.Downloading.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                this.Configure(UpdaterJobState.Downloading.Humanize())
                    .OnEntryAsync(() => this.DownloadFilesAsync(stateMachineJobManifest, stateMachineJob.Id, cancellationToken))
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterDownloadCompleted.Humanize(), UpdaterJobState.Extracting.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterDownloadAborted.Humanize(), UpdaterJobState.Aborted.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                this.Configure(UpdaterJobState.Extracting.Humanize())
                    .OnEntryAsync(() => this.ExtractArchiveAsync(stateMachineJobManifest, stateMachineJob.Id, cancellationToken))
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterExtractArchiveCompleted.Humanize(), UpdaterJobState.CreatingTablesAndSchema.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                this.Configure(UpdaterJobState.CreatingTablesAndSchema.Humanize())
                    .OnEntryAsync(() => this.GlassGuideCreateTablesAndSchemaCommandAsync(cancellationToken))
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterCreateTablesAndSchemaInStagingCompleted.Humanize(), UpdaterJobState.ImportingFixedWidthValuesToGlassGuideDatabase.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                this.Configure(UpdaterJobState.ImportingFixedWidthValuesToGlassGuideDatabase.Humanize())
                    .OnEntryAsync(() => this.ImportFixedWidthValuesToGlassGuideDatabase(stateMachineJob.Id, cancellationToken))
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterImportFixedWidthValuesToGlassGuideDatabaseCompleted.Humanize(), UpdaterJobState.ArchiveDownloadedFiles.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                this.Configure(UpdaterJobState.ArchiveDownloadedFiles.Humanize())
                    .OnEntryAsync(() => this.ArchiveDownloadedFilesAsync(stateMachineJobManifest, stateMachineJob.Id, cancellationToken))
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideArchiveDownloadedFilesCompleted.Humanize(), UpdaterJobState.CleanUpUpdaterJob.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                this.Configure(UpdaterJobState.CleanUpUpdaterJob.Humanize())
                    .OnEntryAsync(() => this.CleanUpUpdaterJobAsync(stateMachineJobManifest, stateMachineJob.Id, cancellationToken))
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideCleanUpUpdaterJobCompleted.Humanize(), UpdaterJobState.Completed.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                this.Configure(UpdaterJobState.Completed.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterJobCancelled.Humanize(), UpdaterJobState.Cancelled.Humanize())
                    .Permit(UpdaterJobTrigger.GlassGuideUpdaterAborted.Humanize(), UpdaterJobState.Aborted.Humanize());

                try
                {
                    await this.ActivateAsync();
                }
                catch (Exception error)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        this.logger.LogError(error, "Cancellation is requested on GlassGuideUpdaterJobStateMachine");
                    }

                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            // This is added because sometimes the StateMachineJobRepository is already null or disposed when the cancellation has occured.
                            if (this.StateMachineJobRepository != null)
                            {
                                var updaterJob = this.StateMachineJobRepository.GetById(updaterJobId);
                                updaterJob.SetEndTime(this.clock.Now());
                                this.StateMachineJobRepository.SaveChanges();
                            }
                            await this.FireAsync(UpdaterJobTrigger.GlassGuideUpdaterJobCancelled.Humanize());
                            return;
                        }

                        // This is added because sometimes the StateMachineJobRepository is already null or disposed when the cancellation has occured.
                        if (this.StateMachineJobRepository != null)
                        {
                            var updaterJob = this.StateMachineJobRepository.GetById(updaterJobId);
                            updaterJob.UpdateSerializedError(JsonConvert.SerializeObject(error));
                            this.StateMachineJobRepository.SaveChanges();
                        }

                        await this.FireAsync(UpdaterJobTrigger.GlassGuideUpdaterAborted.Humanize());
                        this.SendEmailForFailedGlassGuideUpdateJob(updaterJobId, error);
                    }
                    catch (Exception ex)
                    {
                        // This log is added to signify that the StateMachineJobRepository has nulled or disposed.
                        this.logger.LogError(ex, "GlassGuideUpdaterJobStateMachine update error occured while processing error exception.");
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
        stateMachineJob.SetEndTime(this.clock.Now());
        this.StateMachineJobRepository.SaveChanges();
        return await Task.FromResult(new JobStatusResponse(stateMachineJob));
    }

    private async Task QueuedActivatedAsync()
    {
        await this.FireAsync(UpdaterJobTrigger.GlassGuideUpdaterJobQueued.Humanize());
    }

    private async Task DownloadFilesAsync(IUpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Downlading...");
        cancellationToken.ThrowIfCancellationRequested();
        var downloadedFiles = await this.mediator.Send(new DownloadFilesCommand(updaterJobManifest, jobId, UpdaterJobType.GlassGuide), cancellationToken);

        var stateMachineJob = this.StateMachineJobRepository.GetById(jobId);
        if (downloadedFiles == null)
        {
            this.logger.LogError("Failed to download files.");
            stateMachineJob.SetEndTime(this.clock.Now());
            this.StateMachineJobRepository.SaveChanges();
            await this.FireAsync(UpdaterJobTrigger.GlassGuideUpdaterDownloadAborted.Humanize());
            return;
        }

        stateMachineJob.DatasetUrl = downloadedFiles.FirstOrDefault().FileName;
        this.StateMachineJobRepository.SaveChanges();

        if (!downloadedFiles.Any())
        {
            this.logger.LogError("No files downloaded.");
            stateMachineJob.SetEndTime(this.clock.Now());
            this.StateMachineJobRepository.SaveChanges();
            await this.FireAsync(UpdaterJobTrigger.GlassGuideUpdaterDownloadAborted.Humanize());
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
                this.logger.LogError("Download files exists.");
                stateMachineJob.SetEndTime(this.clock.Now());
                this.StateMachineJobRepository.SaveChanges();
                await this.FireAsync(UpdaterJobTrigger.GlassGuideUpdaterDownloadAborted.Humanize());
                return;
            }
        }

        stateMachineJob.IsDownloaded = true;
        this.StateMachineJobRepository.SaveChanges();

        await this.FireAsync(UpdaterJobTrigger.GlassGuideUpdaterDownloadCompleted.Humanize());
    }

    private async Task ExtractArchiveAsync(IUpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Extracting...");
        cancellationToken.ThrowIfCancellationRequested();
        await this.mediator.Send(new ExtractArchivesCommand(updaterJobManifest, jobId), cancellationToken);

        var stateMachineJob = this.StateMachineJobRepository.GetById(jobId);
        stateMachineJob.IsExtracted = true;
        this.StateMachineJobRepository.SaveChanges();

        await this.FireAsync(UpdaterJobTrigger.GlassGuideUpdaterExtractArchiveCompleted.Humanize());
    }

    private async Task GlassGuideCreateTablesAndSchemaCommandAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Creating tables and schema...");
        cancellationToken.ThrowIfCancellationRequested();
        await this.mediator.Send(new CreateTablesAndSchemaCommand(Schema.GlassGuideStaging), cancellationToken);
        await this.FireAsync(UpdaterJobTrigger.GlassGuideUpdaterCreateTablesAndSchemaInStagingCompleted.Humanize());
    }

    private async Task ImportFixedWidthValuesToGlassGuideDatabase(Guid jobId, CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Importing...");
        cancellationToken.ThrowIfCancellationRequested();
        await this.mediator.Send(new ImportDelimiterSeparatedValuesCommand(jobId), cancellationToken);
        await this.FireAsync(UpdaterJobTrigger.GlassGuideUpdaterImportFixedWidthValuesToGlassGuideDatabaseCompleted.Humanize());
    }

    private async Task ArchiveDownloadedFilesAsync(IUpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Archiving...");
        cancellationToken.ThrowIfCancellationRequested();
        await this.mediator.Send(new ArchiveDownloadedFilesCommand(updaterJobManifest, jobId), cancellationToken);
        await this.FireAsync(UpdaterJobTrigger.GlassGuideArchiveDownloadedFilesCompleted.Humanize());
    }

    private async Task CleanUpUpdaterJobAsync(IUpdaterJobManifest updaterJobManifest, Guid jobId, CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Clean-up...");
        var stateMachineJob = this.StateMachineJobRepository.GetById(jobId);
        stateMachineJob.SetEndTime(this.clock.Now());
        this.StateMachineJobRepository.SaveChanges();
        cancellationToken.ThrowIfCancellationRequested();
        await this.mediator.Send(new CleanUpUpdaterJobCommand(updaterJobManifest, jobId), cancellationToken);
        await this.FireAsync(UpdaterJobTrigger.GlassGuideCleanUpUpdaterJobCompleted.Humanize());
    }

    private void SendEmailForFailedGlassGuideUpdateJob(Guid jobId, Exception exception)
    {
        this.logger.LogInformation("Sending mail...");
        var message = new StringBuilder();
        message.AppendLine($"{this.GetType().Name} with an id of {jobId} has failed to complete.");
        message.AppendLine("<br/>");
        message.AppendLine("Please investigate the following exception and retry manually. "
                + "The Glass's Guide updater job can be started by going to the swagger page, "
                + "scrolling down to the VehicleTypes section and triggering it manually. "
                + "Before starting, please check that no other Glass's Guide updater job is already running.");
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
            "uBind: The Glass's Guide updater job has failed to complete.", message.ToString());
    }
}