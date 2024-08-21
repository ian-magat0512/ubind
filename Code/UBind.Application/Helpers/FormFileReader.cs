// <copyright file="FormFileReader.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Helpers
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Helper class for reading form file data content.
    /// </summary>
    public class FormFileReader
    {
        /// <summary>
        /// Opens and copies to a binary file to a temporary one, closes the file and then deletes the temporary file.
        /// </summary>
        /// <param name="file">An IFormFile object usually from the Form property of a HttpContext Request object.</param>
        /// <returns>A byte array containing the contents of the IFormFile object.</returns>
        public static async Task<byte[]> ReadAllFormFileBytes(IFormFile file)
        {
            byte[] data = null;
            if (file.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    data = ms.ToArray();
                }
            }

            return data;
        }
    }
}
