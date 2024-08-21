// <copyright file="ContentSourceFile.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.File
{

    /// <summary>
    /// This class represents the information for a content source file being used by automations.
    /// </summary>
    public class ContentSourceFile
    {
        public ContentSourceFile(FileInfo file, string alias, bool include)
        {
            this.File = file;
            this.Alias = alias;
            this.Include = include;
        }

        public FileInfo File { get; }

        public string Alias { get; }

        public bool Include { get; }
    }
}
