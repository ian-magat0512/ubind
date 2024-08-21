// <copyright file="FileServiceTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Services.Filesystem
{
    using System.Collections.Generic;
    using System.IO.Abstractions.TestingHelpers;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using UBind.Domain.Repositories.FileSystem;
    using UBind.Persistence.Services.Filesystem;
    using Xunit;

    public class FileServiceTest
    {
        private readonly ServiceCollection serviceCollection;

        public FileServiceTest()
        {
            var services = new ServiceCollection();

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\foo.zip", new MockFileData(new byte[] { 0x12, 0x34, 0x56, 0xd2 }) },
            });

            services.AddSingleton<IFileSystemService, FileService>();
            services.AddSingleton(fileSystem.FileSystem);
            services.AddSingleton(new Mock<IFileSystemFileCompressionService>().Object);
            this.serviceCollection = services;
        }

        [Fact]
        public void GetFileMd5Hash_ReturnsCorrectHashValue_WhenFileExists()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IFileSystemService>();

            // Act
            var result = sut.GetFileMd5Hash(@"c:\foo.zip");

            // Assert
            result.Should().Be("EC363F2D366F30EFE0480C033908F43F");
        }
    }
}
