// <copyright file="DocumentMsWordFileFormat.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File
{
    using UBind.Application.FileHandling;

    /// <summary>
    /// Defines an instance of the save format to be used by the MS Word engine.
    /// </summary>
    public class DocumentMsWordFileFormat : IMsWordFileFormat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentMsWordFileFormat"/> class.
        /// </summary>
        /// <param name="fileExtension">The file extension to be used.</param>
        public DocumentMsWordFileFormat(string fileExtension)
        {
            this.FileExtension = fileExtension;
        }

        /// <inheritdoc/>
        public string FileExtension { get; }

        /// <inheritdoc/>
        public MsWordSaveFormatEnum SaveFormat
        {
            get
            {
                switch (this.FileExtension)
                {
                    case ".docx":
                        return MsWordSaveFormatEnum.DocumentDocx;
                    case ".dotx":
                        return MsWordSaveFormatEnum.DocumentDotx;
                    case ".dot":
                        return MsWordSaveFormatEnum.DocumentDot;
                    case ".docm":
                        return MsWordSaveFormatEnum.DocumentDocm;
                    case ".dotm":
                        return MsWordSaveFormatEnum.DocumentDotm;
                    default:
                        return MsWordSaveFormatEnum.Document;
                }
            }
        }
    }
}
