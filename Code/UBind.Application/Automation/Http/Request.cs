// <copyright file="Request.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Http;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Flurl;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NullValueHandling = Newtonsoft.Json.NullValueHandling;
using Path = UBind.Domain.ValueTypes.Path;

/// <summary>
/// Represents the resolved <see cref="HttpRequestConfiguration"/> request to be fired/persisted for the automation data.
/// </summary>
public class Request
{
    private JObject? queryParameters;
    private Uri? uri;

    /// <summary>
    /// Initializes a new instance of the <see cref="Request"/> class.
    /// </summary>
    /// <param name="url">The url to be used.</param>
    /// <param name="httpVerb">The http method of the request.</param>
    /// <param name="headers">The headers for the request, if any.</param>
    /// <param name="contentType">The type of content, if any, of the response.</param>
    /// <param name="characterSet">The character set, e.g. "UTF-8". Only applicable if it's a text MIME type.</param>
    /// <param name="content">The content of the request.</param>
    public Request(
        string url,
        string httpVerb,
        Dictionary<string, StringValues> headers,
        string contentType,
        string characterSet,
        object content,
        ClientCertificateData? clientCertificateData = null)
    {
        this.Url = url;
        this.HttpVerb = httpVerb;
        this.Headers = headers;
        this.ContentType = contentType;
        this.CharacterSet = characterSet;
        this.Content = content;
        this.ClientCertificateData = clientCertificateData;
    }

    [JsonConstructor]
    protected Request([NotNull] string url, [NotNull] string httpVerb)
    {
        this.Url = url;
        this.HttpVerb = httpVerb;
    }

    /// <summary>
    /// Gets the url to be used in the request.
    /// </summary>
    [JsonProperty("url", Required = Required.Always)]
    public string Url { get; private set; }

    [JsonProperty("scheme", NullValueHandling = NullValueHandling.Ignore)]
    public string? Scheme
    {
        get => this.Uri?.Scheme;
        set
        {
            // this property doesn't need to be deserialized, but it's useful for object path lookups.
        }
    }

    [JsonProperty("host", NullValueHandling = NullValueHandling.Ignore)]
    public string? Host
    {
        get => this.Uri?.Host;
        set
        {
            // this property doesn't need to be deserialized, but it's useful for object path lookups.
        }
    }

    [JsonProperty("port", NullValueHandling = NullValueHandling.Ignore)]
    public int? Port
    {
        get => this.Uri?.Port;
        set
        {
            // this property doesn't need to be deserialized, but it's useful for object path lookups.
        }
    }

    [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
    public string? Path
    {
        get => this.Uri?.AbsolutePath;
    }

    [JsonProperty("pathSegments", NullValueHandling = NullValueHandling.Ignore)]
    public string[]? PathSegments
    {
        get => this.Uri != null ? new Path(this.Uri.AbsolutePath).Segments : null;
        set
        {
            // this property doesn't need to be deserialized, but it's useful for object path lookups.
        }
    }

    [JsonProperty("queryString", NullValueHandling = NullValueHandling.Ignore)]
    public string? QueryString
    {
        get => this.Uri?.Query;
        set
        {
            // this property doesn't need to be deserialized, but it's useful for object path lookups.
        }
    }

    [JsonProperty("queryParameters", NullValueHandling = NullValueHandling.Ignore)]
    public JObject QueryParameters
    {
        get
        {
            if (this.queryParameters == null)
            {
                JObject jObject = new JObject();
                if (!string.IsNullOrEmpty(this.Url))
                {
                    Url flurlUrl = new Url(this.Url);
                    foreach (var queryParam in flurlUrl.QueryParams)
                    {
                        if (string.IsNullOrEmpty(queryParam.Name) || queryParam.Value == null)
                        {
                            // we can't add a param without a name
                            // null param values are not typical, but it will just be ignored by flurl
                            // skip them here to prevent further processing
                            continue;
                        }

                        if (jObject.ContainsKey(queryParam.Name))
                        {
                            // If the parameter already exists, it's an array-like query parameter
                            if (jObject[queryParam.Name] is JArray existingArray)
                            {
                                // Add to the existing array
                                existingArray.Add(queryParam.Value.ToString());
                            }
                            else
                            {
                                // Convert the existing single value to an array
                                var array = new JArray();
                                if (jObject[queryParam.Name] != null)
                                {
                                    array.Add(jObject[queryParam.Name]!);
                                }
                                array.Add(queryParam.Value.ToString());
                                jObject[queryParam.Name] = array;
                            }
                        }
                        else
                        {
                            // Add the query parameter as a single value
                            jObject.Add(queryParam.Name, queryParam.Value.ToString());
                        }
                    }
                }

                this.queryParameters = jObject;
            }

            return this.queryParameters;
        }

        set
        {
            // this property doesn't need to be deserialized, but it's useful for object path lookups.
        }
    }

    /// <summary>
    /// Gets the http method of the request.
    /// </summary>
    [JsonProperty("httpVerb", Required = Required.Always)]
    public string HttpVerb { get; private set; }

    /// <summary>
    /// Gets the headers to be used.
    /// </summary>
    [JsonProperty("headers", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(HeadersConverter))]
    public Dictionary<string, StringValues>? Headers { get; private set; }

    /// <summary>
    /// Gets the content type of the response.
    /// </summary>
    [JsonProperty("contentType", NullValueHandling = NullValueHandling.Ignore)]
    public string? ContentType { get; private set; }

    /// <summary>
    /// Gets the character set of the response (e.g. "UTF-8").
    /// This is only set for text based content types.
    /// </summary>
    [JsonProperty(PropertyName = "characterSet", NullValueHandling = NullValueHandling.Ignore)]
    public string? CharacterSet { get; private set; }

    /// <summary>
    /// Gets the content of the response. This could be either a string, a byte array, a JToken
    /// or a list of MIME multipart as instances of <see cref="ContentPart"/>.
    /// If the content type is a text MIME type, then this will be a string. If it's binary it
    /// will be a byte array. If it's a multi-part response then this will be an list of parts
    /// with each part having a ContentType, CharacterSet, and Content property.
    /// </summary>
    [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(HttpContentJsonConverter))]
    public object? Content { get; private set; }

    [JsonProperty("clientCertificate", NullValueHandling = NullValueHandling.Ignore)]
    public ClientCertificateData? ClientCertificateData { get; private set; }

    [JsonIgnore]
    private Uri? Uri
    {
        get
        {
            if (this.uri == null && !string.IsNullOrEmpty(this.Url))
            {
                this.uri = new Uri(this.Url);
            }

            return this.uri;
        }
    }
}
