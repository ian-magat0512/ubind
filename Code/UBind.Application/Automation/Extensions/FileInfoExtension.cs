// <copyright file="FileInfoExtension.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Extensions
{
    using ServiceStack;
    using UBind.Application.Automation.Providers.File;

    /// <summary>
    /// A class that contains extension methods for the <see cref="FileInfo"/> class.
    /// </summary>
    public static class FileInfoExtension
    {
        /// <summary>
        /// Reads the content of the file info as a string
        /// </summary>
        /// <param name="file">The file info</param>
        /// <returns>The content of the file info</returns>
        public static string ReadContentToEnd(this FileInfo file)
        {
            string content = string.Empty;
            if (file == null || file.Content == null)
            {
                return content;
            }

            using (var stream = new MemoryStream(file.Content))
            {
                content = stream.ReadToEnd();
            }

            return content;
        }
    }
}
