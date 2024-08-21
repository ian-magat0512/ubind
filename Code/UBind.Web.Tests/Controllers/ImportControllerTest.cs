// <copyright file="ImportControllerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.Controllers
{
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using UBind.Application.Services.Imports;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;
    using UBind.Web.Controllers.Portal;
    using Xunit;

    // CS1591 Missing XML comment for publicly visible type or member
    // Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

    public class ImportControllerTest
    {
        [Fact]
        public void CSVFileImport_Should_BeAbleToParseDataWithQuotedCommas()
        {
            // Arrange
            var header = @"recordnumber,amount,datetime,name,employmentdate,url,address,postcode,height(cms),weight(lbs),uniqueId,age";
            var content = @"2,1016,7/31/2008 14:22,Geoff Dalgas,6/5/2011 22:21,http://stackoverflow.com,""Corvallis, OR"",7679,351,81,b437f461b3fd27387c5d8ab47a293d35,34";
            var memoryStream = new MemoryStream();
            var sWriter = new StreamWriter(memoryStream);
            sWriter.WriteLine(header);
            sWriter.Flush();
            sWriter.WriteLine(content);
            sWriter.Flush();
            memoryStream.Position = 0;

            var controller = new ImportController(
                new Mock<IImportService>().Object,
                new Mock<ICachingResolver>().Object,
                new Mock<IOrganisationService>().Object,
                new Mock<ICqrsMediator>().Object);

            // Act
            var result = controller.ConvertCsvToListOfDictionary(new StreamReader(memoryStream));

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.First().TryGetValue("address", out string address);
            address.Should().Be("\"Corvallis, OR\"");
            result.First().Should().HaveCount(12);
        }
    }
}
