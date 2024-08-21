// <copyright file="Path.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ValueTypes
{
    using System;
    using System.Collections.Generic;

    public class Path : ValueObject
    {
        private readonly string path;
        private Options options;

        public Path(
            string path,
            string delimiter = "/",
            Options options = Options.ReturnParentsWithTrailingDelimiter
                | Options.RemoveEmptySegments
                | Options.NormaliseFileSystemPath)
        {
            this.Delimiter = delimiter;
            this.options = options;
            if (path == null)
            {
                this.path = string.Empty;
            }
            else
            {
                this.path = options.HasFlag(Options.NormaliseFileSystemPath)
                    ? path.Replace("\\", "/")
                    : path;
            }
        }

        [Flags]
        public enum Options
        {
            None = 0, // 000000
            ReturnParentsWithTrailingDelimiter = 1, // 000001
            RemoveEmptySegments = 2, // 000010
            NormaliseFileSystemPath = 4, // 000100
        }

        public string Delimiter { get; }

        public string[] Segments => this.options.HasFlag(Options.RemoveEmptySegments)
            ? this.path.Split(new string[] { this.Delimiter }, StringSplitOptions.RemoveEmptyEntries)
            : this.path.Split(new string[] { this.Delimiter }, StringSplitOptions.None);

        public static implicit operator string(Path path) => path.ToString();

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

        public static string AddTrailingDelimiter(string path, string delimiter = "/")
        {
            return path.EndsWith(delimiter)
                ? path
                : path + delimiter;
        }

        public static string AddLeadingDelimiter(string path, string delimiter = "/")
        {
            return path.StartsWith(delimiter)
                ? path
                : delimiter + path;
        }

        public bool IsEmpty() => string.IsNullOrEmpty(this.path);

        public Path GetParent()
        {
            if (string.IsNullOrEmpty(this.path))
            {
                return new Path(string.Empty, this.Delimiter, this.options);
            }

            string result = RemoveTrailingDelimiter(this.path);
            if (!result.Contains(this.Delimiter))
            {
                return new Path(string.Empty, this.Delimiter, this.options);
            }

            result = result.Substring(0, result.LastIndexOf(this.Delimiter));
            result = this.options.HasFlag(Options.ReturnParentsWithTrailingDelimiter)
                ? result + this.Delimiter
                : result;
            return new Path(result, this.Delimiter, this.options);
        }

        public Path RemoveTrailingDelimiter()
        {
            string result = RemoveTrailingDelimiter(this.path);
            return new Path(result, this.Delimiter, this.options);
        }

        public Path RemoveLeadingDelimiter()
        {
            string result = RemoveLeadingDelimiter(this.path);
            return new Path(result, this.Delimiter, this.options);
        }

        public string GetLastSegment()
        {
            var segments = this.Segments;
            if (segments.Length == 0)
            {
                return string.Empty;
            }

            return segments[segments.Length - 1];
        }

        public Path Join(params Path[] paths)
        {
            string[] pathStrings = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].Delimiter != this.Delimiter)
                {
                    throw new ArgumentException("An attempt was made to combine paths with different delimiters. "
                        + "When combining paths, all paths must use the same delimiter.");
                }

                pathStrings[i] = paths[i].ToString();
            }

            return this.Join(pathStrings);
        }

        public Path Join(params string[] paths)
        {
            string result = this.path;
            for (int i = 0; i < paths.Length; i++)
            {
                string nextPath = this.options.HasFlag(Options.NormaliseFileSystemPath)
                    ? paths[i].Replace("\\", "/")
                    : paths[i];

                if (string.IsNullOrEmpty(result) && string.IsNullOrEmpty(nextPath))
                {
                    continue;
                }
                else if (string.IsNullOrEmpty(result))
                {
                    result = nextPath;
                }
                else if (string.IsNullOrEmpty(nextPath))
                {
                    continue;
                }
                else
                {
                    result = RemoveTrailingDelimiter(result) + this.Delimiter + RemoveLeadingDelimiter(nextPath);
                }
            }

            return new Path(result, this.Delimiter, this.options);
        }

        public override string ToString()
        {
            return this.path;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.path;
        }
    }
}
