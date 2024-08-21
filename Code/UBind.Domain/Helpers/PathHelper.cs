// <copyright file="PathHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Extension methods for path.
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Regex pattern used for validating JSON pointers.
        /// </summary>
        /// <remarks>Key segment start with lowercase letter or a number (for arrays), does not contain any whitespace
        /// and special characters.</remarks>
        private const string PropertyPathPattern = "^([a-z]|[0-9])+[a-zA-Z0-9]*$";

        /// <summary>
        /// Append a backslash to a path.
        /// </summary>
        /// <param name="path">The path to append the backslash to.</param>
        /// <returns>A new path with backslash appended.</returns>
        public static string PathAddBackslash(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            path = path.TrimEnd();

            if (PathEndsWithDirectorySeparator())
            {
                return path;
            }

            return path + GetDirectorySeparatorUsedInPath();

            bool PathEndsWithDirectorySeparator()
            {
                if (path.Length == 0)
                {
                    return false;
                }

                char lastChar = path[path.Length - 1];
                return lastChar == Path.DirectorySeparatorChar
                    || lastChar == Path.AltDirectorySeparatorChar;
            }

            char GetDirectorySeparatorUsedInPath()
            {
                if (path.Contains(Path.AltDirectorySeparatorChar))
                {
                    return Path.AltDirectorySeparatorChar;
                }

                return Path.DirectorySeparatorChar;
            }
        }

        /// <summary>
        /// Append a colon to a path.
        /// </summary>
        /// <param name="path">The path to append the colon to.</param>
        /// <returns>A new path with colon appended.</returns>
        public static string AppendColon(string path)
        {
            return path + ":";
        }

        /// <summary>
        /// Method for checking if the path is a json pointer or not.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>true if the path is a json pointer, otherwise false.</returns>
        public static bool IsJsonPointer(string? path)
        {
            var relativeRegex = new Regex("^\\d +#?$");
            if (path != null
                && (path.Contains('/') || relativeRegex.IsMatch(path)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method for converting a json pointer to json path.
        /// Note: relative json pointer syntax is not supported.
        /// </summary>
        /// <param name="path">The json pointer to convert to json path.</param>
        public static string ToJsonPath(string path)
        {
            path = path ?? string.Empty;

            // check if relative pointer.
            if (path.IsRelativeJsonPointer())
            {
                throw new ArgumentException($"Conversion to json path does not support relative json pointer {path}. Please use simple json pointer synthax.");
            }

            if (!IsJsonPointer(path))
            {
                return path;
            }

            // escape characters.
            path = path.Replace("~1", "/").Replace("~0", "~");

            string newPath = string.Empty;
            foreach (var item in path.Split('/'))
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                newPath += int.TryParse(item, out int i)
                        ? $"[{item}]"
                        : $".{item}";
            }

            return newPath.Trim(' ', '.');
        }

        /// <summary>
        /// Converts a JSON path to JSON Pointer syntax.
        /// </summary>
        /// <param name="jsonPath">The JSON path.</param>
        /// <returns>The equivalent JSON pointer.</returns>
        public static string ToJsonPointer(string jsonPath)
        {
            string jsonPointer = jsonPath.TrimStart('$')
                .Replace('.', '/').Replace('[', '/')
                .Replace("]", string.Empty);
            if (!jsonPointer.StartsWith("/"))
            {
                jsonPointer = $"/{jsonPointer}";
            }

            return jsonPointer;
        }

        /// <summary>
        /// Verifies if the JSON pointer provided is valid.
        /// </summary>
        /// <param name="path">The string pointer to be validated.</param>
        /// <returns>A flag specifying if the path is valid.</returns>
        /// <remarks>
        /// Valid JSON path should pass the following criteria:
        /// - Path should start with '/' or '#/'.
        /// - Path segments should start with lowercase letter or a number (for arrays),
        /// does not contain any whitespace or special characters.
        /// - Path should not end with '/'.
        /// </remarks>
        public static bool IsValidJsonPointer(string path)
        {
            if (!IsJsonPointer(path))
            {
                return false;
            }

            if (path.Last<char>().Equals('/'))
            {
                return false;
            }

            var pathSegments = path.Replace("#/", "/").Split('/').Skip(1);
            foreach (var item in pathSegments)
            {
                var match = Regex.Match(item, PropertyPathPattern, RegexOptions.None);
                if (!match.Success)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a list of child paths from the paths supplied that have the same base as the parent path.
        /// </summary>
        /// <param name="parentPath">The base parent path. This is expected to be the same as the base path of the entries
        /// in {paths}.</param>
        /// <param name="paths">A list of paths to be parsed. These paths should have the same base path which is equal to
        /// the parameter {parentPath}.</param>
        /// <param name="delimiter">An optional parameter to state the delimiter used in the paths. The default value is a slash '/'.</param>
        /// <returns>A list of child paths matched and obtained from the paths provided.</returns>
        /// <remarks>
        /// As an example scenario:
        /// parentPath = "quote"
        /// paths = ['/quote/policy', '/claim/policy/quote', '/quote/policyTransaction']
        /// returns: ['/policy', '/policyTransaction']
        /// Explanation: Since only the first and third items in the paths provided
        /// have the same base as the {parentPath}, they are the only ones processed (stripped off of the base) and returned.
        /// </remarks>
        public static IEnumerable<string> GetChildPaths(string parentPath, IEnumerable<string> paths, string delimiter = "/")
        {
            if (delimiter is null)
            {
                throw new ArgumentNullException(nameof(delimiter));
            }

            var relatedEntitiesPaths = new List<string>();
            foreach (var path in paths ?? Enumerable.Empty<string>())
            {
                var similarPathSegments
                    = path.Split(new string[] { delimiter }, options: StringSplitOptions.RemoveEmptyEntries);
                if (similarPathSegments.First().Equals(parentPath))
                {
                    relatedEntitiesPaths.Add($"{delimiter}{string.Join(delimiter, similarPathSegments.Skip(1))}");
                }
            }

            return relatedEntitiesPaths;
        }

        public static IEnumerable<string> GetRelatedEntities(string parentPath, IEnumerable<string>? paths, string delimiter = "/")
        {
            if (delimiter is null)
            {
                throw new ArgumentNullException(nameof(delimiter));
            }

            var relatedEntities = new List<string>();
            foreach (var path in paths ?? Enumerable.Empty<string>())
            {
                var similarPathSegments
                    = path.Split(new string[] { delimiter }, options: StringSplitOptions.RemoveEmptyEntries);
                if (similarPathSegments.First().Equals(parentPath))
                {
                    relatedEntities.Add($"{string.Join(delimiter, similarPathSegments.Skip(1))}");
                }
            }

            return relatedEntities;
        }

        public static string NormaliseFolderPath(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return string.Empty;
            }

            folderPath = folderPath.Replace("\\", "/");
            folderPath = RemoveTrailingDelimiter(folderPath, "/");
            return folderPath;
        }

        public static string GetParentFolderPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            path = path.Replace("\\", "/");
            path = RemoveTrailingDelimiter(path);

            if (!path.Contains('/'))
            {
                return string.Empty;
            }

            path = path.Substring(0, path.LastIndexOf("/"));
            return path + "/";
        }

        public static string GetLastSegment(string path, string delimiter = "/")
        {
            var segments = path.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
            {
                return string.Empty;
            }

            return segments[segments.Length - 1];
        }

        public static string Combine(string path1, string path2, string delimiter = "/")
        {
            return RemoveTrailingDelimiter(path1) + delimiter + RemoveLeadingDelimiter(path2);
        }

        public static string RemoveTrailingDelimiter(string path, string delimiter = "/")
        {
            return path.EndsWith(delimiter)
                ? path.Substring(0, path.Length - delimiter.Length)
                : path;
        }

        public static string RemoveLeadingDelimiter(string path, string delimiter = "/")
        {
            return path.StartsWith(delimiter)
                ? path.Substring(delimiter.Length)
                : path;
        }
    }
}
