// <copyright file="ValueToIntegerProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Integer;

using MorseCode.ITask;
using System.Globalization;
using UBind.Application.Automation.Enums;
using UBind.Application.Automation.Helper;
using UBind.Application.Automation.Providers;
using UBind.Domain;
using UBind.Domain.Exceptions;

public class ValueToIntegerProvider : IProvider<Data<long>>
{
    private readonly IProvider<IData>? valueProvider;

    public ValueToIntegerProvider(IProvider<IData>? valueProvider)
    {
        this.valueProvider = valueProvider;
    }

    public string SchemaReferenceKey => "valueToInteger";

    /// <inheritdoc/>
    public async ITask<IProviderResult<Data<long>>> Resolve(IProviderContext providerContext)
    {
        var value = (await this.valueProvider.ResolveValueIfNotNull(providerContext))?.GetValueFromGeneric();
        if (value == null)
        {
            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            throw new ErrorException(Errors.Automation.RequiredProviderParameterValueMissing(this.SchemaReferenceKey, "value", errorData));
        }

        if (value != null && (DataObjectHelper.IsArray(value) || DataObjectHelper.IsObject(value) || value is bool))
        {
            var invalidValue = value is bool ? value : null;
            var typeName = TypeHelper.GetReadableTypeName(value);
            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            if (invalidValue != null)
            {
                errorData.Add(ErrorDataKey.ValueToParse, invalidValue);
            }
            errorData.Add(ErrorDataKey.ValueType, typeName);
            throw new ErrorException(Errors.Automation.ParameterValueTypeInvalid(
                this.SchemaReferenceKey,
                "value",
                invalidValue,
                errorData,
                null,
                $"The value resolved for this parameter was of type \"{typeName}\" which is not a type that can be converted into an integer value. "));
        }

        var success = long.TryParse(value?.ToString(), NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out long result);
        if (success)
        {
            return ProviderResult<Data<long>>.Success(new Data<long>(result));
        }
        else
        {
            var typeName = TypeHelper.GetReadableTypeName(value);
            var invalidValue = $"\"{value}\"";
            var reasonWhyValueIsInvalidIfAvailable = "The text value resolved for this parameter could not be parsed as an integer value. ";
            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            if (value is decimal || value is double)
            {
                invalidValue = value.ToString();
                reasonWhyValueIsInvalidIfAvailable = "The value resolved for this parameter was a decimal number and therefore could not be converted into an integer value. ";
            }
            errorData.Add(ErrorDataKey.ValueToParse, value);
            errorData.Add(ErrorDataKey.ValueType, typeName);

            throw new ErrorException(Errors.Automation.ParameterValueTypeInvalid(
                this.SchemaReferenceKey,
                "value",
                invalidValue,
                errorData,
                null,
                reasonWhyValueIsInvalidIfAvailable));
        }
    }
}
