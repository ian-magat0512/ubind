// <copyright file="TestEwayConfigurationFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Payment
{
    using UBind.Application.Payment;
    using UBind.Application.Payment.Eway;

    /// <summary>
    /// Factory for generating Eway configurations for use in tests.
    /// </summary>
    public static class TestEwayConfigurationFactory
    {
        public static EwayConfiguration CreateWithValidCredentials() => new EwayConfiguration(
                EwayEndpoint.Sandbox,
                "A1001C0it5LRbK7UhbQnF2IpQKUCGTvTC6TH2qFYf71tATFW6QVa2YjFqahktN3fq8xMgl",
                "XHkEk4WJ",
                "ryj1MAyYOwZosMi+SZXoV4hOW95BuDF8kA5zEW6JBD5S9l0cogdt0Hs5K9h0wKlxh6O8dAHgsiTCxvmz0pbKXGVyqSjq1PpjOtMveC4N5C8IGosC1ueLPWUdivfu9T3XjZ9siFYh30Y8oVL3f9eb4KnGvzYJ5UaRk1mUAZL/Ln6MX6op6pHGw2HVbi4GFUq3OfLMzM+hsFMfhGIDNeDitOTNFn1eH2rTNZJkAinCiQ86vT6aX7tHDZIYslukwOKWTaN/kI6fgmdab4LaxE5tErRcM1uhwVnI4UlQqp4uHC/vYS31wQ+1AYftlMG3h1QIFtgww1Ydo8NA4ibLXCluvQ==",
                "https://api.sandbox.ewaypayments.com/encrypt",
                "epk-CC20D9EB-A5AF-4193-8033-CD6E00DB6F03");

        public static EwayConfiguration CreateWithInvalidCredentials() => new EwayConfiguration(
                EwayEndpoint.Sandbox,
                "A1001C0it5LRbK7UhbQnF2IpQKUCGTvTC6TH2qFYf71tATFW6QVa2YjFqahktN3fq8xMgl",
                "QZJD8km6",
                "ryj1MAyYOwZosMi+SZXoV4hOW95BuDF8kA5zEW6JBD5S9l0cogdt0Hs5K9h0wKlxh6O8dAHgsiTCxvmz0pbKXGVyqSjq1PpjOtMveC4N5C8IGosC1ueLPWUdivfu9T3XjZ9siFYh30Y8oVL3f9eb4KnGvzYJ5UaRk1mUAZL/Ln6MX6op6pHGw2HVbi4GFUq3OfLMzM+hsFMfhGIDNeDitOTNFn1eH2rTNZJkAinCiQ86vT6aX7tHDZIYslukwOKWTaN/kI6fgmdab4LaxE5tErRcM1uhwVnI4UlQqp4uHC/vYS31wQ+1AYftlMG3h1QIFtgww1Ydo8NA4ibLXCluvQ==",
                "https://api.sandbox.ewaypayments.com/encrypt",
                "epk-CC20D9EB-A5AF-4193-8033-CD6E00DB6F03");
    }
}
