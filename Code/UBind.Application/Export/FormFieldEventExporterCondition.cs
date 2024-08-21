// <copyright file="FormFieldEventExporterCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Threading.Tasks;
    using UBind.Domain;

    /// <summary>
    /// Factory for form field conditions.
    /// </summary>
    public class FormFieldEventExporterCondition : EventExporterCondition
    {
        private readonly ITextProvider fieldNameProvider;
        private readonly ITextProvider valueProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormFieldEventExporterCondition"/> class.
        /// </summary>
        /// <param name="fieldNameProvider">Factory for generating a string containing the field name.</param>
        /// <param name="valueProvider">Factory for generating a string containing the value.</param>
        public FormFieldEventExporterCondition(ITextProvider fieldNameProvider, ITextProvider valueProvider)
        {
            this.fieldNameProvider = fieldNameProvider;
            this.valueProvider = valueProvider;
        }

        /// <inheritdoc/>
        public override async Task<bool> Evaluate(ApplicationEvent applicationEvent)
        {
            var fieldName = await this.fieldNameProvider.Invoke(applicationEvent);
            var value = await this.valueProvider.Invoke(applicationEvent);
            var result = fieldName is null ? false : fieldName.Equals(value);
            this.DebugInfo = $"When checking whether a field matches a value, we checked that \"{fieldName}\" equals \"{value}\" and it turned out to be " + (result ? "TRUE" : "FALSE");
            return result;
        }
    }
}
