// <copyright file="TemplateJObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Collections.Generic;
    using System.Linq;
    using MoreLinq;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;

    /// <summary>
    /// Defines the <see cref="TemplateJObjectProvider" />.
    /// A base class for Template JObjectProviders.
    /// </summary>
    public abstract class TemplateJObjectProvider
    {
        /// <summary>
        /// Gets the JsonObject to be passed to the template provider.
        /// </summary>
        public JObject JsonObject { get; private set; } = new JObject();

        /// <summary>
        /// Sets the intances JsonObject property from a specific property within the textElements subkey to the products configuration.
        /// </summary>
        /// <param name="config">The product configuration.</param>
        /// <param name="sourcePropertyName">the source json property name under the "textElements" property.</param>
        /// <param name="targetPropertyName">The target property name for the json object.</param>
        protected void SetJsonObjectFromParsedConfigTextElement(string config, string sourcePropertyName, string targetPropertyName)
        {
            // TODO: ensure that the json configuration is valid at the time the release is created, so that each consumer does not have to do their own cleanup
            var rawConfigObj = JObject.Parse(config.Replace(",}", "}"));
            var baseConfigObj = JObject.Parse(rawConfigObj["baseConfiguration"].ToString());
            if (baseConfigObj["textElements"][sourcePropertyName] != null)
            {
                var productsObj = baseConfigObj["textElements"][sourcePropertyName] as JObject;
                var parser = new TextElementsJObjectParser(targetPropertyName, productsObj);
                if (parser.JsonObject != null)
                {
                    this.JsonObject = parser.JsonObject;
                }
            }
        }

        /// <summary>
        /// Creates a JObject with the text elements from a given category.
        /// </summary>
        /// <param name="component">The product component configuration.</param>
        /// <param name="category">The category of the text elements.</param>
        /// <param name="targetPropertyName">The target property name for the json object.</param>
        protected void SetJsonObjectFromTextElementCategory(Component component, string category, string targetPropertyName)
        {
            IEnumerable<TextElement> textElements = component.Form.TextElements
                .Where(te => te.Category == category);
            var jObject = new JObject();
            textElements.ForEach(textElement => jObject.Add(textElement.Name, new JObject
            {
                { "text", new JValue(textElement.Text) },
                { "icon", new JValue(textElement.Icon) },
            }));
            var parser = new TextElementsJObjectParser(targetPropertyName, jObject);
            if (parser.JsonObject != null)
            {
                this.JsonObject = parser.JsonObject;
            }
        }
    }
}
