// <copyright file="FileName.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ValueTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// For representing file name.
    /// </summary>
    public class FileName : ValueObject
    {
        private readonly string fileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileName"/> class.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        public FileName(string fileName)
        {
            var result = ValidateFileName(fileName);
            if (!result.IsSuccess)
            {
                var additionalDetails = new List<string>
                {
                    $"Filename: {fileName}",
                    result.Error,
                };
                throw new ErrorException(Errors.File.FileNameInvalid(additionalDetails));
            }

            this.fileName = fileName;
        }

        /// <summary>
        /// Method for validating file name.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>True or error message wrapped in a Result.</returns>
        public static Result ValidateFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return Result.Failure("Invalid Reason: Filename is blank.");
            }

            if (fileName.Length > 255)
            {
                return Result.Failure("Invalid Reason: Filename is too long (must be 255 characters or less)");
            }

            var invalidFileChars = System.IO.Path.GetInvalidFileNameChars();
            if (fileName.Any(c => invalidFileChars.Contains(c)))
            {
                var invalidChars = string.Join(string.Empty, fileName.Where(c => invalidFileChars.Contains(c)));
                return Result.Failure($"Invalid Reason: Filename contains an invalid character(s): {invalidChars}");
            }

            return Result.Success();
        }

        /// <summary>
        /// Determines if filename has a valid extension.
        /// </summary>
        /// <param name="filename">The file name.</param>
        /// <param name="validExtensions">The valid extensions for the file.</param>
        /// <returns>True if filename has a valid extension.</returns>
        public static bool HasValidFileExtension(string filename, params string[] validExtensions)
        {
            return validExtensions.Contains(System.IO.Path.GetExtension(filename));
        }

        /// <summary>
        /// Changes the extension of the file name.
        /// </summary>
        /// <param name="extension">The extension of the file without the leading ".", otherwise it will thrown an error exception.</param>
        /// <returns>The FileName with the new extension.</returns>
        public FileName ChangeExtension(string extension)
        {
            var fileNameWithNewExtension = System.IO.Path.ChangeExtension(this.fileName, extension);
            var newFile = new FileName(fileNameWithNewExtension);
            return newFile;
        }

        /// <summary>
        /// Method to override default ToString method.
        /// </summary>
        /// <returns>The email address.</returns>
        public override string ToString()
        {
            return this.fileName;
        }

        /// <summary>
        /// Method for overriding the GetEqualityCompnents method.
        /// </summary>
        /// <returns>The list of equality components.</returns>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.fileName;
        }
    }
}
