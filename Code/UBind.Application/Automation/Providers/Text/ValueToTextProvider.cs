// <copyright file="ValueToTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Text;

using MorseCode.ITask;
using UBind.Application.Automation.Enums;
using UBind.Application.Automation.Helper;
using UBind.Domain;
using UBind.Domain.Exceptions;

public class ValueToTextProvider : IProvider<Data<string?>>
{
    private IProvider<IData>? valueProvider;

    public ValueToTextProvider(IProvider<IData>? valueProvider)
    {
        this.valueProvider = valueProvider;
    }

    public string SchemaReferenceKey => "valueToText";

    /// <inheritdoc/>
    public async ITask<IProviderResult<Data<string?>>> Resolve(IProviderContext providerContext)
    {
        var value = (await this.valueProvider.ResolveValueIfNotNull(providerContext))?.GetValueFromGeneric();
        if (value != null && (DataObjectHelper.IsArray(value) || DataObjectHelper.IsObject(value)))
        {
            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            var valueType = TypeHelper.GetReadableTypeName(value);
            errorData.Add(ErrorDataKey.ValueType, valueType);
            throw new ErrorException(Errors.Automation.ParameterValueTypeInvalid(
                this.SchemaReferenceKey,
                "value",
                null,
                errorData,
                null,
                $"The value resolved for this parameter was of type \"{valueType}\" which is not a type that can be converted into a text value. "));
        }

        return ProviderResult<Data<string?>>.Success(new Data<string?>(DataObjectHelper.ConvertToString(value)));
    }
}
