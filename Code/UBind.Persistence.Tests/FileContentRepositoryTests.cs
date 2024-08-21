// <copyright file="FileContentRepositoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Linq;
    using System.Text;
    using FluentAssertions;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="FileContentRepository"/>.
    /// </summary>
    [Collection(DatabaseCollection.Name)]
    public class FileContentRepositoryTests
    {
        private readonly IUBindDbContext context = new UBindDbContext(DatabaseFixture.TestConnectionString);

        [Fact]
        public void Insert_ExistingFileContent_ShouldReturnReferenceToExistingFileContent()
        {
            var repo = new FileContentRepository(this.context);

            // Arrange
            var tenantId = Guid.NewGuid();
            var testContent = "Hello World!";
            var testFile = FileContent.CreateFromBytes(tenantId, Guid.NewGuid(), Encoding.ASCII.GetBytes(testContent));
            var testFileId = repo.Insert(testFile);
            repo.Insert(
                FileContent.CreateFromBytes(tenantId, Guid.NewGuid(), Encoding.ASCII.GetBytes("Another file content.")));
            repo.Insert(FileContent.CreateFromBytes(tenantId, Guid.NewGuid(), Encoding.ASCII.GetBytes("And another one.")));
            this.context.SaveChanges();

            // Act
            var sameFileAttached = FileContent.CreateFromBytes(tenantId, Guid.NewGuid(), Encoding.ASCII.GetBytes(testContent));
            var newFileId = repo.Insert(sameFileAttached);
            this.context.SaveChanges();

            // Assert
            newFileId.Should().Be(testFileId);
        }

        [Fact]
        public void Insert_UniqueFiles_ShouldInsertAndReturnFileId()
        {
            var repo = new FileContentRepository(this.context);
            var rowCount = this.context.FileContents.Count();

            // Arrange
            var firstFile = FileContent.CreateFromBytes(
                Guid.NewGuid(), Guid.NewGuid(), Encoding.ASCII.GetBytes(Guid.NewGuid().ToString()));
            var firstId = repo.Insert(firstFile);
            this.context.SaveChanges();

            // Act
            var secondFile = FileContent.CreateFromBytes(
                Guid.NewGuid(), Guid.NewGuid(), Encoding.ASCII.GetBytes(Guid.NewGuid().ToString()));
            var secondId = repo.Insert(secondFile);
            this.context.SaveChanges();

            // Assert
            firstId.Should().NotBe(secondId);
            this.context.FileContents.Count().Should().Be(rowCount + 2);
        }
    }
}
