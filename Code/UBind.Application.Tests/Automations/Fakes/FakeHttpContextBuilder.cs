// <copyright file="FakeHttpContextBuilder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Fakes
{
    using System.IO;
    using System.Net;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Factory for creating <see cref="HttpContext"/> for use in tests.
    /// </summary>
    public class FakeHttpContextBuilder
    {
        private string scheme = "https";
        private string host = "localhost";
        private string path = "/dummy";
        private string method = HttpMethods.Get;
        private IPAddress remoteIPAddress = IPAddress.Loopback;
        private string queryString;
        private string content = string.Empty;
        private string contentType = "text/plain";

        public HttpContext Build()
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = this.scheme;
            context.Request.Host = new HostString(this.host);
            context.Request.Path = this.path;
            context.Request.Method = this.method;
            context.Connection.RemoteIpAddress = this.remoteIPAddress;
            if (this.queryString.IsNotNullOrEmpty())
            {
                context.Request.QueryString = QueryString.FromUriComponent(this.queryString);
            }

            if (this.content.IsNotNullOrEmpty())
            {
                byte[] contentBytes = Encoding.UTF8.GetBytes(this.content);
                MemoryStream contentStream = new MemoryStream(contentBytes);
                context.Request.Body = contentStream;
                context.Request.ContentType = this.contentType;
                context.Request.ContentLength = contentBytes.Length;
            }

            return context;
        }

        public FakeHttpContextBuilder WithRequestPath(string path)
        {
            this.path = path;
            return this;
        }

        public FakeHttpContextBuilder WithMethod(string method)
        {
            this.method = method;
            return this;
        }

        public FakeHttpContextBuilder WithQueryString(string queryString)
        {
            this.queryString = queryString;
            return this;
        }

        public FakeHttpContextBuilder WithRemoteIPAddress(IPAddress remoteIPAddress)
        {
            this.remoteIPAddress = remoteIPAddress;
            return this;
        }

        public FakeHttpContextBuilder WithHttpContent(string content, string contentType = "raw/text")
        {
            this.content = content;
            this.contentType = contentType;
            return this;
        }
    }
}
