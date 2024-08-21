// <copyright file="FormDataFieldFormatterFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System.Collections.Generic;
    using CSharpFunctionalExtensions;
    using UBind.Domain.FormDataFieldFormatters;
    using UBind.Domain.Product.Component.Form;

    /// <summary>
    /// A Factory class for formdata field formatters.
    /// </summary>
    public class FormDataFieldFormatterFactory : IFormDataFieldFormatterFactory
    {
        private Dictionary<DataType, IFormDataFieldFormatter> fieldFormattersMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormDataFieldFormatterFactory"/> class.
        /// </summary>
        public FormDataFieldFormatterFactory()
        {
            this.fieldFormattersMap = new Dictionary<DataType, IFormDataFieldFormatter>();
            this.fieldFormattersMap.Add(DataType.Abn, new FormDataAbnFieldFormatter());
            this.fieldFormattersMap.Add(DataType.Boolean, new FormDataBooleanFieldFormatter());
            this.fieldFormattersMap.Add(DataType.Currency, new FormDataCurrencyFieldFormatter());
            this.fieldFormattersMap.Add(DataType.Email, new FormDataEmailAddressFieldFormatter());
            this.fieldFormattersMap.Add(DataType.Number, new FormDataNumberFieldFormatter());
            this.fieldFormattersMap.Add(DataType.Percent, new FormDataPercentFieldFormatter());
            this.fieldFormattersMap.Add(DataType.Phone, new FormDataPhoneNumberFieldFormatter());
        }

        /// <inheritdoc/>
        public Result<IFormDataFieldFormatter> Create(DataType dataType)
        {
            IFormDataFieldFormatter field;
            this.fieldFormattersMap.TryGetValue(dataType, out field);
            if (field == null)
            {
                return Result.Failure<IFormDataFieldFormatter>($"When trying to prettify form data, we came across a field of type \"{dataType}\" "
                    + "however we don't have a matching field formatter class.");
            }

            return Result.Success(field);
        }
    }
}
