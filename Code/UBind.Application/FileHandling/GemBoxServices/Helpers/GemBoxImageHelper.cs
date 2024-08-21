// <copyright file="GemBoxImageHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.FileHandling.GemBoxServices.Helpers
{
    using GemBox.Document;

    public class GemBoxImageHelper
    {
        /// <summary>
        /// Calculates the new size of an image
        /// </summary>
        /// <param name="originalSize">The original size of the image</param>
        /// <param name="targetSize">The target size of the image</param>
        /// <param name="preserveWidth">If true, target width will not be scaled and its value
        /// will be used as the final width</param>
        /// <param name="preserveHeight">If true, target height will not be scaled and its value
        /// will be used as the final height</param>
        /// <returns>The computed size of the image</returns>
        public static Size CalculateSize(Size originalSize, Size targetSize, bool preserveWidth, bool preserveHeight)
        {
            // by default, we use target width and height values
            double width = targetSize.Width;
            double height = targetSize.Height;

            // scaledWidth is the target height multiplied to the ratio of the original width vs height
            var scaledWidth = height * originalSize.Width / originalSize.Height;

            // scaledHeight is the target width multiplied to the ratio of the original height vs width
            var scaledHeight = width * originalSize.Height / originalSize.Width;

            // if both width and height are not to be preserved, then no scaling will happen
            if (!preserveWidth && !preserveHeight)
            {
                return targetSize;
            }

            // When both sides of the target are to be preserved, to preserve the aspect ratio,
            // the target size's side that will be preserved will depend on which side of the original size is larger.
            // If the width of the original size is larger than its height, then the target's width will be preserved,
            // and the other will be scaled and vice-versa. If both are equal, then it should use the default values
            // which are the target width and height
            else if (preserveWidth && preserveHeight)
            {
                if (originalSize.Width > originalSize.Height)
                {
                    height = scaledHeight;
                }
                else if (originalSize.Width < originalSize.Height)
                {
                    width = scaledWidth;
                }
            }
            else if (preserveWidth) // if target width is to be preserved, then the height will be scaled
            {
                height = scaledHeight;
            }
            else // if the target height is to be preserved, then the width will be scaled
            {
                width = scaledWidth;
            }

            return new Size(width, height);
        }

        public static bool IsValidImageFileName(string fileName)
        {
            try
            {
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                string extension = Path.GetExtension(fileName).ToLowerInvariant();
                return allowedExtensions.Contains(extension);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
