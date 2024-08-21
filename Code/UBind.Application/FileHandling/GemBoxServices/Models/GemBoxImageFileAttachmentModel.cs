// <copyright file="GemBoxImageFileAttachmentModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.GemBoxServices.Models
{
    using System;
    using Newtonsoft.Json.Linq;
    using UBind.Application.FileHandling.GemBoxServices;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;

    /// <summary>
    /// The image file attachment model implemented for <see cref="GemBoxMsWordEngineService" /> class.
    /// </summary>
    public class GemBoxImageFileAttachmentModel
    {
        private readonly string data;
        private readonly string fieldName;

        private GemBoxImageFileAttachmentModel(string data, string fieldName)
        {
            this.fieldName = fieldName;
            this.data = data;
        }

        public string FileName { get; private set; }

        public string MimeType { get; private set; }

        public Guid AttachmentId { get; private set; }

        public double Width { get; private set; }

        public double Height { get; private set; }

        public static GemBoxImageFileAttachmentModel Create(string data, string fieldName)
        {
            if (string.IsNullOrEmpty(data))
            {
                var errorData = new JObject();
                errorData.Add("imageData", data);
                throw new ErrorException(
                    Errors.DocumentGeneration.ImageFieldMerging.InvalidImageFieldValueImageData(
                        fieldName,
                        errorData));
            }

            var instance = new GemBoxImageFileAttachmentModel(data, fieldName);
            instance.ParseString(data);
            return instance;
        }

        private void ParseString(string data)
        {
            var splitData = data.Split(':');
            if (splitData == null || splitData.Length < 5)
            {
                var errorData = this.GenerateErrorData();
                throw new ErrorException(
                    Errors.DocumentGeneration.ImageFieldMerging.InvalidImageFieldValueImageData(
                        this.fieldName,
                        errorData));
            }

            this.FileName = splitData[0];
            if (!this.IsValidFileName(this.FileName))
            {
                var errorData = this.GenerateErrorData();
                throw new ErrorException(
                    Errors.DocumentGeneration.ImageFieldMerging.InvalidImageFieldValueFileFormat(
                        this.fieldName,
                        this.FileName,
                        errorData));
            }

            this.MimeType = splitData[1];
            if (!this.IsValidMimeType(this.MimeType))
            {
                var errorData = this.GenerateErrorData();
                throw new ErrorException(
                    Errors.DocumentGeneration.ImageFieldMerging.InvalidImageFieldValueMimeType(
                        this.fieldName,
                        this.MimeType,
                        errorData));
            }

            var attachmentIdString = splitData[2];
            var guid = new GuidOrAlias(attachmentIdString).Guid;
            if (!guid.HasValue)
            {
                var errorData = this.GenerateErrorData();
                throw new ErrorException(
                    Errors.DocumentGeneration.ImageFieldMerging.AttachmentIdNotFound(
                        this.fieldName,
                        errorData));
            }

            this.AttachmentId = guid.Value;
            this.SetDimensions(splitData[3], splitData[4]);
        }

        private bool IsValidMimeType(string mimeType)
        {
            string[] allowedMimeTypes = { "image/gif", "image/jpeg", "image/png" };
            foreach (var allowedMimeType in allowedMimeTypes)
            {
                if (string.Equals(mimeType, allowedMimeType, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsValidFileName(string fileName)
        {
            if (!fileName.Contains('.'))
            {
                return false;
            }

            var splitFileName = fileName.Split('.');
            var extension = splitFileName[splitFileName.Length - 1].ToLower();
            string[] validExtensions = { "png", "gif", "jpeg", "jpg" };
            if (!validExtensions.Contains(extension))
            {
                return false;
            }

            return true;
        }

        private void SetDimensions(string widthString, string heightString)
        {
            if (!double.TryParse(widthString, out double width))
            {
                var errorData = this.GenerateErrorData();
                throw new ErrorException(
                    Errors.DocumentGeneration.ImageFieldMerging.InvalidImageFieldValueImageSize(
                        this.fieldName,
                        widthString,
                        heightString,
                        errorData));
            }

            if (!double.TryParse(heightString, out double height))
            {
                var errorData = this.GenerateErrorData();
                throw new ErrorException(
                    Errors.DocumentGeneration.ImageFieldMerging.InvalidImageFieldValueImageSize(
                        this.fieldName,
                        widthString,
                        heightString,
                        errorData));
            }

            this.Width = width;
            this.Height = height;
        }

        private JObject GenerateErrorData()
        {
            var errorData = new JObject
            {
                { "imageData", this.data },
            };
            return errorData;
        }
    }
}
