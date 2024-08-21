// <copyright file="ValueToNumberProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Number;

using MorseCode.ITask;
using System.Globalization;
using UBind.Application.Automation.Enums;
using UBind.Application.Automation.Helper;
using UBind.Application.Automation.Providers;
using UBind.Domain;
using UBind.Domain.Exceptions;

public class ValueToNumberProvider : IProvider<Data<decimal>>
{
    private readonly IProvider<IData>? valueProvider;

    public ValueToNumberProvider(IProvider<IData>? valueProvider) =>
        this.valueProvider = valueProvider;

    public string SchemaReferenceKey => "valueToNumber";

    /// <inheritdoc/>
    public async ITask<IProviderResult<Data<decimal>>> Resolve(IProviderContext providerContext)
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
                $"The value resolved for this parameter was of type \"{typeName}\" which is not a type that can be converted into a number value. "));
        }

        var success = decimal.TryParse(value?.ToString(), NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, null, out decimal result);
        if (success)
        {
            return ProviderResult<Data<decimal>>.Success(new Data<decimal>(result));
        }
        else
        {
            var typeName = TypeHelper.GetReadableTypeName(value);
            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            errorData.Add(ErrorDataKey.ValueToParse, value);
            errorData.Add(ErrorDataKey.ValueType, typeName);
            throw new ErrorException(Errors.Automation.ParameterValueTypeInvalid(
                this.SchemaReferenceKey,
                "value",
                $"\"{value}\"",
                errorData,
                null,
                $"The text value resolved for this parameter could not be parsed as a number value. "));
        }
    }
}
