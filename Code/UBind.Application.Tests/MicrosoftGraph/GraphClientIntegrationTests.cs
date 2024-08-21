// <copyright file="GraphClientIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.MicrosoftGraph
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging.Abstractions;
    using NodaTime;
    using UBind.Application.MicrosoftGraph;
    using UBind.Application.MicrosoftGraph.Exceptions;
    using UBind.Domain.Tests;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="GraphClientIntegrationTests" />.
    /// </summary>
    public class GraphClientIntegrationTests : IAsyncLifetime
    {
        /// <summary>
        /// Defines the authenticationToken.
        /// </summary>
        private string authenticationToken;

        /// <summary>
        /// Defines the sut.
        /// </summary>
        private GraphClient sut;

        /// <summary>
        /// Defines the testRunId.
        /// </summary>
        private Guid testRunId;

        /// <summary>
        /// Defines the testRunRootFolder.
        /// </summary>
        private string testRunRootFolder;

        /// <summary>
        /// Defines the knownFilePath1.
        /// </summary>
        private string knownFilePath1;

        /// <summary>
        /// Defines the knownFilePath2.
        /// </summary>
        private string knownFilePath2;

        /// <summary>
        /// Defines the fileContent1.
        /// </summary>
        private string fileContent1 = "Test file 1";

        /// <summary>
        /// Defines the fileContent2.
        /// </summary>
        private string fileContent2 = "Test file 2";

        /// <summary>
        /// The FixtureSetup.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task InitializeAsync()
        {
            // Get auth token.
            var urlProvider = new GraphUrlProvider();
            var configuration = new TestMicrosoftGraphConfiguration();
            var authenticator = new CachingAuthenticationTokenProvider(
                new AuthenticationTokenProvider(urlProvider, configuration, SystemClock.Instance));
            var tokenWithExpiry = await authenticator.GetAuthenticationTokenAsync();
            this.authenticationToken = tokenWithExpiry.BearerToken;

            // Create subject under test.
            this.sut = new GraphClient(urlProvider, configuration, authenticator, NullLogger<GraphClient>.Instance);

            // Setup test data.
            // Create the folders and files
            this.testRunId = Guid.NewGuid();
            var testRunFolderName = $"Test Run {this.testRunId}";
            await this.sut.CreateFolder(null, configuration.UBindFolderName, this.authenticationToken);
            await this.sut.CreateFolder(configuration.UBindFolderName, testRunFolderName, this.authenticationToken);
            this.testRunRootFolder = Path.Combine(configuration.UBindFolderName, testRunFolderName);
            var file1ContentBytes = Encoding.UTF8.GetBytes(this.fileContent1);
            this.knownFilePath1 = Path.Combine(this.testRunRootFolder, "TestFile1.txt");
            await this.sut.WriteFileContents(file1ContentBytes, this.knownFilePath1, this.authenticationToken);
            var file2ContentBytes = Encoding.UTF8.GetBytes(this.fileContent2);
            this.knownFilePath2 = Path.Combine(this.testRunRootFolder, "TestFile2.txt");
            await this.sut.WriteFileContents(file2ContentBytes, this.knownFilePath2, this.authenticationToken);
        }

        /// <summary>
        /// The FixtureTearDown.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task DisposeAsync()
        {
            await this.sut.DeleteFolder(this.testRunRootFolder, this.authenticationToken);
        }

        // Tests for Task<string> DownloadFileContentAsync(string path, string authenticationToken, int timeoutSeconds = 3);

        /// <summary>
        /// The DownloadFileContentAsync_ReturnsFileContent_WhenFileExists.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task DownloadFileContentAsync_ReturnsFileContent_WhenFileExists()
        {
            // Act
            var content = await this.sut.GetFileStringContents(this.knownFilePath1, this.authenticationToken);

            // Assert
            Assert.Equal(this.fileContent1, content);
        }

        /// <summary>
        /// The DownloadFileContentAsync_ThrowsExcpetion_WhenFileDoesNotExist.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task DownloadFileContentAsync_ThrowsExcpetion_WhenFileDoesNotExist()
        {
            // Act + Act
            await Assert.ThrowsAsync<GraphRequestNotFoundException>(
                () => this.sut.GetFileStringContents("bogusPath", this.authenticationToken));
        }

        /// <summary>
        /// The DownloadFileContentAsync_ThrowsExcpetion_WhenRequestTimesout.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task DownloadFileContentAsync_ThrowsExcpetion_WhenRequestTimesout()
        {
            // Act + Act
            await Assert.ThrowsAsync<GraphRequestTimeoutException>(
                () => this.sut.GetFileStringContents("bogusPath", this.authenticationToken, TimeSpan.FromMilliseconds(1)));
        }

        // Tests for Task<Maybe<string>> TryDownloadFileContentAsync(string path, string authenticationToken, int timeoutSeconds = 3);

        /// <summary>
        /// The TryDownloadFileContentAsync_ReturnsMaybeMonadWithFileContent_WhenFileExists.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task TryDownloadFileContentAsync_ReturnsMaybeMonadWithFileContent_WhenFileExists()
        {
            // Act
            var content = await this.sut.TryGetFileStringContents(this.knownFilePath1, this.authenticationToken);

            // Assert
            Assert.True(content.HasValue);
            Assert.Equal(this.fileContent1, content.Value);
        }

        /// <summary>
        /// The TryDownloadFileContent_ReturnsMaybeMonadNone_WhenFileDoesNotExist.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task TryDownloadFileContent_ReturnsMaybeMonadNone_WhenFileDoesNotExist()
        {
            // Act
            var content = await this.sut.TryGetFileStringContents("bogusPath", this.authenticationToken);

            // Assert
            Assert.True(content.HasNoValue);
        }

        /// <summary>
        /// The TryDownloadFileContent_ThrowsException_WhenRequestTimesout.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task TryDownloadFileContent_ThrowsException_WhenRequestTimesout()
        {
            // Act + Assert
            await Assert.ThrowsAsync<GraphRequestTimeoutException>(
                () => this.sut.TryGetFileStringContents("bogusPath", this.authenticationToken, TimeSpan.FromMilliseconds(1)));
        }

        // Tests for Task<byte[]> DownloadFileDataAsync(string filePath, string authenticationToken, int timeoutSeconds = 3);

        /// <summary>
        /// The DownloadFileDataAsync_ReturnsContentAsByteArray_WhenFileExists.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task DownloadFileDataAsync_ReturnsContentAsByteArray_WhenFileExists()
        {
            // Act
            var bytes = await this.sut.GetFileContents(this.knownFilePath1, this.authenticationToken);

            // Assert
            Assert.NotNull(bytes);
            Assert.Equal(this.fileContent1, Encoding.UTF8.GetString(bytes));
        }

        /// <summary>
        /// The DownloadFileDataAsync_ThrowsNotFoundException_WhenFileDoesNotExist.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task DownloadFileDataAsync_ThrowsNotFoundException_WhenFileDoesNotExist()
        {
            // Act + Assert
            await Assert.ThrowsAsync<GraphRequestNotFoundException>(
                () => this.sut.GetFileContents("bogusPath", this.authenticationToken));
        }

        /// <summary>
        /// The DownloadFileDataAsync_ThrowsTimeoutException_WhenRequestTimesOut.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task DownloadFileDataAsync_ThrowsTimeoutException_WhenRequestTimesOut()
        {
            // Act + Assert
            await Assert.ThrowsAsync<GraphRequestTimeoutException>(
                () => this.sut.GetFileContents("bogusPath", this.authenticationToken, TimeSpan.FromMilliseconds(1)));
        }

        // Tests for Task RenameItem(string itemPath, string newName, string authenticationToken, int timeoutSeconds = 3);

        /// <summary>
        /// The RenameItem_Succeeds_WhenItemIsExistingFile.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task RenameItem_Succeeds_WhenItemIsExistingFile()
        {
            // Arange
            var fileContent = "File to rename";
            var filePath = Path.Combine(this.testRunRootFolder, $"FileToRename{Guid.NewGuid()}.txt");
            await this.sut.WriteFileContents(Encoding.UTF8.GetBytes(fileContent), filePath, this.authenticationToken);
            var newName = $"RenamedFile{Guid.NewGuid()}.txt";

            // Act
            await this.sut.RenameItem(filePath, newName, this.authenticationToken);

            // Assert
            var renamedFileContent = await this.sut.GetFileStringContents(Path.Combine(this.testRunRootFolder, newName), this.authenticationToken);
            Assert.Equal(renamedFileContent, fileContent);
        }

        /// <summary>
        /// The RenameItem_ThrowsNotFoundException_WhenItemDoesNotExist.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task RenameItem_ThrowsNotFoundException_WhenItemDoesNotExist()
        {
            // Arange
            var filePath = Path.Combine(this.testRunRootFolder, $"FileToRename{Guid.NewGuid()}.txt");
            var newName = $"RenamedFile{Guid.NewGuid()}.txt";

            // Act
            await Assert.ThrowsAsync<GraphRequestNotFoundException>(
                () => this.sut.RenameItem(filePath, newName, this.authenticationToken));
        }

        /// <summary>
        /// The RenameItem_ThrowsErrorExcpetion_WhenNewNameIsIllegal.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task RenameItem_ThrowsErrorExcpetion_WhenNewNameIsIllegal()
        {
            // Arange
            var fileContent = "File to rename";
            var filePath = Path.Combine(this.testRunRootFolder, $"FileToRename{Guid.NewGuid()}.txt");
            await this.sut.WriteFileContents(Encoding.UTF8.GetBytes(fileContent), filePath, this.authenticationToken);
            var newName = "////";

            // Act
            await Assert.ThrowsAsync<GraphRequestErrorException>(
                () => this.sut.RenameItem(filePath, newName, this.authenticationToken));
        }

        /// <summary>
        /// The RenameItem_ThrowsTimeout_WhenNewNameIsIllegal.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task RenameItem_ThrowsTimeout_WhenNewNameIsIllegal()
        {
            // Arange
            var fileContent = "File to rename";
            var filePath = Path.Combine(this.testRunRootFolder, $"FileToRename{Guid.NewGuid()}.txt");
            await this.sut.WriteFileContents(Encoding.UTF8.GetBytes(fileContent), filePath, this.authenticationToken);
            var newName = $"RenamedFile{Guid.NewGuid()}.txt";

            // Act
            await Assert.ThrowsAsync<GraphRequestTimeoutException>(
                () => this.sut.RenameItem(filePath, newName, this.authenticationToken, TimeSpan.FromMilliseconds(1)));
        }

        // Tests for Task CreateFolder(string parentPath, string folderName, string authenticationToken, int timeoutSeconds = 3);

        // Tests for Task<IEnumerable<string>> ListItemsInFolder(string folderPath, string authenticationToken);

        // Tests for Task DeleteFolder(string folderPath, string authenticationToken, int timeoutSeconds = 3);

        // Tests for Task<IEnumerable<string>> ListFilesInFolder(string folderPath, string authenticationToken);

        /// <summary>
        /// The ListFilesInFolder_ListsAllFilesInFolder_WhenFolderContainsFiles.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task ListFilesInFolder_ListsAllFilesInFolder_WhenFolderContainsFiles()
        {
            // Act
            var files = await this.sut.ListFilesInFolder(this.testRunRootFolder, this.authenticationToken);

            // Assert
            Assert.True(files.Count() >= 2);
            Assert.Contains("TestFile1.txt", files);
            Assert.Contains("TestFile2.txt", files);
        }

        // Tests for Task<IEnumerable<string>> ListSubfoldersInFolder(string folderPath, string authenticationToken);

        // Tests for Task UploadFile(byte[] fileContent, string destinationPath, string authenticationToken, int timeoutSeconds = 3);

        /// <summary>
        /// The UploadFile_OverwriteExistingFiles.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task UploadFile_OverwriteExistingFiles()
        {
            // Arrange
            var newFilePath = Path.Combine(this.testRunRootFolder, "for-overwrite-test.txt");
            var newFileContentString = $"To be overwritten.";
            var newFileContent = Encoding.UTF8.GetBytes(newFileContentString);
            var overwrittenFileContentString = $"Has been overwritten.";
            var overwrittenFileContent = Encoding.UTF8.GetBytes(overwrittenFileContentString);
            await this.sut.WriteFileContents(newFileContent, newFilePath, this.authenticationToken);

            // Act
            await this.sut.WriteFileContents(overwrittenFileContent, newFilePath, this.authenticationToken);

            // Assert
            var result = await this.sut.TryGetFileStringContents(newFilePath, this.authenticationToken);
            Assert.True(result.HasValue);
            Assert.Equal(overwrittenFileContentString, result.Value);
        }

        // Tests for Task<IEnumerable<string>> ListOfFilesLastModifiedDateTimeInFolder(string folderPath, string authenticationToken);

        // Tests for Task<string> GetFilesHashStringInFolders(List<string> folderPaths, string authenticationToken);

        // Tests for Task UploadFile(string localPath, string destinationPath, string authenticationToken, int timeoutSeconds = 3);

        /// <summary>
        /// Test for copying files from one folder to another folder.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task CopyItem_CopyFileToAnotherFolder()
        {
            // Arrange
            var destinationFolder = "DestinationFolder";
            var destinationFolderPath = Path.Combine(this.testRunRootFolder, destinationFolder);
            await this.sut.CreateFolder(this.testRunRootFolder, destinationFolder, this.authenticationToken);
            var destinationFileName = "Copy New File.txt";

            var sourceFolder = "SourceFolder";
            var sourceFolderPath = Path.Combine(this.testRunRootFolder, sourceFolder);
            await this.sut.CreateFolder(this.testRunRootFolder, sourceFolder, this.authenticationToken);
            var sourceFileName = "New File.txt";
            var sourceFilePath = Path.Combine(sourceFolderPath, sourceFileName);
            var childFileContents = "Child file.";
            await this.sut.WriteFileContents(Encoding.UTF8.GetBytes(childFileContents), sourceFilePath, this.authenticationToken);

            // Act
            await this.sut.CopyItem(
                sourceFilePath,
                destinationFolderPath,
                destinationFileName,
                this.authenticationToken);

            // Assert
            var files = await this.sut.ListFilesInFolder(destinationFolderPath, this.authenticationToken);
            Assert.True(files.Count() == 1);
            Assert.Contains(destinationFileName, files);
        }

        /// <summary>
        /// Test for copying files from one folder to another folder, destination folder does not exists.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task CopyItem_Should_Return_Exception_When_DestinationFolder_DoesNotExists()
        {
            // Arrange
            var destinationFolder = "DestinationFolderNotExists";
            var destinationFolderPath = Path.Combine(this.testRunRootFolder, destinationFolder);

            // do not create the destination folder to simulate destination folder does not exists.
            var destinationFileName = "Copy New File.txt";

            var sourceFolder = "SourceFolder2";
            var sourceFolderPath = Path.Combine(this.testRunRootFolder, sourceFolder);
            await this.sut.CreateFolder(this.testRunRootFolder, sourceFolder, this.authenticationToken);
            var sourceFileName = "New File.txt";
            var sourceFilePath = Path.Combine(sourceFolderPath, sourceFileName);
            var childFileContents = "Child file.";
            await this.sut.WriteFileContents(Encoding.UTF8.GetBytes(childFileContents), sourceFilePath, this.authenticationToken);

            // Assert
            await Assert.ThrowsAsync<GraphRequestNotFoundException>(() => this.sut.CopyItem(
                    sourceFilePath,
                    destinationFolderPath,
                    destinationFileName,
                    this.authenticationToken));
        }

        /// <summary>
        /// Test for copying files from one folder to another folder, source file does not exists.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task CopyItem_Should_Return_Exception_When_SourceFile_DoesNotExists()
        {
            // Arrange
            var destinationFolder = "DestinationFolder3";
            var destinationFolderPath = Path.Combine(this.testRunRootFolder, destinationFolder);
            await this.sut.CreateFolder(this.testRunRootFolder, destinationFolder, this.authenticationToken);
            var destinationFileName = "Copy New File.txt";

            var sourceFolder = "SourceFolderFileNotExists";
            var sourceFolderPath = Path.Combine(this.testRunRootFolder, sourceFolder);
            await this.sut.CreateFolder(this.testRunRootFolder, sourceFolder, this.authenticationToken);
            var sourceFileName = "New File.txt";
            var sourceFilePath = Path.Combine(sourceFolderPath, sourceFileName);

            // Assert
            await Assert.ThrowsAsync<GraphRequestNotFoundException>(() => this.sut.CopyItem(
                    sourceFilePath,
                    destinationFolderPath,
                    destinationFileName,
                    this.authenticationToken));
        }

        /// <summary>
        /// The CopyItem_CopiesFolderContents.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This test is slow, so we're going to skip it most of the time. Renable this if you change the code related to this.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task CopyItem_CopiesFolderContents()
        {
            // Arrange
            var subFolderName = "TestSubFolder";
            var subFolderPath = Path.Combine(this.testRunRootFolder, subFolderName);
            await this.sut.CreateFolder(this.testRunRootFolder, subFolderName, this.authenticationToken);
            var childFileName = "New File.txt";
            var childFilePath = Path.Combine(subFolderPath, childFileName);
            var childFileContents = "Child file.";
            await this.sut.WriteFileContents(Encoding.UTF8.GetBytes(childFileContents), childFilePath, this.authenticationToken);
            var copiedFolderName = "CopiedSubFolder";

            // Act
            await this.sut.CopyItem(
                subFolderPath,
                this.testRunRootFolder,
                copiedFolderName,
                this.authenticationToken);

            // Assert
            var copiedFolderPath = Path.Combine(this.testRunRootFolder, copiedFolderName);
            var files = await this.sut.ListFilesInFolder(copiedFolderPath, this.authenticationToken);
            Assert.True(files.Count() == 1);
            Assert.Contains("New File.txt", files);
        }
    }
}
