// <copyright file="WorkbookProductComponentConfigurationReader.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Product.Component.Configuration.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MoreLinq;
    using StackExchange.Profiling;
    using UBind.Application.FlexCel;
    using UBind.Domain;
    using UBind.Domain.Attributes;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;

    /// <summary>
    /// Reads product configuration from a standard UBind workbook.
    /// </summary>
    public class WorkbookProductComponentConfigurationReader : IWorkbookProductComponentConfigurationReader
    {
        private IAttributeObjectPropertyMapRegistry<WorkbookTableColumnNameAttribute> columnNameRegistry;
        private IWorkbookFieldFactory fieldFactory;
        private IAttributeObjectPropertyMapRegistry<WorkbookTableSectionPropertyNameAttribute> sectionPropertyRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookProductComponentConfigurationReader"/> class.
        /// </summary>
        /// <param name="columnNameRegistry">The registry of maps from column name to object property.</param>
        /// <param name="sectionPropertyRegistry">The registry of maps from section property names to object propery.
        /// </param>
        /// <param name="fieldFactory">The field factory that creates instances of fields by type.</param>
        public WorkbookProductComponentConfigurationReader(
            IAttributeObjectPropertyMapRegistry<WorkbookTableColumnNameAttribute> columnNameRegistry,
            IAttributeObjectPropertyMapRegistry<WorkbookTableSectionPropertyNameAttribute> sectionPropertyRegistry,
            IWorkbookFieldFactory fieldFactory)
        {
            this.columnNameRegistry = columnNameRegistry;
            this.sectionPropertyRegistry = sectionPropertyRegistry;
            this.fieldFactory = fieldFactory;
        }

        /// <summary>
        /// Generates the configuration by reading data from the workbook.
        /// </summary>
        /// <param name="workbook">An instance of the workbook.</param>
        /// <returns>A JObject with the configuration.</returns>
        public Component Read(FlexCelWorkbook workbook)
        {
            using (MiniProfiler.Current.Step(nameof(WorkbookProductComponentConfigurationReader) + "." + nameof(this.Read)))
            {
                List<OptionSet> optionSets = new WorkbookOptionsTableParser(workbook, this.columnNameRegistry).Parse();
                List<QuestionSet> questionSets = new WorkbookQuestionSetsTableParser(
                            workbook,
                            this.fieldFactory,
                            optionSets,
                            this.columnNameRegistry).Parse();
                List<RepeatingQuestionSet> repeatingQuestionSets = new WorkbookRepeatingQuestionSetsTableParser(
                            workbook,
                            this.fieldFactory,
                            optionSets,
                            this.columnNameRegistry).Parse();
                this.SetQuestionSetToRepeatForRepeatingFields(questionSets, repeatingQuestionSets);
                var repeatingInstanceMaxQuantity = workbook.GetNamedRange("Repeating Question Sets", "Repeating_Question_Sets_Values").ColCount;
                var component = new Component
                {
                    Form = new Form
                    {
                        OptionSets = optionSets,
                        QuestionSets = questionSets,
                        RepeatingQuestionSets = repeatingQuestionSets,
                        RepeatingInstanceMaxQuantity = repeatingInstanceMaxQuantity,
                    },
                    CalculatesUsingStandardWorkbook = true,
                };
                new WorkbookTriggersTableParser(workbook, this.columnNameRegistry).Parse(component);
                new WorkbookTextTableParser(workbook, this.columnNameRegistry).Parse(component);
                new WorkbookSettingsTableParser(workbook, this.sectionPropertyRegistry).Parse(component);
                new WorkbookStylesTableParser(workbook, this.columnNameRegistry).Parse(component);
                this.DetectFontsInStylesAndAddToTheme(component.Form.Theme);
                this.DetermineCalculationWidgetHeaderBackgroundColour(component.Form.Theme);
                this.AddStylesForSmallDevices(component.Form.Theme);
                this.AddStylesForCalculationWidget(component.Form.Theme);
                return component;
            }
        }

        /// <summary>
        /// There's a convention in the workbook that for a Repeating Field, the name of that field must be the
        /// exact same name as the Repeating Question Set. We need to link the repeating field to the Question Set
        /// that it needs to be repeat, so we will use this convention to do that.
        /// </summary>
        /// <param name="questionSets">The question sets within the form configuration.</param>
        /// <param name="repeatingQuestionSets">The repeating question sets within the form configuration.</param>
        private void SetQuestionSetToRepeatForRepeatingFields(
            List<QuestionSet> questionSets,
            List<RepeatingQuestionSet> repeatingQuestionSets)
        {
            IEnumerable<Field> repeatingFields = questionSets
                .SelectMany(questionSet => questionSet.Fields, (questionSet, field) => field)
                .Where(field => field is RepeatingField);
            foreach (Field repeatingField in repeatingFields)
            {
                RepeatingQuestionSet questionSetToRepeat =
                    repeatingQuestionSets.Where(qs => qs.Name == repeatingField.Name).FirstOrDefault();
                if (questionSetToRepeat == null)
                {
                    throw new ErrorException(Errors.Product.WorkbookParseFailure(
                            $"The repeating field in question set {repeatingField.QuestionSet.Name} named "
                            + $"{repeatingField.Name} is supposed to have a question set with the same name, "
                            + "however it could not be found. Please ensure you create a question set with the same "
                            + "name as the repeating field."));
                }

                questionSetToRepeat.RepeatingFieldKey = repeatingField.Key;
                ((RepeatingField)repeatingField).QuestionSetToRepeat = questionSetToRepeat;
            }
        }

        /// <summary>
        /// Since workbooks don't define the background colour for the calculation widget in
        /// the settings tab, we'll try to read it from the styles.
        /// </summary>
        /// <param name="theme">The theme.</param>
        private void DetermineCalculationWidgetHeaderBackgroundColour(Theme theme)
        {
            if (string.IsNullOrEmpty(theme.CalculationWidgetHeaderBackgroundColour))
            {
                Style style = theme.Styles.Where(s => s.Category == "Sidebar" && s.Name == "Header").FirstOrDefault();
                if (style != null)
                {
                    theme.CalculationWidgetHeaderBackgroundColour = style.Background;
                }
            }
        }

        /// <summary>
        /// Adds references to unique google fonts found in the styles, so they can be loaded on startup.
        /// </summary>
        /// <param name="theme">The theme of the form.</param>
        private void DetectFontsInStylesAndAddToTheme(Theme theme)
        {
            var uniqueFonts = new HashSet<Tuple<string, string>>();
            theme.Styles.Where(s => !string.IsNullOrEmpty(s.FontFamily) && !string.IsNullOrEmpty(s.FontWeight))
                .ForEach(s => uniqueFonts.Add(new Tuple<string, string>(s.FontFamily, s.FontWeight)));
            uniqueFonts.ForEach(f => theme.GoogleFonts.Add(new GoogleFont
            {
                Family = f.Item1,
                Weight = f.Item2,
            }));
        }

        private void AddStylesForSmallDevices(Theme theme)
        {
            string smallDeviceWrapper = "@media(max-width:767px)";
            theme.Styles.Add(new Style
            {
                Category = "Panels",
                Name = "Small Devices Body",
                Wrapper = smallDeviceWrapper,
                Selector = "body",
                MarginRight = "0",
                MarginLeft = "0",
            });
            theme.Styles.Add(new Style
            {
                Category = "Panels",
                Name = "Small Devices Sidbar",
                Wrapper = smallDeviceWrapper,
                Selector = "#sidebar .body",
                MarginBottom = "0",
            });
            if (theme.SmallDeviceMarginPixels != null)
            {
                theme.Styles.Add(new Style
                {
                    Category = "Panels",
                    Name = "Small Devices Article",
                    Wrapper = smallDeviceWrapper,
                    Selector = "article",
                    PaddingRight = $"{theme.SmallDeviceMarginPixels}px",
                    PaddingLeft = $"{theme.SmallDeviceMarginPixels}px",
                });
                theme.Styles.Add(new Style
                {
                    Category = "Panels",
                    Name = "Small Devices Nav Footer Actions",
                    Wrapper = smallDeviceWrapper,
                    Selector = "nav#footer-actions",
                    MarginRight = $"{theme.SmallDeviceMarginPixels}px",
                    MarginLeft = $"{theme.SmallDeviceMarginPixels}px",
                });
                theme.Styles.Add(new Style
                {
                    Category = "Panels",
                    Name = "Small Devices H1",
                    Wrapper = smallDeviceWrapper,
                    Selector = "h1",
                    MarginRight = $"{theme.SmallDeviceMarginPixels}px",
                    MarginLeft = $"{theme.SmallDeviceMarginPixels}px",
                });
            }
        }

        private void AddStylesForCalculationWidget(Theme theme)
        {
            if (!string.IsNullOrWhiteSpace(theme.CalculationWidgetHeaderBackgroundColour))
            {
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Header Before",
                    Selector = "#sidebar #calculation .header:before",
                    BorderRight = $"10px solid {theme.CalculationWidgetHeaderBackgroundColour}",
                });
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Arrow After",
                    Selector = "#sidebar #calculation .arrow:after",
                    BorderTop = $"10px solid {theme.CalculationWidgetHeaderBackgroundColour}",
                });
            }

            if (!string.IsNullOrWhiteSpace(theme.CalculationWidgetReviewBackgroundColour))
            {
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Review Header",
                    Selector = "#sidebar #calculation.state-review .header",
                    Background = theme.CalculationWidgetReviewBackgroundColour,
                });
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Review Header Before",
                    Selector = "#sidebar #calculation.state-review .header:before",
                    BorderRight = $"10px solid {theme.CalculationWidgetReviewBackgroundColour}",
                });
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Review Arrow After",
                    Selector = "#sidebar #calculation.state-review .arrow:after",
                    BorderTop = $"10px solid {theme.CalculationWidgetReviewBackgroundColour}",
                });
            }

            if (!string.IsNullOrWhiteSpace(theme.CalculationWidgetSoftReferralBackgroundColour))
            {
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Soft Referral Header",
                    Selector = "#sidebar #calculation.state-softReferral .header",
                    Background = theme.CalculationWidgetSoftReferralBackgroundColour,
                });
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Soft Referral Header Before",
                    Selector = "#sidebar #calculation.state-softReferral .header:before",
                    BorderRight = $"10px solid {theme.CalculationWidgetSoftReferralBackgroundColour}",
                });
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Soft Referral Arrow After",
                    Selector = "#sidebar #calculation.state-softReferral .arrow:after",
                    BorderTop = $"10px solid {theme.CalculationWidgetSoftReferralBackgroundColour}",
                });
            }

            if (!string.IsNullOrWhiteSpace(theme.CalculationWidgetEndorsementBackgroundColour))
            {
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Endorsement Header",
                    Selector = "#sidebar #calculation.state-endorsement .header",
                    Background = theme.CalculationWidgetEndorsementBackgroundColour,
                });
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Endorsement Header Before",
                    Selector = "#sidebar #calculation.state-endorsement .header:before",
                    BorderRight = $"10px solid {theme.CalculationWidgetEndorsementBackgroundColour}",
                });
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Endorsement Arrow After",
                    Selector = "#sidebar #calculation.state-endorsement .arrow:after",
                    BorderTop = $"10px solid {theme.CalculationWidgetEndorsementBackgroundColour}",
                });
            }

            if (!string.IsNullOrWhiteSpace(theme.CalculationWidgetHardReferralBackgroundColour))
            {
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Hard Referral Header",
                    Selector = "#sidebar #calculation.state-hardReferral .header",
                    Background = theme.CalculationWidgetHardReferralBackgroundColour,
                });
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Hard Referral Header Before",
                    Selector = "#sidebar #calculation.state-hardReferral .header:before",
                    BorderRight = $"10px solid {theme.CalculationWidgetHardReferralBackgroundColour}",
                });
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Hard Referral Arrow After",
                    Selector = "#sidebar #calculation.state-hardReferral .arrow:after",
                    BorderTop = $"10px solid {theme.CalculationWidgetHardReferralBackgroundColour}",
                });
            }

            if (!string.IsNullOrWhiteSpace(theme.CalculationWidgetDeclineBackgroundColour))
            {
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Decline Header",
                    Selector = "#sidebar #calculation.state-decline .header",
                    Background = theme.CalculationWidgetDeclineBackgroundColour,
                });
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Decline Header Before",
                    Selector = "#sidebar #calculation.state-decline .header:before",
                    BorderRight = $"10px solid {theme.CalculationWidgetDeclineBackgroundColour}",
                });
                theme.Styles.Add(new Style
                {
                    Category = "Calculation",
                    Name = "Decline Arrow After",
                    Selector = "#sidebar #calculation.state-decline .arrow:after",
                    BorderTop = $"10px solid {theme.CalculationWidgetDeclineBackgroundColour}",
                });
            }
        }
    }
}
