// <copyright file="ArchiveFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.File.Model
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Represents an archive, such as a zip file.
    /// </summary>
    public abstract class ArchiveFactory
    {
        public static async Task<IArchive> Open(
            FileInfo sourceFile,
            string? password,
            string? format,
            IClock clock,
            Func<Task<JObject>> getErrorDataCallback)
        {
            if (format == null)
            {
                if (sourceFile.FileName.ToString().EndsWith(".zip"))
                {
                    format = "zip";
                }
                else
                {
                    var errorData = await getErrorDataCallback();
                    throw new ErrorException(Errors.Automation.Archive.FormatNotSpecified(errorData));
                }
            }
            else if (format != "zip")
            {
                var errorData = await getErrorDataCallback();
                throw new ErrorException(Errors.Automation.Archive.UnsupportedFormatSpecified(format, errorData));
            }

            if (!string.IsNullOrEmpty(password))
            {
                return await SharpZipLibZipArchive.Open(sourceFile, password, clock, getErrorDataCallback);
            }

            return await SystemZipArchive.Open(sourceFile, clock, getErrorDataCallback);
        }

        public static async Task<IArchive> Create(
            string format,
            string? password,
            IClock clock,
            Func<Task<JObject>> getErrorDataCallback)
        {
            if (string.IsNullOrEmpty(format))
            {
                var errorData = await getErrorDataCallback();
                throw new ErrorException(Errors.Automation.Archive.FormatNotSpecified(errorData));
            }

            if (format != "zip")
            {
                var errorData = await getErrorDataCallback();
                throw new ErrorException(Errors.Automation.Archive.UnsupportedFormatSpecified(format, errorData));
            }

            if (!string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("Due to limitations with the .NET libraries, we unfortunately do "
                    + "not support creating or modifiying zip files that use passwords.");
            }

            return SystemZipArchive.Create(clock);
        }
    }
}
