// <copyright file="GemBoxCharacterFormatModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.FileHandling.GemBoxServices.Models
{
    using DotLiquid.Util;
    using GemBox.Document;
    using ServiceStack;
    using UBind.Application.FileHandling.GemBoxServices;

    /// <summary>
    /// The character format model implemented for <see cref="GemBoxMsWordEngineService" /> class.
    /// This class is almost identical to the CharacterFormat class of GemBox.Document however
    /// all the properties are nullable so that we can control the properties that we want to
    /// use or not use when resolving the final CharacterFormat of an element.
    /// </summary>
    public class GemBoxCharacterFormatModel
    {
        public GemBoxCharacterFormatModel()
        {
        }

        public GemBoxCharacterFormatModel(CharacterFormat format)
        {
            this.AllCaps = format.AllCaps;
            this.BackgroundColor = format.BackgroundColor;
            this.Bold = format.Bold;
            this.Border = format.Border;
            this.FontColor = format.FontColor;
            this.FontName = format.FontName;
            this.Hidden = format.Hidden;
            this.HighlightColor = format.HighlightColor;
            this.Italic = format.Italic;
            this.Size = format.Size;
            this.SmallCaps = format.SmallCaps;
            this.Spacing = format.Spacing;
            this.Strikethrough = format.Strikethrough;
            this.Subscript = format.Subscript;
            this.Superscript = format.Superscript;
            this.UnderlineColor = format.UnderlineColor;
            this.UnderlineStyle = format.UnderlineStyle;
        }

        public bool? AllCaps { get; set; }

        public Color? BackgroundColor { get; set; }

        public bool? Bold { get; set; }

        public SingleBorder? Border { get; set; }

        public Color? FontColor { get; set; }

        public string? FontName { get; set; }

        public bool? Hidden { get; set; }

        public Color? HighlightColor { get; set; }

        public bool? Italic { get; set; }

        public double? Size { get; set; }

        public bool? SmallCaps { get; set; }

        public double? Spacing { get; set; }

        public bool? Strikethrough { get; set; }

        public bool? Subscript { get; set; }

        public bool? Superscript { get; set; }

        public Color? UnderlineColor { get; set; }

        public UnderlineType? UnderlineStyle { get; set; }

        /// <summary>
        /// Creates a new instance of the GemBoxCharacterFormatModel class from the difference
        /// between two CharacterFormat instances. All properties will be null except the ones
        /// that are different between the two CharacterFormat instances.
        /// </summary>
        /// <param name="baseFormat">The base CharacterFormat</param>
        /// <param name="format">The CharacterFormat to be compared with the base CharacterFormat</param>
        /// <returns>An instance of GemBoxCharacterFormatModel class</returns>
        public static GemBoxCharacterFormatModel FromCharacterFormatDifference(CharacterFormat baseFormat, CharacterFormat format)
        {
            var model = new GemBoxCharacterFormatModel();
            var properties = model.GetType().GetProperties();
            foreach (var property in properties)
            {
                var baseFormatPropertyValue = baseFormat.GetPropertyValue(property.Name);
                var formatPropertyValue = format.GetPropertyValue(property.Name);

                // If they are not equal, then set the equivalent property of the instance
                // of the CustomCharacterFormat with the value from the modified CharacterFormat
                if (!Equals(baseFormatPropertyValue, formatPropertyValue))
                {
                    property.SetProperty(model, formatPropertyValue);
                }
            }

            return model;
        }

        public bool IsDefault()
        {
            if (this.GetType().GetProperties().Any(a => a.GetValue(this) != null))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Applies all the non-null properties of this class to the specified CharacterFormat instance.
        /// </summary>
        /// <param name="format">The CharacterFormat instance</param>
        public void ApplyToCharacterFormat(CharacterFormat format)
        {
            var characterFormatProperties = format.GetType().GetProperties();

            // Iterate the properties of this class
            var properties = this.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = this.GetPropertyValue(property.Name);
                var baseValue = characterFormatProperties.GetPropertyValue(property.Name);

                if (!Equals(value, null) && !Equals(value, baseValue))
                {
                    characterFormatProperties
                        .Where(a => a.Name == property.Name)
                        .FirstOrDefault()
                        ?.SetProperty(format, value);
                }
            }
        }
    }
}
