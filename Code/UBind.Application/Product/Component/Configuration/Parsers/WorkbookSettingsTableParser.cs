// <copyright file="WorkbookSettingsTableParser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Product.Component.Configuration.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json.Linq;
    using UBind.Application.FlexCel;
    using UBind.Domain;
    using UBind.Domain.Attributes;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;

    /// <summary>
    /// Reads and parses the settings tablle from a standard uBind workbook to extract specific values.
    /// </summary>
    public class WorkbookSettingsTableParser : WorkbookTableParser
    {
        private IAttributeObjectPropertyMapRegistry<WorkbookTableSectionPropertyNameAttribute> registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookSettingsTableParser"/> class.
        /// </summary>
        /// <param name="workbook">The uBind workbook.</param>
        /// <param name="registry">The registry of maps from table section property attributes to object properties.
        /// </param>
        public WorkbookSettingsTableParser(
            FlexCelWorkbook workbook,
            IAttributeObjectPropertyMapRegistry<WorkbookTableSectionPropertyNameAttribute> registry)
            : base(workbook, "Table_Settings")
        {
            this.registry = registry;
        }

        /// <summary>
        /// Parses the settings table and populates the component, with a Theme and other settings.
        /// </summary>
        /// <param name="component">The component to populate with information from the settings table.</param>
        public void Parse(Component component)
        {
            component.Form.Theme = this.ParseForTheme();
            component.Form.Theme.GoogleFonts = this.ParseForGoogleFonts();
            component.Form.Theme.ProgressWidgetSettings = this.ParseForProgressWidgetSettings();
            component.Form.Theme.SliderStylingSettings = this.ParseForSliderStylingSettings();
            component.Form.DefaultCurrencyCode = this.GetSectionPropertyValueOrDefault<string>("Financial", null, "Default Currency");
        }

        /// <summary>
        /// Parses the settings table and generates a Theme.
        /// </summary>
        /// <returns>A Theme based upon the settings table from the workbook.</returns>
        public Theme ParseForTheme()
        {
            Dictionary<WorkbookTableSectionPropertyNameAttribute, PropertyInfo> themeAttributeMap =
                this.registry.GetAttributeToPropertyMap(typeof(Theme));
            Theme theme = new Theme();
            foreach (var kvp in themeAttributeMap)
            {
                WorkbookTableSectionPropertyNameAttribute attribute = kvp.Key;
                PropertyInfo property = kvp.Value;
                string majorHeader = attribute.MajorHeader;
                string minorHeader = attribute.MinorHeader;
                string propertyName = attribute.PropertyName;
                Type propertyType = property.PropertyType;

                // e.g: this.GetSectionPropertyValue<string>("Styling", "Other", "Font Awesome Kit")
                var method = this.GetType().GetMethod(
                    nameof(this.GetSectionPropertyValue),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(string), typeof(string), typeof(string) },
                    null);
                var genericMethod = method.MakeGenericMethod(propertyType);
                try
                {
                    var result = genericMethod.Invoke(this, new object[] { majorHeader, minorHeader, propertyName });
                    this.SetValueOfPropertyFromResult(theme, property, result);
                }
                catch (TargetInvocationException ex) when (ex.GetBaseException() is FormatException formatException)
                {
                    throw new ErrorException(
                        Errors.Product.WorkbookParseFailure(
                            $"When attempting to parse a value as a {propertyType}, "
                            + $"the following error occurred: {formatException.Message}",
                            new List<string>
                            {
                                $"Table: {this.TableName}",
                                $"Major header: {majorHeader}",
                                $"Minor header: {minorHeader}",
                                $"Property name: {propertyName}",
                            }),
                        formatException);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.GetBaseException();
                }
            }

            theme.ExternalStyleSheetUrlExpressions = this.GetSectionPropertyValues<string>("Styling", "Style Sheets");
            theme.TooltipIcon = WorkbookParserHelper.AddIconSetClasses(theme.TooltipIcon);
            return theme;
        }

        /// <summary>
        /// Parses the settings table and generates a list of Google Fonts.
        /// </summary>
        /// <returns>A list of the google fonts.</returns>
        public List<GoogleFont> ParseForGoogleFonts()
        {
            List<GoogleFont> result = new List<GoogleFont>();
            List<KeyValuePair<string, string>> googleFontPairs =
                this.GetSectionPropertyKeyValuePairs<string, string>("Styling", "Google Fonts");
            List<KeyValuePair<string, string>> fontWeightPairs =
                this.GetSectionPropertyKeyValuePairs<string, string>("Styling", "Font Weight");
            foreach (var fontKvp in googleFontPairs)
            {
                var weightKvp = fontWeightPairs.Where(kvp => kvp.Key == fontKvp.Key).FirstOrDefault();
                if (!weightKvp.Equals(default(KeyValuePair<string, string>)))
                {
                    result.Add(new GoogleFont
                    {
                        Usage = fontKvp.Key,
                        Family = fontKvp.Value,
                        Weight = weightKvp.Value,
                    });
                }
            }

            return result;
        }

        public ProgressWidgetSettings ParseForProgressWidgetSettings()
        {
            var progressWidgetSettings = new ProgressWidgetSettings();
            this.ParseAndPopulateObject("Progress Widget", "General", progressWidgetSettings);
            progressWidgetSettings.Icons = new ProgressStepIcons();
            this.ParseAndPopulateObject("Progress Widget", "Progress Step Icons", progressWidgetSettings.Icons);
            return progressWidgetSettings;
        }

        public SliderStylingSettings ParseForSliderStylingSettings()
        {
            var sliderSettings = new SliderStylingSettings();
            this.ParseAndPopulateObject("Sliders", "General", sliderSettings);
            sliderSettings.Bar = new SliderStylingSettings.BarSettings();
            this.ParseAndPopulateObject("Sliders", "Bar", sliderSettings.Bar);
            sliderSettings.SelectionBar = new SliderStylingSettings.BarSettings();
            this.ParseAndPopulateObject("Sliders", "Selection Bar", sliderSettings.SelectionBar);
            sliderSettings.Thumb = new SliderStylingSettings.ThumbSettings();
            this.ParseAndPopulateObject("Sliders", "Thumb", sliderSettings.Thumb);
            sliderSettings.Tick = new SliderStylingSettings.TickSettings();
            this.ParseAndPopulateObject("Sliders", "Tick", sliderSettings.Tick);
            sliderSettings.Disabled = new SliderStylingSettings.DisabledSettings();
            this.ParseAndPopulateObject("Sliders", "Disabled", sliderSettings.Disabled);
            sliderSettings.Bubble = new SliderStylingSettings.BubbleSettings();
            this.ParseAndPopulateObject("Sliders", "Bubble", sliderSettings.Bubble);
            sliderSettings.Legend = new SliderStylingSettings.LegendSettings();
            this.ParseAndPopulateObject("Sliders", "Legend", sliderSettings.Legend);
            return sliderSettings;
        }

        /// <summary>
        /// In older workbooks, in the settings tab, there was a section called integrations
        /// where the stripe public API keys were stored per environment. We will read these values
        /// and inject them into the JObject that comes from payment.json.
        /// </summary>
        /// <returns>A JObject with the values, or null if no old stripe public API keys were found.</returns>
        public JObject ParseForOldStripePublicApiKeys()
        {
            var maybeDevelopmentStripePublicApiKey =
                this.GetSectionPropertyValue<string>("Integrations", null, "Development Stripe Public Api Key");
            if (maybeDevelopmentStripePublicApiKey.HasNoValue)
            {
                maybeDevelopmentStripePublicApiKey =
                    this.GetSectionPropertyValue<string>("Integrations", null, "Development Strip Public Api Key");
            }

            var maybeStagingStripePublicApiKey =
                this.GetSectionPropertyValue<string>("Integrations", null, "Staging Stripe Public Api Key");
            if (maybeStagingStripePublicApiKey.HasNoValue)
            {
                maybeStagingStripePublicApiKey =
                    this.GetSectionPropertyValue<string>("Integrations", null, "Staging Strip Public Api Key");
            }

            var maybeProductionStripePublicApiKey =
                this.GetSectionPropertyValue<string>("Integrations", null, "Production Stripe Public Api Key");
            if (maybeProductionStripePublicApiKey.HasNoValue)
            {
                maybeProductionStripePublicApiKey =
                    this.GetSectionPropertyValue<string>("Integrations", null, "Production Strip Public Api Key");
            }

            if (maybeDevelopmentStripePublicApiKey.HasValue
                || maybeStagingStripePublicApiKey.HasValue
                || maybeProductionStripePublicApiKey.HasValue)
            {
                var result = new JObject
                {
                    { "type", "stripe" },
                };

                if (maybeDevelopmentStripePublicApiKey.HasValue)
                {
                    result.Add("development", new JObject
                    {
                        { "publicApiKey", maybeDevelopmentStripePublicApiKey.Value },
                    });
                }

                if (maybeDevelopmentStripePublicApiKey.HasValue)
                {
                    result.Add("staging", new JObject
                    {
                        { "publicApiKey", maybeStagingStripePublicApiKey.Value },
                    });
                }

                if (maybeDevelopmentStripePublicApiKey.HasValue)
                {
                    result.Add("production", new JObject
                    {
                        { "publicApiKey", maybeProductionStripePublicApiKey.Value },
                    });
                }

                return result;
            }

            return null;
        }

        private TObject ParseAndPopulateObject<TObject>(string majorHeader, string minorHeader, TObject targetObject)
        {
            List<KeyValuePair<string, string>> pairs =
                this.GetSectionPropertyKeyValuePairs<string, string>(majorHeader, minorHeader);
            foreach (var pair in pairs)
            {
                if (string.IsNullOrEmpty(pair.Key) || string.IsNullOrEmpty(pair.Value))
                {
                    continue;
                }

                var property = targetObject.GetType().GetProperty(pair.Key.Replace(" ", string.Empty));
                if (property != null)
                {
                    if (property.PropertyType == typeof(bool))
                    {
                        property.SetValue(targetObject, pair.Value.ToBoolean());
                    }
                    else if (property.PropertyType == typeof(bool?))
                    {
                        property.SetValue(targetObject, pair.Value.ToNullableBoolean());
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(targetObject, pair.Value);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(targetObject, int.Parse(pair.Value));
                    }
                    else if (property.PropertyType == typeof(int?))
                    {
                        if (!string.IsNullOrWhiteSpace(pair.Value))
                        {
                            property.SetValue(targetObject, int.Parse(pair.Value));
                        }
                    }
                }
                else
                {
                    throw new ErrorException(
                        Errors.Product.WorkbookParseFailure(
                            $"When attempting to parse a setting \"{majorHeader} > {minorHeader} > {pair.Key}\", "
                            + $"there was no property found with that name. Please check the spelling of it, and that "
                            + "it's a valid configuration option.",
                            new List<string>
                            {
                                $"Table: {this.TableName}",
                                $"Major header: {majorHeader}",
                                $"Minor header: {minorHeader}",
                                $"Setting name: {pair.Key}",
                                $"Setting value: {pair.Value}",
                            }));
                }
            }

            return targetObject;
        }
    }
}
