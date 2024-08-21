// <copyright file="CreateUpdaterJobCommandTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.ThirdPartyDataSets
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using FluentAssertions;
    using Hangfire;
    using Humanizer;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NodaTime;
    using UBind.Application.Commands.ThirdPartyDataSets;
    using UBind.Application.DataDownloader;
    using UBind.Application.Services.HangfireCqrs;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Application.ThirdPartyDataSets.ViewModel;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.ThirdPartyDataSets;
    using Xunit;

    public class CreateUpdaterJobCommandTest
    {
        private readonly ServiceCollection serviceCollection;

        public CreateUpdaterJobCommandTest()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IClock>(SystemClock.Instance);
            services
                .AddSingleton<ICommandHandler<CreateUpdaterJobCommand, UpdaterJobStatusResult>,
                    CreateUpdaterJobCommandHandler>();
            services.AddSingleton<IHangfireCqrsJobService, HangfireCqrsJobService>();

            var mockICqrsMediator = new Mock<ICqrsMediator>();
            services.AddSingleton(mockICqrsMediator.Object);

            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            services.AddSingleton(mockBackgroundJobClient.Object);

            var mockUpdaterJobStateMachine = new Mock<IUpdaterJob>();
            services.AddSingleton(mockUpdaterJobStateMachine.Object);

            IUpdaterJob machineRedBookObject = this.CreateUpdaterJobStateMachineObject("Redbook", "311180", UpdaterJobType.RedBook.Humanize());
            services.AddSingleton(machineRedBookObject);
            services.AddSingleton(this.CreateStateMachineJobsRespository("311180", UpdaterJobType.RedBook.Humanize()));
            services.AddSingleton(this.CreateUpdaterJobFactoryObject(machineRedBookObject));

            IUpdaterJob machineGlassGuideObject = this.CreateUpdaterJobStateMachineObject("GlassGuide", "311181", UpdaterJobType.GlassGuide.Humanize());
            services.AddSingleton(machineGlassGuideObject);
            services.AddSingleton(this.CreateStateMachineJobsRespository("311181", UpdaterJobType.GlassGuide.Humanize()));
            services.AddSingleton(this.CreateUpdaterJobFactoryObject(machineGlassGuideObject));

            this.serviceCollection = services;
        }

        [Fact]
        public async Task CreateUpdaterJobCommand_Should_CreateUpdaterJob()
        {
            ////// Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var updaterJobStateMachine = service.GetService<IUpdaterJob>();
            var sut = service.GetService<ICommandHandler<CreateUpdaterJobCommand, UpdaterJobStatusResult>>();

            Assert.NotNull(updaterJobStateMachine);
            Assert.NotNull(sut);

            var downloadUrls = new (string Url, string FileHash, string fileName)[] { (string.Empty, string.Empty, string.Empty) };
            var updaterJobManifest = new UpdaterJobManifest(DataDownloaderProtocol.Http, downloadUrls, false);

            ///// Act

            var result = await sut.Handle(
                                       new CreateUpdaterJobCommand(
                                           updaterJobStateMachine.GetType(),
                                           updaterJobManifest),
                                       CancellationToken.None);

            ///// Assert
            sut.Should().NotBeNull();
        }

        private IUpdaterJob CreateUpdaterJobStateMachineObject(string machineJobType, string jobId, string humanizedJobType)
        {
            var mockUpdaterJobStateMachine = new Mock<IUpdaterJob>();

            mockUpdaterJobStateMachine.Setup(m => m.GetUpdaterJobStatusResult(It.IsAny<Guid>())).Returns(() =>
            {
                var stateMachineJob = new StateMachineJob(Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant(), humanizedJobType, jobId, string.Empty, string.Empty);

                stateMachineJob.SetState("Downloading");

                var jobStatus = new JobStatusResponse(stateMachineJob);
                var updaterJobStatusResult = new UpdaterJobStatusResult(stateMachineJob);
                updaterJobStatusResult.SetJobStatusResult(Result.Success<JobStatusResponse, Error>(jobStatus));

                return updaterJobStatusResult;
            });

            mockUpdaterJobStateMachine
                .Setup(
                    m => m.CreateAndSaveUpdaterJobStateMachine(
                        It.IsAny<Guid>(),
                        It.IsAny<Instant>(),
                        It.IsAny<string>(),
                        It.IsAny<UpdaterJobManifest>())).Returns(
                    new StateMachineJob(Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant(), machineJobType, jobId, string.Empty, string.Empty));

            mockUpdaterJobStateMachine.Setup(m => m.GetUpdaterJobStatus(It.IsAny<Guid>())).Returns(
                () =>
                {
                    var stateMachineJob = new StateMachineJob(Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant(), humanizedJobType, jobId, string.Empty, string.Empty);
                    var jobStatus = new JobStatusResponse(stateMachineJob);
                    return jobStatus;
                });

            return mockUpdaterJobStateMachine.Object;
        }

        private IStateMachineJobsRepository CreateStateMachineJobsRespository(string jobId, string humanizedJobType)
        {
            var mockStateMachineJobsRepository = new Mock<IStateMachineJobsRepository>();

            mockStateMachineJobsRepository.Setup(m => m.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(
                new StateMachineJob(
                    Guid.NewGuid(),
                    SystemClock.Instance.GetCurrentInstant(),
                    humanizedJobType,
                    jobId,
                    string.Empty,
            string.Empty));

            return mockStateMachineJobsRepository.Object;
        }

        private IUpdaterJobFactory CreateUpdaterJobFactoryObject(IUpdaterJob jobStateMachineObject)
        {
            var mockUpdaterJobFactory = new Mock<IUpdaterJobFactory>();
            mockUpdaterJobFactory.Setup(m => m.GetUpdaterJob(It.IsAny<Type>()))
                .Returns(jobStateMachineObject);

            return mockUpdaterJobFactory.Object;
        }
    }
}
