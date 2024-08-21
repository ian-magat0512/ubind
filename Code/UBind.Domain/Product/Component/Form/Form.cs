// <copyright file="Form.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Validation;

    /// <summary>
    /// Represents the form configuration for a product component.
    /// </summary>
    public class Form
    {
        /// <summary>
        /// Gets or sets the default currency code.
        /// </summary>
        public string DefaultCurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the list of question sets defined for the form.
        /// </summary>
        [ValidateItems]
        public List<QuestionSet> QuestionSets { get; set; }

        /// <summary>
        /// Gets or sets the list of repeating question sets defined for the form.
        /// </summary>
        [ValidateItems]
        public List<RepeatingQuestionSet> RepeatingQuestionSets { get; set; }

        /// <summary>
        /// Gets or sets the list of option sets defined for the form.
        /// </summary>
        [ValidateItems]
        public List<OptionSet> OptionSets { get; set; }

        /// <summary>
        /// Gets or sets the text elements which are used throughout the form.
        /// </summary>
        [ValidateItems]
        public List<TextElement> TextElements { get; set; }

        /// <summary>
        /// Gets or sets the Theme which defines how this form will look and feel.
        /// </summary>
        [ValidateObject]
        public Theme Theme { get; set; }

        /// <summary>
        /// Gets or sets the workflow json configuration for this form.
        /// </summary>
        [Required]
        public JObject WorkflowConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the form model, so that the form will be prefilled with this data when being loaded.
        /// </summary>
        public JObject FormModel { get; set; }

        /// <summary>
        /// Gets or sets a list field keys which matches the rows in the Table_Question_Sets table of a standard ubind
        /// workbook.
        /// An empty string will be included for heading rows.
        /// This is used for calculation requests, and for debugging purposes.
        /// </summary>
        public List<string> WorkbookQuestionSetsTableKeys { get; set; }

        /// <summary>
        /// Gets or sets a list field keys which matches the rows in a standard ubind workbook.
        /// An empty string will be included for heading rows.
        /// This is used for calculation requests, and for debugging purposes.
        /// </summary>
        public List<string> WorkbookRepeatingQuestionSetsTableKeys { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of repeating instance the workbook can handle.
        /// </summary>
        public int RepeatingInstanceMaxQuantity { get; set; }

        /// <summary>
        /// Within the component are references to other objects. When deserializing from json, these references
        /// are not automatically created. This populates them.
        /// For example, a Field holds a reference to it's Question Set, but during deserializing it only
        /// populates the question set key. This function will populate the QuestionSet property with a real
        /// instance.
        /// </summary>
        public void PopulateReferences()
        {
            List<RepeatingField> repeatingFields = new List<RepeatingField>();
            foreach (var questionSet in this.QuestionSets)
            {
                foreach (var field in questionSet.Fields)
                {
                    field.QuestionSet = questionSet;
                    if (field is RepeatingField)
                    {
                        repeatingFields.Add(field as RepeatingField);
                    }
                    else if (field is OptionsField)
                    {
                        OptionsField optionsField = field as OptionsField;
                        if (!string.IsNullOrEmpty(optionsField.OptionSetKey))
                        {
                            optionsField.OptionSet =
                                this.OptionSets.Where(os => os.Key == optionsField.OptionSetKey).First();
                        }
                    }
                }
            }

            foreach (var questionSet in this.RepeatingQuestionSets)
            {
                foreach (var field in questionSet.Fields)
                {
                    field.QuestionSet = questionSet;
                }

                foreach (var repeatingField in repeatingFields)
                {
                    if (questionSet.Name == repeatingField.Name)
                    {
                        repeatingField.QuestionSetToRepeat = questionSet;
                    }
                }
            }
        }
    }
}
