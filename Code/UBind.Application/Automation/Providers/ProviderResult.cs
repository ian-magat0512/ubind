// <copyright file="ProviderResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers;

/// <summary>
/// This class is used to return a result from a provider.
/// It will contain either a value or an error before it throws an exception.
/// Since the Result library for C# is not applicable for out scenario isnce the IProvider interface was need to be out the TDataValue generic type.
/// Some callers of IProvider interface should be not to cast the result to the TDataValue type.
/// </summary>
/// <typeparam name="TDataValue">The generic type of the provider</typeparam>
public class ProviderResult<TDataValue> : IProviderResult<TDataValue>
{
    private ProviderResult(TDataValue? value)
    {
        this.IsSuccess = true;
        this.Value = value;
    }

    private ProviderResult(Domain.Error error)
    {
        this.IsFailure = true;
        this.Error = error;
    }

    public bool IsSuccess { get; private set; }

    public bool IsFailure { get; private set; }

    public TDataValue? Value { get; private set; }

    public Domain.Error? Error { get; private set; }

    public static ProviderResult<TDataValue> Success(TDataValue? value) =>
        new ProviderResult<TDataValue>(value);

    public static ProviderResult<TDataValue> Failure(Domain.Error error) =>
        new ProviderResult<TDataValue>(error);
}
