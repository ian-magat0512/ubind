// <copyright file="LocalFilesystemRepositoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Filesystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using UBind.Persistence.Filesystem;
    using Xunit;

    public class LocalFilesystemRepositoryTests : IDisposable
    {
        private LocalFilesystemFileRepository repository;
        private string repositoryPath;

        public LocalFilesystemRepositoryTests()
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().Location);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);
            this.repositoryPath = Path.Combine(dirPath, "LocalFilesystemRepositoryTests.Repository");
            Directory.CreateDirectory(this.repositoryPath);
            var mockConfiguration = new Mock<ILocalFilesystemStorageConfiguration>();
            mockConfiguration.Setup(x => x.BasePath).Returns(this.repositoryPath);
            this.repository = new LocalFilesystemFileRepository(mockConfiguration.Object);
        }

        public void Dispose()
        {
            Directory.Delete(this.repositoryPath, true);
        }

        [Fact]
        public async Task CopyItem_Makes_An_Exact_Copy_Of_The_File()
        {
            // Arrange
            var testFileContents = "asdf";
            var sourceFilename = "copySource.txt";
            var targetFilename = "copyTarget.txt";
            var sourceFilePath = Path.Combine(this.repositoryPath, sourceFilename);
            var targetFilePath = Path.Combine(this.repositoryPath, targetFilename);
            File.WriteAllText(sourceFilePath, testFileContents);

            // Act
            await this.repository.CopyItem(sourceFilePath, string.Empty, targetFilename, null);

            // Assert
            File.ReadAllText(targetFilePath).Should().Be(testFileContents);
        }

        [Fact]
        public async Task CreateFolder_Creates_A_Folder()
        {
            // Arrange
            var folderName = "TestFolder";

            // Act
            await this.repository.CreateFolder(string.Empty, folderName, null);

            // Assert
            Directory.Exists(Path.Combine(this.repositoryPath, folderName)).Should().BeTrue();
        }

        [Fact]
        public async Task DeleteFolder_Deletes_The_Folder()
        {
            // Arrange
            var folderName = "TestFolder";
            Directory.CreateDirectory(Path.Combine(this.repositoryPath, folderName));

            // Act
            await this.repository.DeleteFolder(folderName, null);

            // Assert
            Directory.Exists(Path.Combine(this.repositoryPath, folderName)).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteItem_Deletes_The_File()
        {
            // Arrange
            var testFileContents = "asdf";
            var testFilename = "file.txt";
            var testFilePath = Path.Combine(this.repositoryPath, testFilename);
            File.WriteAllText(testFilePath, testFileContents);

            // Act
            await this.repository.DeleteItem(testFilename, null);

            // Assert
            File.Exists(testFilePath).Should().BeFalse();
        }

        [Fact]
        public async Task FolderExists_Returns_True_When_Folder_Exists()
        {
            // Arrange
            var folderName = "TestFolder";
            Directory.CreateDirectory(Path.Combine(this.repositoryPath, folderName));

            // Act
            bool exists = await this.repository.FolderExists(folderName, null);

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task FolderExists_Returns_False_When_Folder_Does_Not_Exist()
        {
            // Arrange
            var folderName = "TestFolder";

            // Act
            bool exists = await this.repository.FolderExists(folderName, null);

            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task GetConfigurationFilesInFolder_Provides_a_list_of_ConfigurationFileDto()
        {
            // Arrange
            var testFileContents = "asdf";
            var testFilename1 = "rootFile.txt";
            var testFilePath1 = Path.Combine(this.repositoryPath, testFilename1);
            File.WriteAllText(testFilePath1, testFileContents);
            var folderName = "TestFolder";
            var folderPath = Path.Combine(this.repositoryPath, folderName);
            Directory.CreateDirectory(folderPath);
            var testFilename2 = "subFile.txt";
            var testFilePath2 = Path.Combine(folderPath, testFilename2);
            File.WriteAllText(testFilePath2, testFileContents);

            // Act
            var results = await this.repository.GetConfigurationFilesInFolder(folderName, null);

            // Assert
            results.Should().HaveCount(1);
            results.ToList().First().Id.Should().Be("TestFolder\\subFile.txt");
        }

        [Fact]
        public async Task GetFileContents_Gets_File_Contents()
        {
            // Arrange
            var testBytes = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
            var testFilename = "testFile.txt";
            var testFilePath = Path.Combine(this.repositoryPath, testFilename);
            File.WriteAllBytes(testFilePath, testBytes);

            // Act
            var contents = await this.repository.GetFileContents(testFilename, null);

            // Assert
            contents[6].Should().Be(0x20);
        }

        [Fact]
        public async Task GetFilesHashStringInFolders_Does_not_return_empty_string()
        {
            // Arrange
            var testFileContents = "asdf";
            var testFilename1 = "rootFile.txt";
            var testFilePath1 = Path.Combine(this.repositoryPath, testFilename1);
            File.WriteAllText(testFilePath1, testFileContents);
            var folderName = "TestFolder";
            var folderPath = Path.Combine(this.repositoryPath, folderName);
            Directory.CreateDirectory(folderPath);
            var testFilename2 = "subFile.txt";
            var testFilePath2 = Path.Combine(folderPath, testFilename2);
            File.WriteAllText(testFilePath2, testFileContents);
            var folderPaths = new List<string> { string.Empty, folderName };

            // Act
            string hashString = await this.repository.GetFilesHashStringInFolders(folderPaths, null);

            // Assert
            hashString.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetFileStringContents_works()
        {
            // Arrange
            var testFileContents = "asdf";
            var testFilename = "file.txt";
            var testFilePath = Path.Combine(this.repositoryPath, testFilename);
            File.WriteAllText(testFilePath, testFileContents);

            // Act
            var contents = await this.repository.GetFileStringContents(testFilename, null);

            // Assert
            contents.Should().Be(testFileContents);
        }

        [Fact]
        public async Task ListFilesInFolder_should_list_immediate_files_without_the_full_path()
        {
            // Arrange
            var testFileContents = "asdf";
            var testFilename1 = "rootFile.txt";
            var testFilePath1 = Path.Combine(this.repositoryPath, testFilename1);
            File.WriteAllText(testFilePath1, testFileContents);
            var folderName = "TestFolder";
            var folderPath = Path.Combine(this.repositoryPath, folderName);
            Directory.CreateDirectory(folderPath);
            var testFilename2 = "subFile.txt";
            var testFilePath2 = Path.Combine(folderPath, testFilename2);
            File.WriteAllText(testFilePath2, testFileContents);

            // Act
            var files = await this.repository.ListFilesInFolder(string.Empty, null);

            // Assert
            files.Should().HaveCount(1);
            files.First().Should().Be(testFilename1);
        }

        [Fact]
        public async Task ListItemsInFolder_should_list_immediate_files_and_folders_without_the_full_path()
        {
            // Arrange
            var testFileContents = "asdf";
            var testFilename1 = "rootFile.txt";
            var testFilePath1 = Path.Combine(this.repositoryPath, testFilename1);
            File.WriteAllText(testFilePath1, testFileContents);
            var folderName = "TestFolder";
            var folderPath = Path.Combine(this.repositoryPath, folderName);
            Directory.CreateDirectory(folderPath);
            var testFilename2 = "subFile.txt";
            var testFilePath2 = Path.Combine(folderPath, testFilename2);
            File.WriteAllText(testFilePath2, testFileContents);
            var folderPaths = new List<string> { string.Empty, folderName };

            // Act
            var items1 = await this.repository.ListItemsInFolder(string.Empty, null);
            var items2 = await this.repository.ListItemsInFolder(folderName, null);

            // Assert
            items1.Should().HaveCount(2);
            items1.First().Should().Be(testFilename1);
            items2.Should().HaveCount(1);
            items2.First().Should().Be(testFilename2);
        }

        [Fact]
        public async Task ListOfFilesLastModifiedDateTimeInFolder_should_give_a_date_string()
        {
            // Arrange
            var testFileContents = "asdf";
            var testFilename = "file.txt";
            var testFilePath = Path.Combine(this.repositoryPath, testFilename);
            File.WriteAllText(testFilePath, testFileContents);

            // Act
            var results = await this.repository.ListOfFilesLastModifiedDateTimeInFolder(string.Empty, null);

            // Assert
            // TODO: check for correct date format?
            results.First().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ListSubfoldersInFolder_should_list_immediate_folders_without_the_full_path()
        {
            // Arrange
            var folderName1 = "TestFolder1";
            var folderPath1 = Path.Combine(this.repositoryPath, folderName1);
            Directory.CreateDirectory(folderPath1);
            var folderName2 = "TestFolder2";
            var folderPath2 = Path.Combine(folderPath1, folderName2);
            Directory.CreateDirectory(folderPath2);

            // Act
            var folders = await this.repository.ListSubfoldersInFolder(folderName1, null);

            // Assert
            folders.Should().HaveCount(1);
            folders.First().Should().Be(folderName2);
        }

        [Fact]
        public async Task RenameItem_works()
        {
            // Arrange
            var testFileContents = "asdf";
            var sourceFilename = "source.txt";
            var targetFilename = "target.txt";
            var sourceFilePath = Path.Combine(this.repositoryPath, sourceFilename);
            var targetFilePath = Path.Combine(this.repositoryPath, targetFilename);
            File.WriteAllText(sourceFilePath, testFileContents);

            // Act
            await this.repository.RenameItem(sourceFilePath, targetFilename, null);

            // Assert
            File.Exists(targetFilePath).Should().BeTrue();
        }

        [Fact]
        public async Task TryGetFileStringContents_succeeds_when_the_file_exists()
        {
            // Arrange
            var testFileContents = "asdf";
            var testFilename = "file.txt";
            var testFilePath = Path.Combine(this.repositoryPath, testFilename);
            File.WriteAllText(testFilePath, testFileContents);

            // Act
            var contents = await this.repository.TryGetFileStringContents(testFilename, null);

            // Assert
            contents.HasValue.Should().BeTrue();
        }

        [Fact]
        public async Task TryGetFileStringContents_fails_when_the_file_doesnt_exist()
        {
            // Act
            var contents = await this.repository.TryGetFileStringContents("asdfasdfasdf", null);

            // Assert
            contents.HasNoValue.Should().BeTrue();
        }

        [Fact]
        public async Task WriteFileContents_should_succeed()
        {
            // Arrange
            var testBytes = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
            var testFilename = "testFile.txt";
            var testFilePath = Path.Combine(this.repositoryPath, testFilename);

            // Act
            await this.repository.WriteFileContents(testBytes, testFilename, null);

            // Assert
            var contents = File.ReadAllBytes(testFilePath);
            contents[6].Should().Be(0x20);
        }
    }
}
