// <copyright file="JobStatusResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.ViewModel;

using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using UBind.Domain;

public class JobStatusResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JobStatusResult"/> class.
    /// </summary>
    /// <param name="jobStatusResult">The job status result.</param>
    public JobStatusResult(Result<JobStatusResponse, Error> jobStatusResult)
    {
        this.IsFailure = jobStatusResult.IsFailure;
        this.IsSuccess = jobStatusResult.IsSuccess;
        this.Value = jobStatusResult.Value;
    }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "isFailure")]
    public bool IsFailure { get; }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "isSuccess")]
    public bool IsSuccess { get; }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "value")]
    public JobStatusResponse Value { get; }
}
