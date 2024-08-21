// <copyright file="TestFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.File
{
    using System.Threading.Tasks;
    using MorseCode.ITask;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;

    /// <summary>
    /// A provider which provides a file to be used in a unit test.
    /// This allows us to replace the provider used with one that grabs
    /// a local test file.
    /// </summary>
    public class TestFileProvider : IProvider<Data<FileInfo>>
    {
        private readonly string filePath;

        public TestFileProvider(string filePath)
        {
            this.filePath = filePath;
        }

        public string SchemaReferenceKey => "testFile";

        public ITask<IProviderResult<Data<FileInfo>>> Resolve(IProviderContext providerContext)
        {
            var content = System.IO.File.ReadAllBytes(this.filePath);
            var filename = System.IO.Path.GetFileName(this.filePath);
            return Task.FromResult(ProviderResult<Data<FileInfo>>.Success(
                (Data<FileInfo>)new FileInfo(filename, content))).AsITask();
        }
    }
}
