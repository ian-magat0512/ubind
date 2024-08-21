// <copyright file="X509HttpFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

using System.Security.Cryptography.X509Certificates;
using Flurl.Http.Configuration;

public class X509HttpFactory : DefaultHttpClientFactory
{
    private readonly X509Certificate2 cert;

    public X509HttpFactory(X509Certificate2 cert)
    {
        this.cert = cert;
    }

    public override HttpMessageHandler CreateMessageHandler()
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(this.cert);
        return handler;
    }
}
