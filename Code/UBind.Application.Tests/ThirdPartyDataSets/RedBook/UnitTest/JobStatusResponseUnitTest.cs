// <copyright file="JobStatusResponseUnitTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.ThirdPartyDataSets.RedBook.UnitTest
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using NodaTime;
    using UBind.Application.ThirdPartyDataSets.ViewModel;
    using UBind.Domain.ThirdPartyDataSets;
    using Xunit;

    public class JobStatusResponseUnitTest
    {
        [Fact]
        public void Should_Parse_JobStatusResponseVm()
        {
            //// Arrange
            var id = Guid.Parse("852d118c-5ba1-49ef-b8b3-9076f31f6599");
            var hangFireId = "hanfire1111";
            var createdTimestamp = Instant.FromUnixTimeTicks(16073942468626104);
            var sut = new StateMachineJob(id, createdTimestamp, "redbook", hangFireId, string.Empty, string.Empty);

            sut.SetState("Download");
            sut.SetEndTime(Instant.FromUnixTimeTicks(16073942468646104));

            //// Act

            var result = new JobStatusResponse(sut);

            //// Assert
            result.Id.Should().Be(id);
            result.HangfireJobId.Should().Be(hangFireId);
            result.StartDateTime.Should().Be(createdTimestamp.ToString());
        }

        [Fact]
        public void Should_Parse_List_JobStatusResponseVm()
        {
            //// Arrange
            var id1 = Guid.Parse("852d118c-5ba1-49ef-b8b3-9076f31f6599");
            var hangFireId1 = "hanfire1111";
            var createdTimestamp1 = Instant.FromUnixTimeTicks(16073942468626104);
            var sut1 = new StateMachineJob(id1, createdTimestamp1, "redbook", hangFireId1, string.Empty, string.Empty);
            sut1.SetEndTime(Instant.FromUnixTimeTicks(16073942468646104));
            sut1.SetState("Download");

            var id2 = Guid.Parse("122d118c-5ba1-49ef-b8b3-9076f31f65ff");
            var hangFireId2 = "hanfire222";
            var createdTimestamp2 = Instant.FromUnixTimeTicks(16073942468626201);
            var sut2 = new StateMachineJob(id2, createdTimestamp2, "redbook", hangFireId2, string.Empty, string.Empty);
            sut2.SetEndTime(Instant.FromUnixTimeTicks(16073942468646201));
            sut2.SetState("Done");

            //// Act
            var result = new List<JobStatusResponse>
            {
                new JobStatusResponse(sut1),
                new JobStatusResponse(sut2),
            };

            //// Assert
            result.Should().HaveCount(2);
            result[0].Id.Should().Be(id1);
            result[1].Id.Should().Be(id2);
            result[0].HangfireJobId.Should().Be(hangFireId1);
            result[1].HangfireJobId.Should().Be(hangFireId2);
            result[0].StartDateTime.Should().Be(createdTimestamp1.ToString());
            result[1].StartDateTime.Should().Be(createdTimestamp2.ToString());
        }
    }
}
