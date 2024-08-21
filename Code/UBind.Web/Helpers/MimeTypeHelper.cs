// <copyright file="MimeTypeHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Helpers;

using Microsoft.AspNetCore.StaticFiles;

public static class MimeTypeHelper
{
    public static Dictionary<string, string> Mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { ".cshtml", "text/plain" },
    };

    public static string DetectMimeType(string filePath, string defaultMimeType = "application/octet-stream")
    {
        string mimeType = defaultMimeType;
        var extension = GetExtension(filePath);
        if (extension != null)
        {
            string? possibleMimeType;
            if (!Mappings.TryGetValue(extension, out possibleMimeType))
            {
                if (new FileExtensionContentTypeProvider().TryGetContentType(filePath, out possibleMimeType))
                {
                    mimeType = possibleMimeType;
                }
            }
            else
            {
                mimeType = possibleMimeType;
            }
        }

        return mimeType;
    }

    private static string? GetExtension(string path)
    {
        // Don't use Path.GetExtension as that may throw an exception if there are
        // invalid characters in the path. Invalid characters should be handled
        // by the FileProviders
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        int index = path.LastIndexOf('.');
        if (index < 0)
        {
            return null;
        }

        return path.Substring(index);
    }
}
